/**
 * Evidence Form Script
 * 
 * Monitors the ImagePreview field for changes and logs activity.
 * The actual sync is handled by the async ImageSyncPlugin which
 * triggers on any Update to the Evidence entity.
 * 
 * @module EvidenceForm
 * @author Jan Kytyr
 */

namespace ConstructSafe.Evidence {

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
    export function onLoad(executionContext: Xrm.Events.EventContext): void {
        const formContext = executionContext.getFormContext();

        // Register onChange handler for ImagePreview field
        const imagePreviewAttr = formContext.getAttribute(Fields.IMAGE_PREVIEW);
        if (imagePreviewAttr) {
            imagePreviewAttr.addOnChange(onImagePreviewChange);
            console.log("[ConstructSafe] Evidence: ImagePreview onChange registered.");
        }

        console.log("[ConstructSafe] Evidence form loaded.");
    }

    /**
     * Called when ImagePreview field value changes.
     * The async ImageSyncPlugin handles the actual sync automatically.
     * This handler just ensures the field is submitted with the save.
     */
    function onImagePreviewChange(executionContext: Xrm.Events.EventContext): void {
        const formContext = executionContext.getFormContext();

        console.log("[ConstructSafe] Evidence: ImagePreview changed. Async plugin will handle sync.");

        // Ensure ImagePreview value is submitted when form saves
        const imagePreviewAttr = formContext.getAttribute(Fields.IMAGE_PREVIEW);
        if (imagePreviewAttr) {
            (imagePreviewAttr as Xrm.Attributes.StringAttribute).setSubmitMode("always");
        }
    }
}
