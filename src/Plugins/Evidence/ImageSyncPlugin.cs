using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

using Ev = Networg.ConstructSafe.Plugins.Common.Evidence;

namespace Networg.ConstructSafe.Plugins.Evidence
{
    /// <summary>
    /// Plugin: Bidirectional sync between File column (Attachment) and Image column (ImagePreview).
    ///
    /// Uses a reconciliation approach: on every trigger, checks the current state of both
    /// columns and ensures consistency.
    ///
    /// Logic:
    ///   1. Try to read Attachment (File) → if image exists, ensure ImagePreview matches
    ///   2. If Attachment is empty but ImagePreview has data → copy ImagePreview to Attachment
    ///   3. If Attachment is not an image → clear ImagePreview
    ///   4. If both empty → do nothing
    ///
    /// Registration:
    ///   Message:  Create, Update
    ///   Entity:   new_evidence
    ///   Stage:    PostOperation (40)
    ///   Mode:     Synchronous
    ///   Filtering Attributes (Update): ALL
    /// </summary>
    public class ImageSyncPlugin : IPlugin
    {
        private const int FileTypePhoto = 100000000;

        private static readonly HashSet<string> ImageExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(
                typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(
                typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(null);
            var tracingService = (ITracingService)serviceProvider.GetService(
                typeof(ITracingService));

            try
            {
                tracingService.Trace("[ImageSyncPlugin] Starting... Depth={0}, Message={1}",
                    context.Depth, context.MessageName);

                // Anti-loop: skip if triggered by our own Update
                if (context.Depth > 1)
                {
                    tracingService.Trace("[ImageSyncPlugin] Depth > 1, skipping.");
                    return;
                }

                if (!(context.InputParameters["Target"] is Entity targetEntity))
                {
                    tracingService.Trace("[ImageSyncPlugin] No Target entity. Exiting.");
                    return;
                }

                Guid evidenceId = targetEntity.Id;
                tracingService.Trace("[ImageSyncPlugin] Processing Evidence: {0}", evidenceId);

                // ========================================
                // RECONCILIATION: check state of both columns
                // ========================================

                // Step 1: Try to read Attachment (File column)
                bool hasAttachment = false;
                bool attachmentIsImage = false;
                byte[] attachmentBytes = null;

                try
                {
                    attachmentBytes = DownloadFileColumn(service, evidenceId, tracingService);
                    if (attachmentBytes != null && attachmentBytes.Length > 0)
                    {
                        hasAttachment = true;
                        attachmentIsImage = IsFileAnImage(targetEntity, service, evidenceId, tracingService);
                        tracingService.Trace("[ImageSyncPlugin] Attachment: exists={0}, isImage={1}, size={2}",
                            hasAttachment, attachmentIsImage, attachmentBytes.Length);
                    }
                }
                catch (Exception ex)
                {
                    tracingService.Trace("[ImageSyncPlugin] No attachment or error reading: {0}", ex.Message);
                    hasAttachment = false;
                }

                // Step 2: Read ImagePreview (Image column)
                var fullRecord = service.Retrieve(Ev.EntityName, evidenceId,
                    new Microsoft.Xrm.Sdk.Query.ColumnSet(Ev.Attributes.ImagePreview));
                byte[] imagePreviewBytes = fullRecord.GetAttributeValue<byte[]>(Ev.Attributes.ImagePreview);
                bool hasImagePreview = imagePreviewBytes != null && imagePreviewBytes.Length > 0;

                tracingService.Trace("[ImageSyncPlugin] State: hasAttachment={0}, attachmentIsImage={1}, hasImagePreview={2}",
                    hasAttachment, attachmentIsImage, hasImagePreview);

                // ========================================
                // SYNC DECISIONS
                // ========================================

                if (hasAttachment && attachmentIsImage && !hasImagePreview)
                {
                    // Case A: Attachment has image, ImagePreview empty → copy to ImagePreview
                    tracingService.Trace("[ImageSyncPlugin] Case A: Attachment→ImagePreview");
                    WriteImagePreview(service, evidenceId, attachmentBytes, tracingService);
                }
                else if (hasAttachment && attachmentIsImage && hasImagePreview)
                {
                    // Case B: Both have data. Check if attachment changed (different size = update preview)
                    if (attachmentBytes.Length != imagePreviewBytes.Length)
                    {
                        tracingService.Trace("[ImageSyncPlugin] Case B: Attachment changed→update ImagePreview");
                        WriteImagePreview(service, evidenceId, attachmentBytes, tracingService);
                    }
                    else
                    {
                        tracingService.Trace("[ImageSyncPlugin] Case B: Both in sync, nothing to do.");
                    }
                }
                else if (hasAttachment && !attachmentIsImage && hasImagePreview)
                {
                    // Case C: Attachment is not an image (PDF etc.) → clear ImagePreview
                    tracingService.Trace("[ImageSyncPlugin] Case C: Non-image attachment→clear ImagePreview");
                    ClearImagePreview(service, evidenceId, tracingService);
                }
                else if (!hasAttachment && hasImagePreview)
                {
                    // Case D: No Attachment but ImagePreview has data
                    // Always copy ImagePreview → Attachment (Canvas App upload or re-sync)
                    // ImagePreview is the "source of truth" when Attachment is empty
                    tracingService.Trace("[ImageSyncPlugin] Case D: ImagePreview→Attachment (no attachment found)");
                    CopyImagePreviewToAttachment(service, evidenceId, imagePreviewBytes, tracingService);
                    SetFileTypeIfEmpty(service, evidenceId, tracingService);
                }
                else if (!hasAttachment && !hasImagePreview)
                {
                    // Case E: Both empty → nothing to do
                    tracingService.Trace("[ImageSyncPlugin] Case E: Both empty, nothing to do.");
                }

                tracingService.Trace("[ImageSyncPlugin] Done.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("[ImageSyncPlugin] Error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    string.Format("Image sync failed: {0}", ex.Message), ex);
            }
        }

        // =====================================================
        // FILE OPERATIONS
        // =====================================================

        /// <summary>
        /// Downloads the entire file from the Attachment (File) column.
        /// Returns null if no file exists.
        /// </summary>
        private byte[] DownloadFileColumn(
            IOrganizationService service,
            Guid evidenceId,
            ITracingService tracingService)
        {
            var initRequest = new OrganizationRequest("InitializeFileBlocksDownload")
            {
                ["Target"] = new EntityReference(Ev.EntityName, evidenceId),
                ["FileAttributeName"] = Ev.Attributes.Attachment
            };
            var initResponse = service.Execute(initRequest);

            string token = (string)initResponse.Results["FileContinuationToken"];
            long fileSize = (long)initResponse.Results["FileSizeInBytes"];

            if (fileSize == 0) return null;

            tracingService.Trace("[ImageSyncPlugin] Downloading {0} bytes...", fileSize);

            var fileBytes = new List<byte>();
            long offset = 0;
            long blockSize = 4 * 1024 * 1024;

            while (offset < fileSize)
            {
                var dlRequest = new OrganizationRequest("DownloadBlock")
                {
                    ["FileContinuationToken"] = token,
                    ["Offset"] = offset,
                    ["BlockLength"] = blockSize
                };
                var dlResponse = service.Execute(dlRequest);
                byte[] block = (byte[])dlResponse.Results["Data"];
                fileBytes.AddRange(block);
                offset += block.Length;
            }

            return fileBytes.ToArray();
        }

        /// <summary>
        /// Uploads image bytes to the Attachment (File) column.
        /// </summary>
        private void CopyImagePreviewToAttachment(
            IOrganizationService service,
            Guid evidenceId,
            byte[] imageData,
            ITracingService tracingService)
        {
            tracingService.Trace("[ImageSyncPlugin] Uploading {0} bytes to Attachment...", imageData.Length);

            try
            {
                var initRequest = new OrganizationRequest("InitializeFileBlocksUpload")
                {
                    ["Target"] = new EntityReference(Ev.EntityName, evidenceId),
                    ["FileAttributeName"] = Ev.Attributes.Attachment,
                    ["FileName"] = "evidence.jpg"
                };
                var initResponse = service.Execute(initRequest);
                string token = (string)initResponse.Results["FileContinuationToken"];

                int blockSize = 4 * 1024 * 1024;
                int offset = 0;
                int blockNum = 0;
                var blockIds = new List<string>();

                while (offset < imageData.Length)
                {
                    int currentSize = Math.Min(blockSize, imageData.Length - offset);
                    byte[] block = new byte[currentSize];
                    Array.Copy(imageData, offset, block, 0, currentSize);

                    string blockId = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(blockNum.ToString("D6")));
                    blockIds.Add(blockId);

                    var uploadRequest = new OrganizationRequest("UploadBlock")
                    {
                        ["FileContinuationToken"] = token,
                        ["BlockData"] = block,
                        ["BlockId"] = blockId
                    };
                    service.Execute(uploadRequest);

                    offset += currentSize;
                    blockNum++;
                }

                var commitRequest = new OrganizationRequest("CommitFileBlocksUpload")
                {
                    ["FileContinuationToken"] = token,
                    ["FileName"] = "evidence.jpg",
                    ["MimeType"] = "image/jpeg",
                    ["BlockList"] = blockIds.ToArray()
                };
                service.Execute(commitRequest);

                tracingService.Trace("[ImageSyncPlugin] Attachment uploaded successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("[ImageSyncPlugin] Failed to upload: {0}", ex.Message);
            }
        }

        // =====================================================
        // IMAGE PREVIEW OPERATIONS
        // =====================================================

        private void WriteImagePreview(
            IOrganizationService service,
            Guid evidenceId,
            byte[] imageData,
            ITracingService tracingService)
        {
            tracingService.Trace("[ImageSyncPlugin] Writing ImagePreview ({0} bytes)...", imageData.Length);
            var update = new Entity(Ev.EntityName, evidenceId);
            update[Ev.Attributes.ImagePreview] = imageData;
            service.Update(update);
            tracingService.Trace("[ImageSyncPlugin] ImagePreview written.");
        }

        private void ClearImagePreview(
            IOrganizationService service,
            Guid evidenceId,
            ITracingService tracingService)
        {
            tracingService.Trace("[ImageSyncPlugin] Clearing ImagePreview...");
            var update = new Entity(Ev.EntityName, evidenceId);
            update[Ev.Attributes.ImagePreview] = null;
            service.Update(update);
            tracingService.Trace("[ImageSyncPlugin] ImagePreview cleared.");
        }

        // =====================================================
        // HELPERS
        // =====================================================

        /// <summary>
        /// Checks if the current file is an image based on FileType OptionSet or file extension.
        /// </summary>
        private bool IsFileAnImage(
            Entity targetEntity,
            IOrganizationService service,
            Guid evidenceId,
            ITracingService tracingService)
        {
            OptionSetValue fileType = targetEntity.GetAttributeValue<OptionSetValue>(Ev.Attributes.FileType);

            if (fileType == null)
            {
                var record = service.Retrieve(Ev.EntityName, evidenceId,
                    new Microsoft.Xrm.Sdk.Query.ColumnSet(Ev.Attributes.FileType));
                fileType = record.GetAttributeValue<OptionSetValue>(Ev.Attributes.FileType);
            }

            if (fileType != null)
            {
                return fileType.Value == FileTypePhoto;
            }

            // Fallback: check file name extension
            try
            {
                var initRequest = new OrganizationRequest("InitializeFileBlocksDownload")
                {
                    ["Target"] = new EntityReference(Ev.EntityName, evidenceId),
                    ["FileAttributeName"] = Ev.Attributes.Attachment
                };
                var resp = service.Execute(initRequest);
                string fileName = (string)resp.Results["FileName"];
                if (!string.IsNullOrEmpty(fileName))
                {
                    return ImageExtensions.Contains(System.IO.Path.GetExtension(fileName));
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// Sets FileType to Photo if currently empty (Canvas App upload scenario).
        /// </summary>
        private void SetFileTypeIfEmpty(
            IOrganizationService service,
            Guid evidenceId,
            ITracingService tracingService)
        {
            var record = service.Retrieve(Ev.EntityName, evidenceId,
                new Microsoft.Xrm.Sdk.Query.ColumnSet(Ev.Attributes.FileType));
            var ft = record.GetAttributeValue<OptionSetValue>(Ev.Attributes.FileType);

            if (ft == null)
            {
                tracingService.Trace("[ImageSyncPlugin] Setting FileType to Photo.");
                var update = new Entity(Ev.EntityName, evidenceId);
                update[Ev.Attributes.FileType] = new OptionSetValue(FileTypePhoto);
                service.Update(update);
            }
        }
    }
}
