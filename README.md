# Networg – ConstructSafe Non-Conformity Manager

Power Apps Model-Driven + Canvas Application for **ConstructSafe Inc.** to track and manage non-conformities in construction projects.

> **NETWORG Power Apps Candidate Review Assignment**

## 📁 Repository Structure

```
networg/
├── src/
│   ├── PowerApps/          # TypeScript - frontend form logic
│   │   └── Entities/
│   │       ├── NonConformity/
│   │       │   ├── Forms/          # Form event handlers
│   │       │   └── Ribbons/        # Ribbon button scripts
│   │       ├── CorrectiveAction/
│   │       │   └── Forms/
│   │       └── Evidence/
│   │           └── Forms/          # ImagePreview sync trigger
│   ├── Plugins/            # C# - backend server-side logic
│   │   ├── Common/         # Shared constants (EntityConstants.cs)
│   │   ├── NonConformity/  # AutoNumber, Notification plugins
│   │   └── Evidence/       # ImageSync plugin
│   └── Static/             # CSS, icons, images
├── solutions/
│   ├── Networg_Unpacked/   # Source-controlled XML of the solution
│   └── Managed_Builds/     # Exported .zip files (gitignored)
├── tests/
│   ├── Frontend/           # Jest unit tests for TS
│   └── Backend/            # xUnit tests for C# plugins
├── deploy/
│   ├── PowerShell/         # Deployment scripts
│   └── GitHubActions/      # CI/CD pipeline definitions
├── mapping/
│   └── deployment.settings.json   # Maps local JS → Dataverse Web Resources
├── docs/
│   └── task_progress.md    # Project progress tracking
├── package.json            # Node.js / TypeScript dependencies
├── tsconfig.json           # TypeScript compiler configuration
└── .gitignore
```

## 🛠️ Development Setup

```bash
# Install dependencies
npm install

# Build TypeScript → JavaScript
npm run build

# Watch mode (auto-rebuild on changes)
npm run watch

# Build C# Plugins (Release)
cd src/Plugins && dotnet build --configuration Release
```

## 🏗️ Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------
| **Model-Driven App** | Power Apps | Main management UI |
| **Canvas App** | Power Apps | Field worker mobile UI |
| **Data** | Dataverse | Tables, relationships, file storage |
| **Frontend Logic** | TypeScript | Form scripts, ribbon commands |
| **Backend Logic** | C# Plugins | AutoNumber, Notification, ImageSync |
| **Automation** | Power Automate | Notifications, PDF generation |
| **Source Control** | Git + GitHub | Version management |

## 📊 Data Model

- **Non-Conformity** – Main entity (type, severity, status, location, ticket number)
- **Corrective Action** – Remediation steps linked to NC (N:1)
- **Evidence** – Attachments/photos linked to NC and CA (N:1)
  - `Attachment` (File) – stores all file types
  - `ImagePreview` (Image) – synced copy for Canvas App display

## 🔌 Plugins

| Plugin | Entity | Message | Description |
|--------|--------|---------|-------------|
| **AutoNumberPlugin** | Non-Conformity | Create | Generates NC-XXXXX ticket numbers |
| **NotificationPlugin** | Non-Conformity | Create/Update | Emails assigned manager |
| **ImageSyncPlugin** | Evidence | Create/Update | Bidirectional Attachment↔ImagePreview sync |

## 🚀 Status

- [x] Environment Setup (NetworgTest)
- [x] Dataverse Data Model (3 tables + global choices)
- [x] Model-Driven App (ConstructSafe Manager)
- [x] TypeScript Business Logic (NonConformity form)
- [x] Git Repository Structure
- [x] AutoNumbering Plugin (NC-XXXXX)
- [x] Canvas App (ConstructSafe Field App)
- [x] ImageSync Plugin (File↔Image)
- [ ] Evidence Form JS (ImagePreview trigger)
- [ ] Power Automate Flows
- [ ] Presentation
