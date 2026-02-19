using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

// Alias to avoid conflict: namespace "NonConformity" vs class "NonConformity"
using NC = Networg.ConstructSafe.Plugins.Common.NonConformity;

namespace Networg.ConstructSafe.Plugins.NonConformity
{
    /// <summary>
    /// Plugin: Sends email notification to Assigned Manager
    /// when a Non-Conformity record is Created or Updated.
    /// 
    /// Registration:
    ///   Message:  Create / Update
    ///   Entity:   new_nonconformity
    ///   Stage:    PostOperation (40)
    ///   Mode:     Asynchronous
    ///   
    /// Filtering Attributes (Update only):
    ///   new_status, new_severity, new_assignedmanager
    /// </summary>
    public class NotificationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // ── Obtain services ──────────────────────────────────
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(null); // SYSTEM context for sending email
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                tracingService.Trace("[NotificationPlugin] Executing...");

                // ── Validate context ─────────────────────────────
                if (context.InputParameters == null ||
                    !context.InputParameters.Contains("Target") ||
                    !(context.InputParameters["Target"] is Entity targetEntity))
                {
                    tracingService.Trace("[NotificationPlugin] No Target entity found. Exiting.");
                    return;
                }

                // ── Determine if this is Create or Update ────────
                bool isCreate = context.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase);
                bool isUpdate = context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase);

                if (!isCreate && !isUpdate) return;

                // ── Get full record (Target may not have all fields) ─
                var ncRecord = service.Retrieve(
                    NC.EntityName,
                    targetEntity.Id,
                    new ColumnSet(
                        NC.Attributes.Name,
                        NC.Attributes.Type,
                        NC.Attributes.Severity,
                        NC.Attributes.Status,
                        NC.Attributes.Location,
                        NC.Attributes.TicketNumber,
                        NC.Attributes.AssignedManager,
                        NC.Attributes.Owner
                    )
                );

                // ── Check if Assigned Manager exists ─────────────
                var managerRef = ncRecord.GetAttributeValue<EntityReference>(NC.Attributes.AssignedManager);

                if (managerRef == null)
                {
                    tracingService.Trace("[NotificationPlugin] No Assigned Manager. Skipping notification.");
                    return;
                }

                // ── Build notification content ───────────────────
                string ncName = ncRecord.GetAttributeValue<string>(NC.Attributes.Name) ?? "N/A";
                string ticketNumber = ncRecord.GetAttributeValue<string>(NC.Attributes.TicketNumber) ?? "";
                string location = ncRecord.GetAttributeValue<string>(NC.Attributes.Location) ?? "Not specified";

                string typeLabel = GetOptionSetLabel(ncRecord, NC.Attributes.Type);
                string severityLabel = GetOptionSetLabel(ncRecord, NC.Attributes.Severity);
                string statusLabel = GetOptionSetLabel(ncRecord, NC.Attributes.Status);

                string action = isCreate ? "NEW" : "UPDATED";
                string subject = string.Format("[ConstructSafe] {0}: Non-Conformity {1} - {2}", action, ticketNumber, ncName);

                string body = string.Format(@"
<html>
<body style='font-family: Segoe UI, Arial, sans-serif; color: #333;'>
    <h2 style='color: #c0392b;'>⚠️ Non-Conformity {0}</h2>
    <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
        <tr style='background: #f8f9fa;'>
            <td style='padding: 8px; font-weight: bold; border: 1px solid #dee2e6;'>Ticket</td>
            <td style='padding: 8px; border: 1px solid #dee2e6;'>{1}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold; border: 1px solid #dee2e6;'>Name</td>
            <td style='padding: 8px; border: 1px solid #dee2e6;'>{2}</td>
        </tr>
        <tr style='background: #f8f9fa;'>
            <td style='padding: 8px; font-weight: bold; border: 1px solid #dee2e6;'>Type</td>
            <td style='padding: 8px; border: 1px solid #dee2e6;'>{3}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold; border: 1px solid #dee2e6;'>Severity</td>
            <td style='padding: 8px; border: 1px solid #dee2e6;'><strong>{4}</strong></td>
        </tr>
        <tr style='background: #f8f9fa;'>
            <td style='padding: 8px; font-weight: bold; border: 1px solid #dee2e6;'>Status</td>
            <td style='padding: 8px; border: 1px solid #dee2e6;'>{5}</td>
        </tr>
        <tr>
            <td style='padding: 8px; font-weight: bold; border: 1px solid #dee2e6;'>Location</td>
            <td style='padding: 8px; border: 1px solid #dee2e6;'>{6}</td>
        </tr>
    </table>
    <br/>
    <p>Please review and take appropriate action.</p>
    <p style='color: #888; font-size: 12px;'>This notification was generated automatically by ConstructSafe.</p>
</body>
</html>", action, ticketNumber, ncName, typeLabel, severityLabel, statusLabel, location);

                // ── Create and send email ────────────────────────
                SendNotificationEmail(service, tracingService, context.UserId, managerRef, subject, body);

                tracingService.Trace("[NotificationPlugin] Notification sent successfully to {0}.", managerRef.Name);
            }
            catch (Exception ex)
            {
                tracingService.Trace("[NotificationPlugin] Error: {0}", ex.ToString());
                // Don't throw – notification failure should not block the save
            }
        }

        /// <summary>
        /// Creates an Email activity and sends it via Dataverse.
        /// </summary>
        private void SendNotificationEmail(
            IOrganizationService service,
            ITracingService tracingService,
            Guid senderId,
            EntityReference recipientRef,
            string subject,
            string body)
        {
            // FROM: System user (plugin executor)
            var fromParty = new Entity("activityparty");
            fromParty["partyid"] = new EntityReference("systemuser", senderId);

            // TO: Assigned Manager
            var toParty = new Entity("activityparty");
            toParty["partyid"] = recipientRef;

            // Create Email entity
            var email = new Entity("email");
            email["from"] = new Entity[] { fromParty };
            email["to"] = new Entity[] { toParty };
            email["subject"] = subject;
            email["description"] = body;
            email["directioncode"] = true; // Outgoing

            Guid emailId = service.Create(email);
            tracingService.Trace("[NotificationPlugin] Email activity created: {0}", emailId);

            // Send the email
            var sendRequest = new OrganizationRequest("SendEmail");
            sendRequest["EmailId"] = emailId;
            sendRequest["IssueSend"] = true;
            sendRequest["TrackingToken"] = "";

            service.Execute(sendRequest);
        }

        /// <summary>
        /// Gets the display label for an OptionSet value.
        /// </summary>
        private string GetOptionSetLabel(Entity record, string attributeName)
        {
            var optionSet = record.GetAttributeValue<OptionSetValue>(attributeName);
            if (optionSet == null) return "N/A";

            // Use formatted values if available (populated by Retrieve)
            if (record.FormattedValues.ContainsKey(attributeName))
            {
                return record.FormattedValues[attributeName];
            }

            return optionSet.Value.ToString();
        }
    }
}
