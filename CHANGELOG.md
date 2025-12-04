# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.3] - 2025-12-02

### Added
- People manager window with add/edit/delete, notes, and aliases
- Evidence creation now uses the full Evidence window with immediate notes/attachments
- Resizable splitters in the Evidence window between notes/metadata and metadata/attachments
- Default evidence types seeded on new cases (Document(s), Contract, Letter, Email, Message(s), Photograph, Receipt, Audio, Video)
- Drag-and-drop support for adding attachments directly into evidence

### Fixed
- Attachment open behavior now honors default apps (HTML in browser, media in associated players)
- Evidence type rename reliability and display in Case Settings
- Dialog/window ownership now uses the active window to keep popups on the correct monitor

## [1.1.2] - 2025-12-02

### Added
- Drag-and-drop support for adding attachments directly into evidence
- New cases start with default evidence types (Document(s), Contract, Letter, Email, Message(s), Photograph, Receipt, Audio, Video)

### Fixed
- Attachments now open with the correct default application (HTML in browser, media in associated players)
- Evidence type rename flows correctly update and save without crashes

### Added
- Changelog viewer window accessible from Help menu
- "View Changelog" menu item in Help menu for viewing version history
- Enhanced update notification system that displays changelog content
- "Already up-to-date" message when no updates are available
- CHANGELOG.md file for tracking version history and changes

### Changed
- Update dialog now displays changelog content instead of just linking to GitHub
- Improved update experience with inline changelog preview
- CHANGELOG.md is now included in application build output

## [1.0.1] - 2025-12-01

### Added
- Windows installer for application distribution
- Auto-update feature with GitHub integration
- Company branding and auto-update configuration
- GitHub Actions workflow for GitHub Pages deployment

### Changed
- Bumped application version to 1.0.1

## [1.0.0] - Initial Release

### Added
- Tabbed bottom pane with case information display
- Attachment management functionality
- Editor font size configuration setting
- Placeholder text for DatePicker controls
- AdonisUI integration for modern UI appearance
- Rich text support for evidence descriptions
- Case settings and user preferences management
- Event-driven handling for tag and person options
- Case and evidence management dialogs
- Metadata editing and save functionality
- Evidence date resolution and sorting
- MVVM architecture with WPF
- Timeline-based evidence organization

### Changed
- Unified UI styling across application
- Modernized interface with AdonisUI theme
- Improved user experience with autosave functionality

### Removed
- Legacy tag system (replaced with enhanced metadata)

---

## Version Tracking

**Current Version:** 1.1.3
**Repository:** https://github.com/LCRH1883/evidence_timeline

### Version History
- **1.1.3** - People manager, evidence create window, dialog fixes, and attachment improvements
- **1.1.2** - Attachment improvements and evidence type reliability
- **1.1.1** - Enhanced update system with changelog integration
- **1.0.1** - Auto-update and installer release
- **1.0.0** - Initial public release

---

## How to Update This File

When releasing a new version:

1. Add a new version section at the top (below the "Unreleased" section if using one)
2. Include the version number and release date
3. Organize changes into categories:
   - **Added** - New features
   - **Changed** - Changes to existing functionality
   - **Deprecated** - Soon-to-be removed features
   - **Removed** - Removed features
   - **Fixed** - Bug fixes
   - **Security** - Security improvements or fixes
4. Update the "Current Version" in the Version Tracking section
5. Commit the changelog with the version bump

[1.1.3]: https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.1.3
[1.1.2]: https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.1.2
[1.1.1]: https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.1.1
[1.0.1]: https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.0.1
[1.0.0]: https://github.com/LCRH1883/evidence_timeline/releases/tag/v1.0.0
