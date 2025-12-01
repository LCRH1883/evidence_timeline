# Auto-Update Setup Guide

## Overview
This application now supports non-destructive auto-updates using AutoUpdater.NET and GitHub Releases.

## How It's Non-Destructive
- **User data is safe**: All cases are stored in user-selected folders (outside the app directory)
- **Only app files updated**: Updates only replace application binaries
- **Settings preserved**: AppData settings remain untouched
- **Separate dev/production**: Debug builds use different settings folders

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

### 1. Update Code with Your GitHub Info

In `App.xaml.cs` and `MainWindow.xaml.cs`, replace:
```
YOUR_USERNAME → Your GitHub username
YOUR_REPO → Your repository name
```

Example:
```csharp
AutoUpdater.Start("https://raw.githubusercontent.com/intagri/evidence-timeline/main/update.xml");
```

### 2. Commit update.xml to Your Repository

1. Edit `update.xml` with your GitHub info
2. Commit it to the `main` branch of your repository
3. Push to GitHub

### 3. Create a GitHub Release

When releasing a new version:

1. **Build Release version:**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false
   ```

2. **Create installer** (using tools like Inno Setup, WiX, or just a zip)

3. **Create GitHub Release:**
   - Go to your repo → Releases → Create new release
   - Tag: `v1.0.1` (increment version number)
   - Title: `Evidence Timeline v1.0.1`
   - Description: Changelog/release notes
   - Attach your installer/executable file

4. **Update update.xml:**
   ```xml
   <?xml version="1.0" encoding="UTF-8"?>
   <item>
       <version>1.0.1</version>
       <url>https://github.com/YOUR_USERNAME/YOUR_REPO/releases/download/v1.0.1/EvidenceTimeline-Setup.exe</url>
       <changelog>https://github.com/YOUR_USERNAME/YOUR_REPO/releases/tag/v1.0.1</changelog>
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
