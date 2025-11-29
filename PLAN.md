# Development Plan (WPF, .NET 10)

Progress tracker for the Evidence Timeline app. Use the checkboxes to mark completion and update notes when finishing a step or hitting issues so we can resume easily.

Legend: `[ ]` pending · `[x]` done

## 0. Environment setup
- [ ] 0.1 Install .NET 10 SDK — Notes: verify `dotnet --version` shows 10.x once installed.
- [ ] 0.2 Install IDE — Notes: ensure Visual Studio with “.NET desktop development” (WPF templates) or Rider.
- [x] 0.3 Create solution and Git repo — Notes: `.git` exists with `evidence_timeline.slnx`.

## 1. Create base WPF project (.NET 10)
- [x] 1.1 Create WPF project — Notes: `evidence_timeline.csproj` targets `net10.0-windows` with WPF.
- [x] 1.2 Set basic project options — Notes: `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` set in `evidence_timeline.csproj`.
- [x] 1.3 Create base folder structure — Notes: created `Models/`, `Services/`, `ViewModels/`, `Views/`, `Utilities/`.

## 2. Define domain models and JSON contracts
- [x] 2.1 Core enums and supporting types — Notes: `EvidenceDateMode` and `EvidenceDateInfo` in `Models/EvidenceDateInfo.cs`.
- [x] 2.2 Case model — Notes: `CaseInfo` with `RootPath` ignored for JSON in `Models/CaseInfo.cs`.
- [x] 2.3 Evidence model — Notes: `Evidence` with defaults for lists, note file, timestamps in `Models/Evidence.cs`.
- [x] 2.4 Attachment model — Notes: `AttachmentInfo` in `Models/AttachmentInfo.cs`.
- [x] 2.5 People, tags, types — Notes: `Person`, `Tag`, `EvidenceType` in `Models/Person.cs`, `Models/Tag.cs`, `Models/EvidenceType.cs`.
- [x] 2.6 Evidence list view model — Notes: `EvidenceSummary` grid DTO in `Models/EvidenceSummary.cs`.

## 3. Path and JSON utilities
- [x] 3.1 Path helper — Notes: Slugging, case/evidence folder naming, ensure directory in `Utilities/PathHelper.cs`.
- [x] 3.2 JSON helper — Notes: CamelCase `DefaultOptions` + async load/save in `Utilities/JsonHelper.cs`.

## 4. Storage services
- [x] 4.1 Case storage service — Notes: `ICaseStorageService` and `CaseStorageService` handle create/load/save + initial files in `Services/`.
- [x] 4.2 Reference data service — Notes: `IReferenceDataService` and `ReferenceDataService` manage tags/types/people JSON in `Services/`.
- [x] 4.3 Evidence storage service — Notes: `IEvidenceStorageService` and `EvidenceStorageService` for evidence CRUD, folder naming, and evidence.json persistence in `Services/`.

## 5. MVVM setup and main application shell
- [x] 5.1 MVVM base — Notes: `ViewModels/BaseViewModel` with `SetProperty`; `RelayCommand` helper added.
- [x] 5.2 Main window layout — Notes: `MainWindow.xaml` scaffolds nav/filters, list grid, metadata pane, notes pane; `MainViewModel` provides bindings and stub commands.

## 6. Case creation/opening flow
- [x] 6.1 Start dialog/view — Notes: startup dialog prompts to create or open a case.
- [x] 6.2 Create case flow — Notes: folder picker + prompts wired in `MainViewModel` using storage services.
- [x] 6.3 Open case flow — Notes: folder picker loads case, refs, evidence summaries in `MainViewModel`.

## 7. Evidence list and search/filter
- [x] 7.1 Build `EvidenceSummary` list — Notes: summaries built with resolved names/search keys in `MainViewModel`.
- [x] 7.2 Evidence list view — Notes: DataGrid bound to `EvidenceList`, search box/filters active.
- [x] 7.3 Search/filter logic — Notes: text + tag/type/person filters applied in `MainViewModel.ApplyFilters`.

## 8. Metadata pane (right side)
- [x] 8.1 Metadata view model — Notes: `SelectedEvidenceDetail` bound with lookup-backed tag/person display.
- [x] 8.2 Metadata view — Notes: Title/Court/Type bound; tags/people selectable; dates (mode + fields), and linked evidence text editable.
- [x] 8.3 Save metadata — Notes: Save button persists metadata (title/court/type/tags/people/dates/links), refreshes summaries, and auto-saves on Enter/dropdown and tag/person toggles.

## 9. Notes pane (bottom)
- [x] 9.1 Notes loading — Notes: loads `note.md` per evidence selection in `MainViewModel`.
- [x] 9.2 Notes saving — Notes: Save command writes `note.md`; autosaves while typing in main/detail views.

## 10. Attachments support
- [x] 10.1 Attach files — Notes: Add attachment command copies to `files/`, records `AttachmentInfo`, saves evidence.
- [x] 10.2 Show attachments — Notes: attachments listed with open/folder/remove; per-item controls in metadata pane.

## 11. People, tags, and types management screens
- [x] 11.1 Tags management — Notes: add/rename/delete via left-pane buttons; evidence updated and summaries refreshed.
- [x] 11.2 Types management — Notes: add/rename/delete via left-pane buttons; evidence type cleared on delete and summaries refreshed.
- [x] 11.3 People management — Notes: add/rename/delete via left-pane buttons; evidence updated and summaries refreshed.

## 12. Evidence linking UI
- [x] 12.1 Model usage — Notes: `LinkedEvidenceIds` editable and saved via metadata save.
- [x] 12.2 UI — Notes: Manage links dialog + manual text field.
- [x] 12.3 Logic — Notes: selections saved, summaries refreshed (navigation still basic).

## 13. Evidence detail window (multi-window support)
- [x] 13.1 New window — Notes: `EvidenceWindow.xaml` + `EvidenceWindowViewModel` with metadata/notes/attachments.
- [x] 13.2 Open from main view — Notes: double-click and command open detail window for selected evidence.
- [x] 13.3 Synchronization — Notes: detail window save/notes events now refresh the main view selections and summaries.

## 14. Case settings and preferences
- [ ] 14.1 Case settings — Optional `caseSettings.json` (sort order, pane visibility).
- [ ] 14.2 Global settings — `settings.json` in app data (recent cases, theme optional).

## 15. Error handling and robustness
- [ ] 15.1 Basic error handling — Wrap I/O, surface dialogs on failures.
- [ ] 15.2 Validation — Enforce required fields and valid dates.

## 16. Performance sanity checks (1,000+ evidence items)
- [ ] 16.1 Synthetic case generator — Create 1k–5k evidence for load testing.
- [ ] 16.2 Manual testing — Verify load, search, scrolling performance.

## 17. Publish and packaging
- [ ] 17.1 Single-file publish — `dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true [-p:SelfContained=true]`.
- [ ] 17.2 Installer (optional) — ClickOnce/MSIX; single EXE acceptable for internal use.
