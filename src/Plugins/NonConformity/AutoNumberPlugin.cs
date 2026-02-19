using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

// Alias to avoid conflict: namespace "NonConformity" vs class "NonConformity"
using NC = Networg.ConstructSafe.Plugins.Common.NonConformity;

namespace Networg.ConstructSafe.Plugins.NonConformity
{
    /// <summary>
    /// Plugin: Auto-generates Ticket Number for new Non-Conformity records.
    /// Format: NC-XXXXX (e.g. NC-00001, NC-00002, ...)
    /// 
    /// Registration:
    ///   Message:  Create
    ///   Entity:   new_nonconformity
    ///   Stage:    PreOperation (20)
    ///   Mode:     Synchronous
    /// </summary>
    public class AutoNumberPlugin : IPlugin
    {
        private const string Prefix = "NC";

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(null);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                tracingService.Trace("[AutoNumberPlugin] Starting...");

                if (!context.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase))
                {
                    tracingService.Trace("[AutoNumberPlugin] Not a Create message. Exiting.");
                    return;
                }

                if (!(context.InputParameters["Target"] is Entity targetEntity))
                {
                    tracingService.Trace("[AutoNumberPlugin] No Target entity. Exiting.");
                    return;
                }

                // Skip if ticket number is already set
                string existingTicket = targetEntity.GetAttributeValue<string>(NC.Attributes.TicketNumber);
                if (!string.IsNullOrEmpty(existingTicket))
                {
                    tracingService.Trace("[AutoNumberPlugin] Ticket already set: {0}. Skipping.", existingTicket);
                    return;
                }

                tracingService.Trace("[AutoNumberPlugin] Querying for last ticket number...");

                // Find the highest existing ticket number
                var query = new QueryExpression(NC.EntityName)
                {
                    ColumnSet = new ColumnSet(NC.Attributes.TicketNumber),
                    TopCount = 1,
                    Orders =
                    {
                        new OrderExpression("createdon", OrderType.Descending)
                    },
                    Criteria =
                    {
                        Conditions =
                        {
                            new ConditionExpression(
                                NC.Attributes.TicketNumber,
                                ConditionOperator.NotNull
                            ),
                            new ConditionExpression(
                                NC.Attributes.TicketNumber,
                                ConditionOperator.BeginsWith,
                                Prefix + "-"
                            )
                        }
                    }
                };

                var results = service.RetrieveMultiple(query);
                int nextNumber = 1;

                tracingService.Trace("[AutoNumberPlugin] Found {0} existing records with ticket numbers.", results.Entities.Count);

                if (results.Entities.Count > 0)
                {
                    string lastTicket = results.Entities[0]
                        .GetAttributeValue<string>(NC.Attributes.TicketNumber);

                    tracingService.Trace("[AutoNumberPlugin] Last ticket: {0}", lastTicket ?? "null");

                    if (!string.IsNullOrEmpty(lastTicket))
                    {
                        // Parse "NC-00042" → 42
                        string[] parts = lastTicket.Split('-');
                        if (parts.Length >= 2 && int.TryParse(parts[parts.Length - 1], out int lastNum))
                        {
                            nextNumber = lastNum + 1;
                        }
                    }
                }

                // Format: NC-00001
                string ticketNumber = string.Format("{0}-{1}", Prefix, nextNumber.ToString("D5"));

                tracingService.Trace("[AutoNumberPlugin] Setting ticket number: {0}", ticketNumber);

                // Set on the Target entity (PreOperation = modifies before save)
                targetEntity[NC.Attributes.TicketNumber] = ticketNumber;

                tracingService.Trace("[AutoNumberPlugin] Done.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("[AutoNumberPlugin] Error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    string.Format("Auto-numbering failed: {0}", ex.Message), ex);
            }
        }
    }
}
