# 📄 Technical Documentation – ConstructSafe Management System

This document provides a deep dive into the technical architecture and implementation details of the ConstructSafe solution designed for the NETWORG Candidate Review.

## 🏗️ Data Architecture (Dataverse)

The solution is built on a relational Dataverse model consisting of three primary tables:

1. **Non-Conformity (`new_nonconformity`)**: The root entity for incident reporting. It uses a custom **AutoNumbering Plugin** to generate unique ticket numbers (`NC-0000X`).
2. **Corrective Action (`new_correctiveaction`)**: Linked to Non-Conformity (N:1). Stores planned resolutions.
3. **Evidence (`new_evidence`)**: Linked to both NC and CA (N:1). Stores photos and documents.
    - *Optimization*: Uses a bidirectional **ImageSync Plugin** to keep `Attachment` (File) and `Image Preview` (Image) in sync, allowing for high-performance thumbnail generation in reports.

## 💻 Custom Development

### TypeScript (Form Logic)

We utilized TypeScript for client-side logic to ensure type safety and maintainability.

- **Dynamic UI**: Fields like `Location Details` and `Photo Mandatory` checkbox change visibility based on the NC Type using `Xrm.Page` (Client API).
- **External Integration**: A custom JavaScript function handles the HTTP POST request to Power Automate, passing the record ID and triggering the PDF generation.

### 💡 Business Rules vs. Custom Scripts (Assignment Requirement)

During implementation, we evaluated both approaches:

- **Business Rules**: Used for simple logic (hiding fields based on local conditions) because they are declaratively maintained and run both on the client and server.
- **Custom Scripts (JS/TS)**: Used for more complex requirements like triggering Power Automate via HTTP, calling `Xrm.Utility` for progress indicators, and complex cross-entity logic that exceeds Business Rule capabilities.
  - *Decision*: We used TypeScript to ensure type-safety and a maintainable code structure in Git.

### C# Plugins (Backend Logic)

Three custom plugins were developed to handle synchronous and asynchronous backend tasks:

1. **AutoNumbering**: Executed on `Pre-Operation Create` to ensure atomic increment of ticket numbers.
2. **Notification Agent**: Executed on `Post-Operation Create/Update` to send automated emails to Assigned Managers.
3. **ImageSync**: Automatically generates thumbnails for file attachments, which is critical for the PDF reporting engine's performance.

## ⚡ Automation & Reporting

### Power Automate: PDF Reporting Engine

The core reporting logic resides in a high-performance Power Automate flow:

- **Challenge**: Standard PDF generation often fails with large Base64 image payloads.
- **Solution**: The flow retrieves **Medium-sized thumbnails** from Dataverse's `Image Preview` column instead of full-resolution files. This reduces the payload by ~90% while maintaining report quality.
- **Branding**: Uses a modern HTML/CSS template with CSS Flexbox for professional layout.

## 📱 Canvas Field App

The Canvas app provides a streamlined "one-step" experience for site workers:

- **Double-Patch Logic**: Uses a specialized Power Fx formula to create a Corrective Action and link an uploaded photo simultaneously, ensuring data integrity even on mobile devices.
- **iOS Compatibility**: Optimized for iPhone by using record-passing via `Navigate` context instead of standard global variables.

## 🛠️ Portability & Git Structure

The project follows a standard professional structure:

- `src/`: Source code for all custom units.
- `out/`: Compiled assets for direct deployment.
- Solution files are exported as Power Platform Solutions for easy environment migration.

---
*Technical documentation finalized for review.*
