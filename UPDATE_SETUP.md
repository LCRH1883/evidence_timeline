# Auto-Update Setup Guide

## Overview
This application now supports non-destructive auto-updates using AutoUpdater.NET and GitHub Releases.

## How It's Non-Destructive

Updates **ONLY** replace application executable files. All user data persists through updates.

### What Gets Updated (Application Files Only)
- Application binaries (`.exe`, `.dll` files)
- Program resources (icons, UI files)
- **Location**: Installation directory (e.g., `C:\Program Files\Evidence Timeline\`)

### What Is NEVER Touched (Your Data)
- Case files and folders (user-selected locations)
- Evidence metadata (stored in case folders)
- Notes and attachments (stored in case folders)
- Application settings (stored in `%AppData%`)
- Recent cases list (stored in `%AppData%`)
- Window positions and sizes (stored in `%AppData%`)
- User preferences (stored in `%AppData%`)

### Data Storage Locations

**Case Data** (Never touched by updates):
- Location: User-selected folder when creating/opening case
- Contains:
  - `case.json` - Case metadata (name, case number, court location, court level)
  - `settings.json` - Case-specific settings (pane visibility, sort order, zoom level, editor font size)
  - `evidence/` - All evidence metadata files
  - `evidence/{evidence-id}/` - Individual evidence folders
    - `metadata.json` - Evidence details, dates, types, people, attachments
    - `note.md` - Evidence notes (formatted text)
    - `files/` - File attachments copied here

**Application Settings** (Persisted through updates):
- Debug: `%AppData%\EvidenceTimeline-Dev\settings.json`
- Release: `%AppData%\EvidenceTimeline\settings.json`
- Contains:
  - Recent cases list (paths to recently opened cases)
  - Last opened case path
  - Application preferences

### Why It's Safe
1. **Separate storage**: User data is in user-selected folders, app is in Program Files
2. **No data migration**: Updates don't move or modify any data files
3. **Settings preservation**: All settings stored outside app directory
4. **Rollback safe**: Can reinstall older version without losing data

## Development vs Production

### Debug Builds (Development)
- Settings folder: `%AppData%\EvidenceTimeline-Dev`
- Auto-updates: **DISABLED**
- Help → Check for Updates: Shows message that updates are disabled
- Used while developing on your machine

### Release Builds (Installed)
- Settings folder: `%AppData%\EvidenceTimeline`
- Auto-updates: **ENABLED**
- Checks for updates on startup (can skip or remind later)
- Help → Check for Updates: Manually checks for new version

## Setup Instructions

### 1. GitHub Repository

Your repository is configured as:
- **Username**: LCRH1883
- **Repository**: evidence_timeline
- **Update URL**: `https://raw.githubusercontent.com/LCRH1883/evidence_timeline/main/update.xml`

This is already configured in the code - no changes needed!

### 2. Commit update.xml to Your Repository

1. The `update.xml` file is already configured
2. Commit it to the `main` branch of your repository:
   ```bash
   git add update.xml
   git commit -m "Add auto-update configuration"
   git push origin main
   ```
3. This file must be in your public GitHub repo for the app to access it

### 3. Create a GitHub Release

When releasing a new version:

1. **Build Release version:**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false
   ```

2. **Create installer** (using tools like Inno Setup, WiX, or just a zip)

3. **Create GitHub Release:**
   - Go to your repo → Releases → Create new release
   - Tag: `v1.1.0` (increment version number)
   - Title: `Evidence Timeline v1.1.0`
   - Description: Changelog/release notes
   - Attach your installer/executable file

4. **Update update.xml:**
   ```xml
   <?xml version="1.0" encoding="UTF-8"?>
   <item>
       <version>1.1.0</version>
       <url>https://github.com/LCRH1883/evidence_timeline/releases/download/v1.1.0/EvidenceTimeline-Setup-v1.1.0.exe</url>
       <changelog>https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.1.0</changelog>
       <mandatory>false</mandatory>
   </item>
   ```

5. **Commit and push** the updated `update.xml`

## How Updates Work

### Automatic Check (Release builds only)
- Checks on app startup
- If update found, shows dialog with:
  - **Update**: Downloads and installs new version
  - **Skip**: Ignore this version
  - **Remind Later**: Check again in 7 days

### Manual Check
- Help → Check for Updates
- Debug builds: Shows message that updates are disabled
- Release builds: Immediately checks for updates

## Version Numbering

Use semantic versioning: `MAJOR.MINOR.PATCH`
- **MAJOR**: Breaking changes
- **MINOR**: New features
- **PATCH**: Bug fixes

Example: `1.0.0` → `1.0.1` (bug fix) → `1.1.0` (new feature) → `2.0.0` (major change)

## Testing Updates

1. Build Debug version (your dev environment)
2. Build Release version
3. Install Release version in different location (e.g., Program Files)
4. Both can run simultaneously without conflict
5. Test update from installed Release version

## Security Notes

- Updates are downloaded over HTTPS
- AutoUpdater.NET verifies file integrity
- Users can always skip updates
- Set `mandatory="true"` in update.xml to force critical security updates

## Troubleshooting

**Update check fails:**
- Verify `update.xml` is in `main` branch
- Check GitHub URL is correct
- Ensure file is publicly accessible (not in private repo without auth)

**Settings conflict:**
- Debug uses: `%AppData%\EvidenceTimeline-Dev`
- Release uses: `%AppData%\EvidenceTimeline`
- They are completely separate

**Update doesn't install:**
- User may need admin rights for Program Files
- Consider using ClickOnce or per-user install location
