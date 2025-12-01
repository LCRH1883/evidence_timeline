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
- When creating a new release, **always update CHANGELOG.md** with the new version and changes.
- Update version numbers in `evidence_timeline.csproj` (`Version`, `AssemblyVersion`, `FileVersion`).
- Update version number in `installer.iss` (`MyAppVersion` define).
- Follow the changelog format: add a new version section with release date and categorize changes (Added, Changed, Fixed, etc.).
- Update the "Current Version" in the CHANGELOG.md Version Tracking section.
- Build the release version:
  1. Run `dotnet publish evidence_timeline.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false`
  2. Run the Inno Setup compiler: `"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss`
- **Installer naming scheme:** `EvidenceTimeline-Setup-v{version}.exe` (e.g., `EvidenceTimeline-Setup-v1.0.2.exe`)
- **Latest release builds are output to:** `D:\Projects\evidence_timeline\installer_output`
- Tag releases in git with semantic versioning (e.g., `v1.0.2`).

## Security & Configuration Tips
- Do not commit secrets or machine-specific settings. Use user-local config files if future integrations require keys.
- Stay on Windows-compatible APIs and verify new packages support the targeted `net10.0-windows` WPF environment.
