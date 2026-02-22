"use strict";
/**
 * Evidence Form Script
 *
 * Handles the bidirectional sync trigger for ImagePreview ↔ Attachment.
 * When the user changes the ImagePreview (Image) field, this script
 * touches the Notes field to ensure the Dataverse Update message fires,
 * which triggers the ImageSyncPlugin to copy data to the File column.
 *
 * @module EvidenceForm
 * @author Jan Kytyr
 */
var ConstructSafe;
(function (ConstructSafe) {
    var Evidence;
    (function (Evidence) {
        // =====================================================
        // CONSTANTS
        // =====================================================
        const Fields = {
            IMAGE_PREVIEW: "new_imagepreview",
            NOTES: "new_notes",
            FILE_TYPE: "new_filetype",
            ATTACHMENT: "new_attachment",
            NAME: "new_name"
        };
        // Timestamp marker prefix used to detect plugin-trigger touches
        const SYNC_MARKER = "[ImageSync]";
        // =====================================================
        // FORM EVENT HANDLERS
        // =====================================================
        /**
         * Called on form load. Registers event handlers.
         *
         * Registration:
         *   Form Event: OnLoad
         *   Function: ConstructSafe.Evidence.onLoad
         *   Pass execution context: Yes
         */
        function onLoad(executionContext) {
            const formContext = executionContext.getFormContext();
            // Register onChange handler for ImagePreview field
            const imagePreviewAttr = formContext.getAttribute(Fields.IMAGE_PREVIEW);
            if (imagePreviewAttr) {
                imagePreviewAttr.addOnChange(onImagePreviewChange);
                console.log("[ConstructSafe] Evidence: ImagePreview onChange registered.");
            }
            else {
                console.warn("[ConstructSafe] Evidence: ImagePreview field not found on form.");
            }
            console.log("[ConstructSafe] Evidence form loaded successfully.");
        }
        Evidence.onLoad = onLoad;
        /**
         * Called when ImagePreview field value changes.
         * Touches the Notes field to make the form dirty, ensuring
         * the Update message fires and triggers ImageSyncPlugin.
         */
        function onImagePreviewChange(executionContext) {
            const formContext = executionContext.getFormContext();
            console.log("[ConstructSafe] Evidence: ImagePreview changed, triggering sync...");
            // Touch the Notes field to force form dirty state
            const notesAttr = formContext.getAttribute(Fields.NOTES);
            if (notesAttr) {
                const currentNotes = notesAttr.getValue() || "";
                // Add/update sync marker with timestamp
                const timestamp = new Date().toISOString();
                let newNotes;
                if (currentNotes.indexOf(SYNC_MARKER) >= 0) {
                    // Replace existing marker
                    newNotes = currentNotes.replace(/\[ImageSync\].*$/, SYNC_MARKER + " " + timestamp);
                }
                else {
                    // Append marker
                    newNotes = currentNotes
                        ? currentNotes + "\n" + SYNC_MARKER + " " + timestamp
                        : SYNC_MARKER + " " + timestamp;
                }
                notesAttr.setValue(newNotes);
                notesAttr.setSubmitMode("always");
                console.log("[ConstructSafe] Evidence: Notes touched with sync marker.");
            }
            else {
                console.warn("[ConstructSafe] Evidence: Notes field not found, cannot trigger sync.");
            }
        }
    })(Evidence = ConstructSafe.Evidence || (ConstructSafe.Evidence = {}));
})(ConstructSafe || (ConstructSafe = {}));
//# sourceMappingURL=evidence.form.js.map