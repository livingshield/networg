# NETWORG Power Apps Assignment – Analysis & Implementation

## Phase 1: Analysis

- [x] Extract and read the PDF assignment
- [x] Create detailed analysis document

## Phase 2: Planning (after discussion with user)

- [x] Design data model (Dataverse tables)
- [x] Plan Power Apps Model-Driven App architecture
- [x] Plan Repository Structure (Frontend/Backend separation)

## Phase 3: Implementation

- [x] Set up Power Apps Developer environment
- [x] Create Dataverse tables and relationships
- [x] Build Model-Driven App
- [x] TypeScript form script (conditional fields, validation) – DEPLOYED ✅
- [x] Custom Views (Active NC, My NC, By Severity) – DEPLOYED ✅
- [x] Notification Plugin (email to Assigned Manager) – DEPLOYED ✅
- [x] Git source control (bonus) ✅

## Phase 4: Optional Tasks

- [x] Autonumbering plugin (NC-XXXXX) – DEPLOYED ✅
- [x] Non-Conformity form: Ticket Number read-only on load ✅
- [/] Canvas App for Corrective Actions – IN PROGRESS
  - [x] Canvas App screens (NCListScreen, NCDetailScreen, AddCAScreen)
  - [x] Evidence data model (with Corrective Action lookup)
  - [x] Add Media Button for image capture
  - [/] File upload to Dataverse File column (Power Automate blocked by premium connector)
  - [x] ImageSyncPlugin – bidirectional File ↔ Image sync – BUILT ✅
  - [x] Image Preview column added to Evidence entity
  - [ ] Evidence form JavaScript – ImagePreview change trigger
- [ ] Custom homepage grid onclick behavior
- [ ] BPF process with custom next stage button
- [ ] HTTP-triggered Power Automate flow
- [ ] Custom connector in Power Automate
- [ ] Synchronous copy logic for non-conformity + children

## Phase 5: Bonus Points

- [x] JavaScript/TypeScript for conditional fields ✅
- [ ] Power Automate PDF summary flow
- [ ] Discussion: Business Rules vs. Custom Scripts (for presentation)

## Phase 6: Presentation

- [ ] Prepare structured presentation
- [ ] Live demo

---

## Recent Work Log (Feb 21-22, 2026)

### Feb 21: Canvas App & File Upload

- Built Canvas App **ConstructSafe Field App** with 3 screens
- Added Evidence→Corrective Action lookup to Dataverse schema
- Updated `EntityConstants.cs` with new lookup constant
- Attempted Power Automate flow for file upload (blocked by premium HTTP connector)
- Made Ticket Number field read-only in NonConformity form (`nonconformity.form.ts/js`)

### Feb 22: ImageSync Plugin

- Designed dual-column architecture: File (Attachment) + Image (ImagePreview)
- Created `ImageSyncPlugin.cs` – bidirectional reconciliation:
  - Attachment→ImagePreview (Model-Driven upload)
  - ImagePreview→Attachment (Canvas App upload)
  - Anti-loop via `context.Depth > 1`
- Added `new_imagepreview` column constant to `EntityConstants.cs`
- Plugin deployed and tested:
  - ✅ Attachment image → ImagePreview works
  - ✅ Attachment change → ImagePreview updates
  - ⚠️ Image column changes don't trigger Update message → needs JS workaround
- Next: Evidence form JavaScript to trigger plugin on ImagePreview change
