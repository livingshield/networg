# ConstructSafe Inc. – Non-Conformity Management System

A comprehensive Power Platform solution for tracking and managing construction non-conformities, designed for **NETWORG Power Apps Candidate Review**.

## 🚀 Project Overview

**ConstructSafe Inc.** requires a robust system to report incidents, assign corrective actions, and notify management. This solution leverages the full power of the Microsoft Power Platform, combining Model-Driven Apps, Canvas Apps, Power Automate, and custom Dataverse Plugins.

## ✨ Key Features

- **Model-Driven Management**: A professional back-office app for managers to review, track, and close Non-Conformities (NC).
- **TypeScript Logic**: Custom form scripts for dynamic field visibility and data validation based on incident types.
- **Canvas Field App**: A mobile-optimized experience for site workers to report incidents, add corrective actions, and capture photo evidence on-site.
- **Automated PDF Reports**: High-end Power Automate flow that generates professional PDF summaries with embedded photo documentation upon incident closure.
- **Advanced Dataverse Plugins (C#)**:
  - **AutoNumbering**: Unique ticket generation (e.g., NC-0001).
  - **Image Sync**: Bidirectional synchronization between file attachments and image thumbnails for optimized reporting.
  - **Notifications**: Automated email alerts to assigned managers.

## 🛠️ Technology Stack

- **Platform**: Microsoft Power Platform (Power Apps, Power Automate, Dataverse)
- **Languages**: TypeScript (Frontend), C# (Dataverse Plugins), HTML/CSS (Reporting)
- **Source Control**: Git (Structured repository with source/output separation)
- **Dev Tools**: PAC CLI, VS Code, Node.js

## 📁 Project Structure

```text
├── src/
│   ├── Plugins/         # C# Plugin source code
│   ├── PowerApps/       # TypeScript Form Scripts (TS)
│   └── Static/          # Solution assets and branding
├── out/                 # Compiled JavaScript Web Resources
├── docs/                # Technical documentation and plans
├── solutions/           # Exported solution files (managed/unmanaged)
└── README.md            # You are here
```

## 🔨 How to Deploy

1. **Import Solution**: Import the `ConstructSafe_Managed.zip` into your Dataverse environment.
2. **Publish Customizations**: Ensure all entities and scripts are published.
3. **Configure Flow**: Update the HTTP trigger URL in the `nonconformity.form.js` file to match your environment's Power Automate endpoint.
4. **Grant Access**: Ensure users have the appropriate security roles (ConstructSafe User / Manager).

---
*Developed as part of the NETWORG Candidate Review Assignment.*
