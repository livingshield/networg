# Networg – ConstructSafe Non-Conformity Manager

Power Apps Model-Driven Application for **ConstructSafe Inc.** to track and manage non-conformities in construction projects.

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
│   │           └── Forms/
│   ├── Plugins/            # C# - backend server-side logic
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
│   └── analyza_zadani.md   # Assignment analysis
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
```

## 🏗️ Tech Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **App** | Power Apps (Model-Driven) | Main UI |
| **Data** | Dataverse | Tables, relationships |
| **Frontend Logic** | TypeScript | Form scripts, ribbon commands |
| **Backend Logic** | C# Plugins | Autonumbering, server-side validation |
| **Automation** | Power Automate | Notifications, PDF generation |
| **Source Control** | Git + GitHub | Version management |

## 📊 Data Model

- **Non-Conformity** – Main entity (type, severity, status, location)
- **Corrective Action** – Remediation steps linked to NC (N:1)
- **Evidence** – Attachments/photos linked to NC (N:1)

## 🚀 Status

- [x] Environment Setup (NetworgTest)
- [x] Dataverse Data Model (3 tables + global choices)
- [x] Model-Driven App (ConstructSafe Manager)
- [x] Git Repository Structure
- [ ] TypeScript Business Logic
- [ ] Power Automate Flows
- [ ] Optional/Bonus Tasks
