# Evidence Timeline

A comprehensive case management and evidence timeline application for legal professionals.

**Developed by Intagri Technologies LLC**
© 2025 Intagri Technologies LLC. All rights reserved.

## Overview

Evidence Timeline is a Windows desktop application designed to help legal professionals organize, manage, and analyze case evidence with an intuitive timeline-based interface. The application supports rich note-taking, file attachments, person tracking, and comprehensive case metadata management.

## Key Features

### Case Management
- **Create and manage multiple cases** with dedicated case folders
- **Case metadata tracking**: case number, court location, court level
- **Recent cases list** for quick access to frequently used cases
- **Case statistics**: view total evidence count, creation date, last opened date
- **Persistent case settings**: each case remembers its UI preferences independently

### Evidence Organization
- **Timeline-based evidence view** with sortable date columns
- **Multiple date support per evidence item**: acquisition date, event date, custom dates
- **Evidence types**: Document, Physical, Digital, Testimonial, Video, Audio, Photo, Other
- **Evidence linking**: connect related pieces of evidence
- **Person associations**: track which people are related to each evidence item
- **File attachments**: attach and manage multiple files per evidence
- **Evidence search and filtering** (via timeline interface)

### Rich Note-Taking
- **Formatted notes** with rich text editing support
- **Text formatting**: bold, italic, underline, strikethrough
- **Lists**: bullet lists and numbered lists with proper continuation
- **Adjustable font size** (6pt-14pt) via View menu, saved per-case
- **Notes preserved** with each evidence item and case

### User Interface
- **Tabbed bottom pane** with Notes, Case Info, and Info tabs
- **Collapsible panels**: left pane (evidence list), right pane (metadata), bottom pane (notes/info)
- **Zoom control**: adjust interface scale (80%-150%)
- **Dark/Light theme support** via AdonisUI
- **Customizable pane visibility** saved per-case

### Data Persistence & Safety
- **Non-destructive updates**: application updates never touch your case data
- **Separate storage**: case files stored in user-selected folders, app settings in AppData
- **All data preserved through updates**: cases, evidence, notes, attachments, settings
- **Automatic case settings save**: UI preferences persist between sessions

### Auto-Update System
- **Automatic update checks** on startup (Release builds only)
- **GitHub-based updates** from public repository
- **Update options**: Install now, Skip this version, Remind me in 7 days
- **Development builds isolated**: Debug builds use separate settings folder and have updates disabled
- **Safe updates**: only replaces application binaries, never touches user data

### Company Branding
- **Professional startup screen** with company logo
- **Branded window icons** across all windows
- **About dialog** with version and company information
- **Copyright notices** on startup screen

## Installation

### End Users (Release Version)

1. Download the latest installer from the [Releases page](https://github.com/LCRH1883/evidence_timeline/releases)
2. Run `EvidenceTimeline-Setup.exe`
3. Follow the installation wizard
4. Launch Evidence Timeline from the Start menu or desktop shortcut

**System Requirements**:
- Windows 10 or Windows 11
- .NET 10.0 Runtime (installed automatically if needed)

### Developers (Debug Version)

See the [Development Setup](#development-setup) section below.

## Usage Guide

### Creating a New Case

1. Launch Evidence Timeline
2. Click **"Create new case"** on the start screen
3. Choose a folder location for your case (case data will be stored here)
4. Enter the case name
5. The case folder will be created and the main window will open

### Opening an Existing Case

1. Launch Evidence Timeline
2. Select a case from the **"Recent cases"** list and click **"Open selected"**
3. Or click **"Browse..."** to navigate to a case folder

### Adding Evidence

1. Click the **+ (Add Evidence)** button in the toolbar
2. Enter evidence details:
   - Evidence number (auto-assigned, editable)
   - Description
   - Evidence type
   - Dates (acquisition, event, or custom dates)
3. Click **"Save"**

### Editing Evidence

1. Double-click an evidence item in the timeline
2. Or select an evidence item and click the **Edit** button
3. Modify any fields as needed
4. Add notes with rich text formatting
5. Attach files by clicking **"Add Attachment"**
6. Link related evidence items
7. Associate people with the evidence
8. Click **"Save"** to persist changes

### Managing Case Information

1. Click the **"Case Info"** tab in the bottom pane
2. Enter or update:
   - Case Number
   - Court Location
   - Court Level
3. Changes save automatically

### Viewing Case Statistics

Click the **"Info"** tab in the bottom pane to view:
- Total number of evidence items
- Case creation date
- Last opened date
- Case folder path

### Customizing the Interface

**Adjust Font Size** (for text input areas):
- Go to **View → Editor Font Size**
- Select from 6pt to 14pt
- Setting is saved per-case

**Toggle Panes**:
- **View → Show Left Pane** (evidence list)
- **View → Show Right Pane** (metadata panel)
- **View → Show Bottom Pane** (notes/info tabs)

**Zoom**:
- **View → Zoom In** (Ctrl+Plus)
- **View → Zoom Out** (Ctrl+Minus)
- **View → Reset Zoom** (Ctrl+0)

**Sort Evidence**:
- **View → Sort Newest First** (toggle)

### Taking Notes

1. Select an evidence item or view case notes
2. Click in the notes area (RichTextBox)
3. Use the formatting toolbar:
   - **B** - Bold
   - **I** - Italic
   - **U** - Underline
   - **S** - Strikethrough
   - **Bullet list** - Create/continue bulleted lists
   - **Numbered list** - Create/continue numbered lists
4. Press Enter in a list to continue with the next item
5. Notes save automatically

### Checking for Updates

- **Automatic** (Release builds only): Checks on startup, prompts if update available
- **Manual**: **Help → Check for Updates**
- Debug builds: Updates are disabled in development versions

## Data Storage

### Case Data (User-Selected Location)
When you create a case, you choose where to store it. The case folder contains:

```
YourCaseFolder/
├── case.json              # Case metadata (name, case number, court info)
├── settings.json          # Case-specific settings (panes, zoom, font size, sort)
└── evidence/              # All evidence data
    ├── {evidence-id}/     # Individual evidence folder
    │   ├── metadata.json  # Evidence details, dates, types, people, links
    │   ├── note.md        # Evidence notes (formatted text)
    │   └── files/         # File attachments for this evidence
    └── ...
```

**Important**: This folder is NEVER touched by application updates. You can move it, back it up, or store it anywhere you like.

### Application Settings (AppData)

Application-level settings are stored separately:

- **Release builds**: `%AppData%\EvidenceTimeline\settings.json`
- **Debug builds**: `%AppData%\EvidenceTimeline-Dev\settings.json`

Contains:
- Recent cases list
- Last opened case path
- Application preferences

**Important**: These settings persist through updates and are separate from case data.

## Update Safety

### What Gets Updated
When you install an application update, **ONLY** these items are replaced:
- Application executables (`.exe` files)
- Application libraries (`.dll` files)
- Program resources (icons, UI files)
- Installation directory contents (e.g., `C:\Program Files\Evidence Timeline\`)

### What Is NEVER Touched
Your data is **completely safe** during updates:
- ✅ All case files and folders (stored wherever you chose)
- ✅ All evidence metadata
- ✅ All notes and attachments
- ✅ Application settings in AppData
- ✅ Recent cases list
- ✅ Window positions and preferences

### Why It's Safe
1. **Separate storage**: User data is in user-selected folders, app is in Program Files
2. **No data migration**: Updates don't move or modify any data files
3. **Settings preservation**: All settings stored outside app directory
4. **Rollback safe**: Can reinstall older version without losing data

## Development Setup

### Prerequisites
- Windows 10 or Windows 11
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 (recommended) or VS Code with C# extension

### Clone and Build

```bash
# Clone the repository
git clone https://github.com/LCRH1883/evidence_timeline.git
cd evidence_timeline

# Restore dependencies
dotnet restore

# Build Debug version
dotnet build

# Run the application
dotnet run
```

### Project Structure

```
evidence_timeline/
├── assets/                    # Application icons and images
│   ├── intagri icon.png      # Window icons
│   ├── intagri-icon.ico      # Executable icon
│   └── Logo.png              # Company logo
├── Models/                    # Data models
│   ├── CaseInfo.cs
│   ├── CaseSettings.cs
│   ├── EvidenceItem.cs
│   └── ...
├── ViewModels/               # MVVM view models
│   ├── MainViewModel.cs
│   └── EvidenceWindowViewModel.cs
├── Views/                    # UI windows and dialogs
│   ├── StartDialog.xaml
│   ├── MainWindow.xaml
│   ├── EvidenceWindow.xaml
│   └── ...
├── App.xaml                  # Application entry point and styles
├── App.xaml.cs
└── evidence_timeline.csproj  # Project file
```

### Development vs Production

The application uses conditional compilation to separate development and production environments:

**Debug Builds (Development)**:
- Settings folder: `%AppData%\EvidenceTimeline-Dev`
- Auto-updates: **DISABLED**
- Help → Check for Updates: Shows "updates disabled" message
- Use for development on your machine

**Release Builds (Installed)**:
- Settings folder: `%AppData%\EvidenceTimeline`
- Auto-updates: **ENABLED**
- Checks for updates on startup
- Use for production installations

This allows developers to run both versions simultaneously without conflicts.

### Building for Release

```bash
# Build Release version
dotnet build -c Release

# Publish self-contained (includes .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained true

# Publish framework-dependent (smaller, requires .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained false
```

Output will be in `bin\Release\net10.0-windows\win-x64\publish\`

## Creating a Release

### 1. Update Version Number

Update the version in `update.xml`:
```xml
<version>1.0.1</version>
```

### 2. Build Release Version

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

### 3. Create Installer

Use an installer tool such as:
- [Inno Setup](https://jrsoftware.org/isinfo.php) (free)
- [WiX Toolset](https://wixtoolset.org/) (free)
- Or create a ZIP archive for manual installation

### 4. Create GitHub Release

1. Go to: https://github.com/LCRH1883/evidence_timeline/releases
2. Click **"Create a new release"**
3. Tag version: `v1.0.1` (must match update.xml)
4. Release title: `Evidence Timeline v1.0.1`
5. Add release notes describing changes
6. Attach the installer file (e.g., `EvidenceTimeline-Setup.exe`)
7. Click **"Publish release"**

### 5. Update update.xml

Update `update.xml` with the new release information:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<item>
    <version>1.0.1</version>
    <url>https://github.com/LCRH1883/evidence_timeline/releases/download/v1.0.1/EvidenceTimeline-Setup.exe</url>
    <changelog>https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.0.1</changelog>
    <mandatory>false</mandatory>
</item>
```

### 6. Commit and Push

```bash
git add update.xml
git commit -m "Release version 1.0.1"
git push origin main
```

The auto-updater will now detect the new version and prompt users to update.

## Architecture

### Technology Stack
- **.NET 10.0**: Latest .NET framework
- **WPF (Windows Presentation Foundation)**: UI framework
- **XAML**: Declarative UI markup
- **AdonisUI**: Modern theme library for dark/light modes
- **AutoUpdater.NET**: Automatic update system

### Design Patterns
- **MVVM (Model-View-ViewModel)**: Separation of UI and business logic
- **Data Binding**: Automatic UI updates when data changes
- **Commands**: Declarative action handling
- **Dependency Injection**: Service-based architecture (planned)

### Key Dependencies
- `AdonisUI` v1.17.1 - UI theme framework
- `AdonisUI.ClassicTheme` v1.17.1 - Classic theme support
- `Autoupdater.NET.Official` v1.9.2 - Auto-update functionality
- `MahApps.Metro.IconPacks` v6.2.1 - Icon library

## Contributing

This is a proprietary application developed by Intagri Technologies LLC. For bug reports or feature requests, please contact the development team.

## License

© 2025 Intagri Technologies LLC. All rights reserved.

This software is proprietary and confidential. Unauthorized copying, distribution, or use is strictly prohibited.

## Support

For support inquiries, please contact:
- **GitHub Issues**: https://github.com/LCRH1883/evidence_timeline/issues
- **Company**: Intagri Technologies LLC

## Additional Documentation

- **[UPDATE_SETUP.md](UPDATE_SETUP.md)** - Detailed auto-update system documentation
- **[ICON_SETUP.md](ICON_SETUP.md)** - Icon and branding implementation guide
- **[CLAUDE.md](CLAUDE.md)** - Development guidelines for AI-assisted coding

## Version History

### v1.0.0 (Initial Release)
- Case and evidence management
- Timeline-based evidence organization
- Rich text note-taking with formatting
- File attachment support
- Person and evidence linking
- Adjustable editor font sizes (6pt-14pt)
- Tabbed interface (Notes, Case Info, Info)
- Auto-update system
- Company branding and professional UI
- Non-destructive data persistence
