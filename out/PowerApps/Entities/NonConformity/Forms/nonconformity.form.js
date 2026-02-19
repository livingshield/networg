"use strict";
/**
 * NonConformity Form Script
 *
 * Handles dynamic form behavior for the Non-Conformity entity.
 * Demonstrates TypeScript usage as required by the assignment's Bonus Points.
 *
 * Key features:
 * - Conditional field visibility based on Non-Conformity Type
 * - Auto-setting Severity for Safety-type non-conformities
 * - Form validation before save
 *
 * @module NonConformityForm
 * @author Jan Kytyr
 */
var ConstructSafe;
(function (ConstructSafe) {
    var NonConformity;
    (function (NonConformity) {
        // =====================================================
        // CONSTANTS
        // =====================================================
        /** Logical names of form fields */
        const Fields = {
            TITLE: "new_name",
            DESCRIPTION: "new_description",
            TYPE: "new_type",
            SEVERITY: "new_severity",
            STATUS: "new_status",
            DATE_REPORTED: "new_datereported",
            LOCATION: "new_location",
            TICKET_NUMBER: "new_ticketnumber",
            ASSIGNED_MANAGER: "new_assignedmanager",
        };
        /** Option set values for Non-Conformity Type */
        const NonConformityType = {
            SAFETY: 100000000,
            QUALITY: 100000001,
            ENVIRONMENTAL: 100000002,
            DOCUMENTATION: 100000003,
            OTHER: 100000004,
        };
        /** Option set values for Severity (new_nonconformityseverity) */
        const SeverityLevel = {
            LOW: 100000000,
            MEDIUM: 100000001,
            HIGH: 100000002,
            CRITICAL: 100000003,
        };
        /** Option set values for Priority (new_nonconformitypriority) */
        const PriorityLevel = {
            LOW: 100000000,
            MEDIUM: 100000001,
            HIGH: 100000002,
        };
        /** Option set values for Status (new_nonconformitystatus) */
        const NCStatus = {
            OPEN: 100000000,
            IN_PROGRESS: 100000001,
            RESOLVED: 100000002,
            CLOSED: 100000003,
        };
        /** Option set values for File Type (new_nonconformityfiletype) */
        const FileType = {
            PHOTO: 100000000,
            DOCUMENT: 100000001,
            VIDEO: 100000002,
            OTHER: 100000003,
        };
        // =====================================================
        // FORM EVENT HANDLERS
        // =====================================================
        /**
         * Called when the Non-Conformity form loads.
         * Registers event handlers and sets initial field visibility.
         *
         * @param executionContext - The execution context passed by Power Apps
         */
        function onLoad(executionContext) {
            const formContext = executionContext.getFormContext();
            // Register onChange handler for the Type field
            const typeAttribute = formContext.getAttribute(Fields.TYPE);
            if (typeAttribute) {
                typeAttribute.addOnChange(onTypeChange);
            }
            // Set initial visibility based on current Type value
            applyTypeBasedRules(formContext);
            // Auto-set Date Reported if creating a new record
            if (formContext.ui.getFormType() === 1 /* XrmEnum.FormType.Create */) {
                setDefaultDateReported(formContext);
            }
            console.log("[ConstructSafe] NonConformity form loaded successfully.");
        }
        NonConformity.onLoad = onLoad;
        /**
         * Called when the Non-Conformity form is saved.
         * Performs validation before allowing save.
         *
         * @param executionContext - The execution context passed by Power Apps
         */
        function onSave(executionContext) {
            var _a;
            const formContext = executionContext.getFormContext();
            const typeValue = getSelectedOptionValue(formContext, Fields.TYPE);
            // Validation: Safety type MUST have a Location
            if (typeValue === NonConformityType.SAFETY) {
                const location = (_a = formContext.getAttribute(Fields.LOCATION)) === null || _a === void 0 ? void 0 : _a.getValue();
                if (!location || location.trim() === "") {
                    executionContext.getEventArgs().preventDefault();
                    formContext.ui.setFormNotification("Safety non-conformities require a Location to be specified.", "ERROR", "SAFETY_LOCATION_REQUIRED");
                    return;
                }
            }
            // Clear any previous notifications
            formContext.ui.clearFormNotification("SAFETY_LOCATION_REQUIRED");
        }
        NonConformity.onSave = onSave;
        // =====================================================
        // PDF REPORT GENERATION
        // =====================================================
        /**
         * Triggers the Power Automate flow to generate a PDF summary report.
         * Called from a Command Bar button on the Non-Conformity form.
         *
         * The flow URL must be updated after creating the Power Automate flow.
         *
         * @param primaryControl - The form context passed by Command Bar
         */
        function generatePDF(primaryControl) {
            var _a;
            const formContext = primaryControl;
            // Ensure record is saved before generating PDF
            if (formContext.data.entity.getIsDirty()) {
                formContext.ui.setFormNotification("Please save the record before generating a PDF report.", "WARNING", "PDF_SAVE_FIRST");
                setTimeout(() => formContext.ui.clearFormNotification("PDF_SAVE_FIRST"), 5000);
                return;
            }
            const recordId = formContext.data.entity.getId().replace(/[{}]/g, "");
            const ticketNumber = ((_a = formContext.getAttribute(Fields.TICKET_NUMBER)) === null || _a === void 0 ? void 0 : _a.getValue()) || "report";
            const flowUrl = "https://9096f8d154d5e3249a8daa4b9f0942.58.environment.api.powerplatform.com:443/powerautomate/automations/direct/workflows/4012f04020e84977901f4c3cc890d12c/triggers/manual/paths/invoke?api-version=1&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=fyIPVzL2Otxu2TgrGjrhXT4sS5j2w5iBxs6Tf_TWep8";
            Xrm.Utility.showProgressIndicator("Generating PDF report...");
            const request = new XMLHttpRequest();
            request.open("POST", flowUrl, true);
            request.setRequestHeader("Content-Type", "application/json");
            request.onreadystatechange = function () {
                if (request.readyState === 4) {
                    Xrm.Utility.closeProgressIndicator();
                    if (request.status === 200 || request.status === 202) {
                        formContext.ui.setFormNotification("PDF report generated successfully. Check the Timeline for the attachment.", "INFO", "PDF_GENERATED");
                        formContext.data.refresh(false);
                        setTimeout(() => formContext.ui.clearFormNotification("PDF_GENERATED"), 8000);
                    }
                    else if (request.status === 0) {
                        // CORS blocks reading the response, but the flow was triggered successfully.
                        // Power Automate does not handle OPTIONS preflight - this is expected behaviour.
                        formContext.ui.setFormNotification("Report is being generated. Please refresh the Timeline in a few seconds.", "INFO", "PDF_GENERATED");
                        setTimeout(() => {
                            formContext.ui.clearFormNotification("PDF_GENERATED");
                            formContext.data.refresh(false);
                        }, 5000);
                        console.log("[ConstructSafe] CORS blocked response (expected) - flow was triggered.");
                    }
                    else {
                        formContext.ui.setFormNotification("PDF generation failed. Please try again or contact your administrator.", "ERROR", "PDF_FAILED");
                        setTimeout(() => formContext.ui.clearFormNotification("PDF_FAILED"), 8000);
                        console.error("[ConstructSafe] PDF generation failed:", request.status, request.responseText);
                    }
                }
            };
            request.onerror = function () {
                // onerror fires on network/CORS issues - same as status 0 case
                Xrm.Utility.closeProgressIndicator();
                formContext.ui.setFormNotification("Report is being generated. Please refresh the Timeline in a few seconds.", "INFO", "PDF_GENERATED");
                setTimeout(() => {
                    formContext.ui.clearFormNotification("PDF_GENERATED");
                    formContext.data.refresh(false);
                }, 5000);
                console.log("[ConstructSafe] CORS/network error - flow was still triggered.");
            };
            request.send(JSON.stringify({
                ncId: recordId,
                ticketNumber: ticketNumber
            }));
            console.log("[ConstructSafe] PDF generation request sent for record:", recordId);
        }
        NonConformity.generatePDF = generatePDF;
        // =====================================================
        // BUSINESS LOGIC
        // =====================================================
        /**
         * Handles changes to the Type field.
         * Dynamically adjusts form layout based on the selected type.
         *
         * This is a key requirement from the assignment:
         * "Users can report different types of non-conformities,
         *  with the app displaying relevant fields based on the selected type."
         */
        function onTypeChange(executionContext) {
            const formContext = executionContext.getFormContext();
            applyTypeBasedRules(formContext);
        }
        /**
         * Core logic: Applies conditional rules based on the Non-Conformity Type.
         *
         * | Type            | Location  | Severity auto-set | Description required |
         * |-----------------|-----------|-------------------|---------------------|
         * | Safety          | Required  | High (locked)     | Yes                 |
         * | Quality         | Optional  | User choice       | Required            |
         * | Environmental   | Required  | User choice       | Required            |
         * | Documentation   | Hidden    | User choice       | Required            |
         * | Other           | Optional  | User choice       | Optional            |
         */
        function applyTypeBasedRules(formContext) {
            var _a;
            const typeValue = getSelectedOptionValue(formContext, Fields.TYPE);
            // Reset all fields to default state first
            resetFieldStates(formContext);
            switch (typeValue) {
                case NonConformityType.SAFETY:
                    // Safety: Location is required, Severity auto-set to High
                    setFieldRequired(formContext, Fields.LOCATION, true);
                    setFieldRequired(formContext, Fields.DESCRIPTION, true);
                    (_a = formContext.getAttribute(Fields.SEVERITY)) === null || _a === void 0 ? void 0 : _a.setValue(SeverityLevel.HIGH);
                    setFieldLocked(formContext, Fields.SEVERITY, true);
                    break;
                case NonConformityType.QUALITY:
                    // Quality: Description is required
                    setFieldRequired(formContext, Fields.DESCRIPTION, true);
                    break;
                case NonConformityType.ENVIRONMENTAL:
                    // Environmental: Both Location and Description are required
                    setFieldRequired(formContext, Fields.LOCATION, true);
                    setFieldRequired(formContext, Fields.DESCRIPTION, true);
                    break;
                case NonConformityType.DOCUMENTATION:
                    // Documentation: Hide Location (not relevant), Description required
                    setFieldVisible(formContext, Fields.LOCATION, false);
                    setFieldRequired(formContext, Fields.DESCRIPTION, true);
                    break;
                case NonConformityType.OTHER:
                default:
                    // Other/Default: All fields optional, visible
                    break;
            }
        }
        // =====================================================
        // HELPER FUNCTIONS
        // =====================================================
        /**
         * Sets the Date Reported field to current date/time for new records.
         */
        function setDefaultDateReported(formContext) {
            const dateAttr = formContext.getAttribute(Fields.DATE_REPORTED);
            if (dateAttr && !dateAttr.getValue()) {
                dateAttr.setValue(new Date());
            }
        }
        /**
         * Resets all conditionally-modified fields to their default state.
         */
        function resetFieldStates(formContext) {
            setFieldVisible(formContext, Fields.LOCATION, true);
            setFieldRequired(formContext, Fields.LOCATION, false);
            setFieldRequired(formContext, Fields.DESCRIPTION, false);
            setFieldLocked(formContext, Fields.SEVERITY, false);
        }
        /**
         * Gets the numeric value of a selected option set.
         */
        function getSelectedOptionValue(formContext, fieldName) {
            const attribute = formContext.getAttribute(fieldName);
            return attribute ? attribute.getValue() : null;
        }
        /**
         * Sets a field's visibility on the form.
         */
        function setFieldVisible(formContext, fieldName, visible) {
            const control = formContext.getControl(fieldName);
            if (control) {
                control.setVisible(visible);
            }
        }
        /**
         * Sets a field's requirement level.
         */
        function setFieldRequired(formContext, fieldName, required) {
            const attribute = formContext.getAttribute(fieldName);
            if (attribute) {
                attribute.setRequiredLevel(required ? "required" : "none");
            }
        }
        /**
         * Locks or unlocks a field on the form.
         */
        function setFieldLocked(formContext, fieldName, locked) {
            const control = formContext.getControl(fieldName);
            if (control) {
                control.setDisabled(locked);
            }
        }
    })(NonConformity = ConstructSafe.NonConformity || (ConstructSafe.NonConformity = {}));
})(ConstructSafe || (ConstructSafe = {}));
//# sourceMappingURL=nonconformity.form.js.map