# Repository Guidelines

## Project Structure & Module Organization
- Root contains the WPF entry points: `App.xaml`/`App.xaml.cs` for startup and shared resources, `MainWindow.xaml`/`MainWindow.xaml.cs` for the initial window, and `evidence_timeline.csproj` targeting `net10.0-windows`.
- Build outputs live in `bin/` and `obj/`; leave them untracked.
- When adding features, group code into `Views/`, `ViewModels/`, `Models/`, `Services/`, and `Assets/` (images/styles). Keep shared XAML styles in resource dictionaries and reference them via `App.xaml`.

## Build, Test, and Development Commands
- `dotnet restore` — restore NuGet dependencies.
- `dotnet build evidence_timeline.csproj` — compile the app (use `-c Release` for distributable builds).
- `dotnet run --project evidence_timeline.csproj` — launch the WPF app for local development.
- `dotnet clean` — remove previous build outputs.
- `dotnet format` — apply consistent C# formatting (run before commits when available).

## Coding Style & Naming Conventions
- Use 4-space indentation and Allman braces (newline before `{`) to match the current template.
- PascalCase for types, methods, properties, events; camelCase with a leading `_` for private fields; ALL_CAPS for constants.
- Keep nullable annotations accurate (`Nullable` is enabled). Avoid `!` unless justified; prefer explicit null checks.
- Favor MVVM: place UI behavior in view models and commands; keep code-behind minimal and limited to wiring.
- XAML: name elements semantically (e.g., `TimelineList`, `FilterPanel`), keep layout simple (`Grid`/`StackPanel`), and extract reusable styles/templates.

## Testing Guidelines
- No automated tests yet; prefer xUnit in a `tests/` folder (e.g., `EvidenceTimeline.Tests`).
- Name tests `MethodName_Scenario_ExpectedResult`; keep tests deterministic and isolated from UI threading where possible.
- Run `dotnet test` for the suite once added; target high coverage on view models and services, with UI covered via view-model interactions.

## Commit & Pull Request Guidelines
- Use imperative, concise commit messages (e.g., `Add timeline filtering`, `Fix null handling in event import`).
- Scope commits narrowly; separate refactors from feature changes when possible.
- PRs should summarize the change, list validation steps (`dotnet build`, `dotnet run`), and include screenshots or notes for visible UI updates. Link related issues and include test results once tests exist.

## Release Management

### Complete Release Checklist

When creating a new version release, follow these steps **in order**:

#### 1. Update Version Numbers
- [ ] **CHANGELOG.md**: Add new version section at the top with date and categorize changes (Added, Changed, Fixed, Removed, Security)
- [ ] **CHANGELOG.md**: Update "Current Version" in the Version Tracking section
- [ ] **CHANGELOG.md**: Add version link at the bottom (e.g., `[1.1.0]: https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.1.0`)
- [ ] **evidence_timeline.csproj**: Update `Version`, `AssemblyVersion`, and `FileVersion` (e.g., `1.1.0` and `1.1.0.0`)
- [ ] **installer.iss**: Update `MyAppVersion` define (e.g., `#define MyAppVersion "1.1.0"`)
- [ ] **update.xml**: Update `<version>` tag (e.g., `1.1.0`)
- [ ] **update.xml**: Update `<url>` tag with new download URL (e.g., `https://github.com/LCRH1883/evidence_timeline/releases/download/v1.1.0/EvidenceTimeline-Setup-v1.1.0.exe`)
- [ ] **update.xml**: Update `<changelog>` tag with new release URL (e.g., `https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.1.0`)

#### 2. Build the Release
- [ ] Clean previous builds: `dotnet clean`
- [ ] Publish release build: `dotnet publish evidence_timeline.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false`
- [ ] Compile installer: `"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss`
- [ ] Verify installer created: Check `installer_output\EvidenceTimeline-Setup-v{version}.exe` exists

#### 3. Commit Changes
- [ ] Commit all version changes: `git add CHANGELOG.md evidence_timeline.csproj installer.iss update.xml`
- [ ] Create commit: `git commit -m "Bump version to {version}"`
- [ ] Tag release: `git tag v{version}` (e.g., `git tag v1.1.0`)
- [ ] Push changes: `git push origin main`
- [ ] Push tags: `git push origin v{version}`

#### 4. Create GitHub Release
- [ ] Go to: `https://github.com/LCRH1883/evidence_timeline/releases/new`
- [ ] Select tag: `v{version}` (should exist from previous step)
- [ ] Release title: `Evidence Timeline v{version}` (e.g., `Evidence Timeline v1.1.0`)
- [ ] Description: Copy changelog content from CHANGELOG.md for this version
- [ ] Upload installer: Attach `installer_output\EvidenceTimeline-Setup-v{version}.exe`
- [ ] Verify installer filename matches URL in update.xml exactly
- [ ] Click "Publish release"

#### 5. Verify Auto-Update Works
- [ ] Install the previous version if not already installed
- [ ] Run the application and go to Help → Check for Updates
- [ ] Verify it detects the new version
- [ ] Verify changelog content displays correctly
- [ ] Test the update process (optional but recommended)

### Important Notes
- **Installer naming scheme:** `EvidenceTimeline-Setup-v{version}.exe` (e.g., `EvidenceTimeline-Setup-v1.1.0.exe`)
- **Installer output location:** `D:\Projects\evidence_timeline\installer_output\`
- **Version format:** Use semantic versioning `MAJOR.MINOR.PATCH` (e.g., `1.1.0`)
- **Git tag format:** Always prefix with `v` (e.g., `v1.1.0`)
- **Auto-update requires:** The `update.xml` file MUST be in the `main` branch on GitHub and publicly accessible
- **Installer URL must match:** The filename in the GitHub release must exactly match the URL in `update.xml`

## Security & Configuration Tips
- Do not commit secrets or machine-specific settings. Use user-local config files if future integrations require keys.
- Stay on Windows-compatible APIs and verify new packages support the targeted `net10.0-windows` WPF environment.
