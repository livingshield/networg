namespace Networg.ConstructSafe.Plugins.Common
{
    // ==========================================================
    // GLOBAL OPTION SETS (Číselníky / Volby)
    // ==========================================================

    /// <summary>
    /// Non Conformity Type (new_nonconformitytype)
    /// </summary>
    public enum NonConformityType
    {
        Safety = 100000000,
        Quality = 100000001,
        Environmental = 100000002,
        Documentation = 100000003,
        Other = 100000004
    }

    /// <summary>
    /// Non-Conformity Severity (new_nonconformityseverity)
    /// </summary>
    public enum NonConformitySeverity
    {
        Low = 100000000,
        Medium = 100000001,
        High = 100000002,
        Critical = 100000003
    }

    /// <summary>
    /// Non-Conformity Priority (new_nonconformitypriority)
    /// </summary>
    public enum NonConformityPriority
    {
        Low = 100000000,
        Medium = 100000001,
        High = 100000002
    }

    /// <summary>
    /// Non-Conformity Status (new_nonconformitystatus)
    /// </summary>
    public enum NonConformityStatus
    {
        Open = 100000000,
        InProgress = 100000001,
        Resolved = 100000002,
        Closed = 100000003
    }

    /// <summary>
    /// Non-Conformity File Type (new_nonconformityfiletype)
    /// </summary>
    public enum NonConformityFileType
    {
        Photo = 100000000,
        Document = 100000001,
        Video = 100000002,
        Other = 100000003
    }

    // ==========================================================
    // ENTITY DEFINITIONS
    // ==========================================================

    /// <summary>
    /// Constants for the Non-Conformity entity (new_nonconformity)
    /// </summary>
    public static class NonConformity
    {
        public const string EntityName = "new_nonconformity";
        public const string PrimaryIdAttribute = "new_nonconformityid";
        public const string PrimaryNameAttribute = "new_name";

        public struct Attributes
        {
            public const string Name = "new_name";
            public const string Description = "new_description";
            public const string Type = "new_type";                     // Global OptionSet: new_nonconformitytype
            public const string Severity = "new_severity";             // Global OptionSet: new_nonconformityseverity
            public const string Status = "new_status";                 // Global OptionSet: new_nonconformitystatus
            public const string DateReported = "new_datereported";
            public const string Location = "new_location";
            public const string TicketNumber = "new_ticketnumber";
            public const string AssignedManager = "new_assignedmanager"; // Lookup → User
            public const string Owner = "ownerid";
        }
    }

    /// <summary>
    /// Constants for the Corrective Action entity (new_correctiveaction)
    /// </summary>
    public static class CorrectiveAction
    {
        public const string EntityName = "new_correctiveaction";
        public const string PrimaryIdAttribute = "new_correctiveactionid";

        public struct Attributes
        {
            public const string Name = "new_name";
            public const string Description = "new_description";
            public const string DueDate = "new_duedate";
            public const string NonConformity = "new_nonconformity";   // Lookup → Non-Conformity
            public const string Notes = "new_notes";
            public const string Priority = "new_priority";             // Global OptionSet: new_nonconformitypriority
            public const string Status = "new_status";                 // Global OptionSet: new_nonconformitystatus
        }
    }

    /// <summary>
    /// Constants for the Evidence entity (new_evidence)
    /// </summary>
    public static class Evidence
    {
        public const string EntityName = "new_evidence";
        public const string PrimaryIdAttribute = "new_evidenceid";

        public struct Attributes
        {
            public const string Name = "new_name";
            public const string NonConformity = "new_nonconformity";   // Lookup → Non-Conformity
            public const string FileType = "new_filetype";             // Global OptionSet: new_nonconformityfiletype
            public const string Attachment = "new_attachment";         // File
            public const string Notes = "new_notes";
        }
    }
}
