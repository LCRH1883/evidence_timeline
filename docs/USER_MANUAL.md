# Evidence Timeline - User Manual

**Version 1.1.1**
**Developed by Intagri Technologies LLC**
**© 2025 Intagri Technologies LLC. All rights reserved.**

---

## Table of Contents

1. [Introduction](#introduction)
2. [Getting Started](#getting-started)
3. [Case Management](#case-management)
4. [Evidence Management](#evidence-management)
5. [Notes and Documentation](#notes-and-documentation)
6. [Attachments](#attachments)
7. [Search and Filtering](#search-and-filtering)
8. [Customizing the Interface](#customizing-the-interface)
9. [Data Management](#data-management)
10. [Updates and Maintenance](#updates-and-maintenance)
11. [Troubleshooting](#troubleshooting)
12. [Keyboard Shortcuts](#keyboard-shortcuts)

---

## Introduction

### What is Evidence Timeline?

Evidence Timeline is a comprehensive case management application designed specifically for legal professionals. It helps you organize, manage, and analyze case evidence using an intuitive timeline-based interface.

### Key Benefits

- **Timeline Organization**: View all evidence chronologically
- **Rich Documentation**: Take formatted notes with attachments
- **Relationship Tracking**: Link related evidence and associate people
- **Offline Operation**: Works completely offline - your data stays local
- **Safe Updates**: Application updates never touch your case data
- **Flexible Organization**: Organize cases your way with customizable views

### System Requirements

- **Operating System**: Windows 10 or Windows 11
- **Runtime**: .NET 10.0 (installed automatically with the application)
- **Disk Space**: 200 MB for application, plus space for your case data
- **Memory**: 2 GB RAM minimum, 4 GB recommended
- **Display**: 1280x720 minimum resolution

---

## Getting Started

### Installation

1. Download the latest installer `EvidenceTimeline-Setup-v{version}.exe` from the [Releases page](https://github.com/LCRH1883/evidence_timeline/releases)
2. Run the installer
3. Follow the installation wizard prompts
4. Choose whether to create a desktop shortcut
5. Launch Evidence Timeline from the Start menu or desktop

### First Launch

When you first launch Evidence Timeline, you'll see the **Start Dialog**:

- **Create new case**: Start a brand new case
- **Browse**: Open an existing case folder
- **Recent cases**: Quick access to previously opened cases (empty on first launch)

### Creating Your First Case

1. Click **"Create new case"** on the start screen
2. Choose a folder location where you want to store this case
   - Recommended: Create a dedicated folder like `Documents\Cases\`
   - You can use network drives or external drives
3. Enter a case name (e.g., "State v. Smith 2025")
4. Click **OK**
5. The main window opens with your new, empty case

**Important**: The folder you choose will contain ALL data for this case. You can move or backup this folder anytime.

---

## Case Management

### Case Information

Every case has metadata that helps you organize and identify it:

- **Case Name**: The primary identifier (shown in window title)
- **Case Number**: Court-assigned case number
- **Court Location**: Which court/jurisdiction
- **Court Level**: District, Superior, Supreme, etc.

### Editing Case Information

1. Click the **"Case Info"** tab in the bottom pane
2. Enter or update the case information fields:
   - Case Number
   - Court Location
   - Court Level
3. Changes save automatically
4. Click the **"Save Case Info"** button to force save

### Viewing Case Statistics

Click the **"Info"** tab in the bottom pane to see:

- **Total Evidence**: Count of evidence items in the case
- **Case Created**: When the case was first created
- **Last Opened**: Most recent access date
- **Case Location**: Full path to the case folder on disk

### Opening Recent Cases

Evidence Timeline remembers your recently used cases:

1. Launch the application
2. Select a case from the **"Recent cases"** list
3. Click **"Open selected"**

To clear recent cases:
1. Go to **File → Preferences**
2. Click **"Clear recent cases"**

### Opening Cases from File Browser

If you prefer to navigate manually:

1. Click **"Browse..."** on the start screen
2. Navigate to the case folder
3. Select the folder and click **"Select Folder"**

### Multiple Cases

You can work with multiple cases:

- Each case is independent and isolated
- Open different case files in separate instances of the application
- No limit on the number of cases you can create

---

## Evidence Management

### Adding New Evidence

**Method 1: Toolbar Button**
1. Click the **+ (New Evidence)** button in the toolbar
2. Fill in the evidence details dialog
3. Click **Save**

**Method 2: Menu**
1. Go to **File → New Evidence**
2. Fill in details
3. Click **Save**

### Evidence Fields

When creating or editing evidence, you can set:

#### Basic Information
- **Evidence Number**: Auto-assigned, but editable (e.g., "E-001")
- **Title/Description**: Brief description of the evidence
- **Court Number**: Court-assigned exhibit number (if applicable)

#### Evidence Type
Choose from predefined types:
- Document
- Physical
- Digital
- Testimonial
- Video
- Audio
- Photo
- Other

To manage evidence types:
1. **Data → Types → Add Type**
2. **Data → Types → Rename Type**
3. **Data → Types → Delete Type**

#### Dates

Evidence Timeline supports flexible date tracking:

**Exact Date Mode**:
- Use when you know the precise date
- Select a single date from the calendar

**Around Date Mode**:
- Use for approximate dates
- Example: "Around 30 days from December 1, 2025"
- Set amount (number) and unit (days/weeks/months)

**Broad Date Range Mode**:
- Use for a time span
- Set start date and end date
- Example: "Between January 1 - January 31, 2025"

To change date mode:
1. Select evidence in the main list
2. In the right metadata pane, choose **Date mode** dropdown
3. Select: Exact, Around, or Broad
4. Fill in the appropriate date fields

### Editing Evidence

**Method 1: Double-Click**
- Double-click any evidence item in the timeline

**Method 2: Edit Button**
1. Select an evidence item
2. Click the **Edit** button in the toolbar

**Method 3: Metadata Pane**
1. Select an evidence item
2. Edit fields directly in the right **Metadata** pane
3. Changes auto-save after 2 seconds

### Deleting Evidence

1. Select an evidence item
2. Click the **Delete** button in the toolbar
3. Confirm the deletion
4. **Warning**: This action cannot be undone

### Linking Evidence

To connect related pieces of evidence:

**Method 1: Link Dialog**
1. Select an evidence item
2. Click **Edit → Manage Links**
3. Check the boxes next to related evidence
4. Click **Save**

**Method 2: Manual Entry**
1. Select an evidence item
2. In the metadata pane, scroll to **"Linked IDs (manual)"**
3. Enter evidence numbers separated by commas
   - Example: `E-002, E-005, E-012`
4. Press Enter or click away to save

### Associating People

Track which people are related to each piece of evidence:

#### Adding People to the Database
1. **Data → People → Add Person**
2. Enter the person's name
3. Click **OK**

The person is now available to associate with evidence.

#### Linking People to Evidence
1. Select an evidence item
2. In the metadata pane, scroll to the **People** section
3. Check the boxes next to relevant people
4. Changes save automatically

#### Managing People
- **Rename**: **Data → People → Rename Person**
- **Delete**: **Data → People → Delete Person** (removes from all evidence)

---

## Notes and Documentation

### Case Notes vs. Evidence Notes

Evidence Timeline supports two types of notes:

- **Case Notes**: General notes about the entire case
  - Access via the **Notes** tab in the bottom pane
- **Evidence Notes**: Specific notes for individual evidence items
  - Open an evidence item window to access its notes

### Taking Notes

1. Click in the notes area
2. Start typing
3. Use the formatting toolbar to style text
4. Notes auto-save every 2 seconds

### Text Formatting

The rich text editor supports:

| Format | Toolbar Button | Keyboard Shortcut |
|--------|----------------|-------------------|
| **Bold** | **B** | Ctrl+B |
| *Italic* | *I* | Ctrl+I |
| <u>Underline</u> | U | Ctrl+U |
| Bullet List | • List | - |
| Numbered List | 1. List | - |

### Creating Lists

**Bullet Lists**:
1. Click the bullet list button (•)
2. Type your first item
3. Press **Enter** to create the next bullet
4. Press **Enter** twice to exit the list

**Numbered Lists**:
1. Click the numbered list button (1.)
2. Type your first item
3. Press **Enter** to create the next number
4. Press **Enter** twice to exit the list

### Adjusting Font Size

To change the editor font size:

1. **View → Editor Font Size**
2. Choose from 6pt to 14pt (default is 9pt)
3. Setting saves per-case

---

## Attachments

### Adding File Attachments

1. Select an evidence item (or open the evidence window)
2. Scroll to the **Attachments** section in the metadata pane
3. Click **"Add attachment"**
4. Browse and select one or more files
5. Files are **copied** into the case folder

**Supported File Types**: All file types (documents, images, videos, etc.)

### Viewing Attachments

1. Locate the attachment in the **Attachments** list
2. Click the **Open** button (↗ icon)
3. The file opens in its default application

### Attachment Storage

Attachments are stored in:
```
YourCaseFolder/evidence/{evidence-id}/files/
```

Files are **copied**, not moved. Original files remain in their location.

### Showing Attachment Location

1. Click the **Folder** button (📁 icon) next to an attachment
2. Windows Explorer opens to show the file

### Removing Attachments

1. Click the **Delete** button (🗑 icon) next to an attachment
2. Confirm deletion
3. **Warning**: This permanently deletes the file from the case folder

---

## Search and Filtering

### Searching Evidence

1. Use the **Search** box in the filter bar
2. Type any text to search:
   - Evidence titles
   - Descriptions
   - Evidence numbers
   - Court numbers
3. Results filter in real-time as you type

### Filtering by Type

1. Use the **Type** dropdown in the filter bar
2. Select an evidence type
3. Only evidence of that type is shown

### Filtering by Person

1. Use the **Person** dropdown in the filter bar
2. Select a person's name
3. Only evidence associated with that person is shown

### Combining Filters

You can use multiple filters simultaneously:
- Search + Type filter
- Search + Person filter
- Type + Person filter
- All three together

### Clearing Filters

Click the **"Clear"** button in the filter bar to remove all filters and show all evidence.

### Sorting Evidence

Toggle sort order with:
- **View → Sort Newest First** (checked = newest first, unchecked = oldest first)

---

## Customizing the Interface

### Pane Visibility

Evidence Timeline has three adjustable panes:

**Right Metadata Pane**:
- **View → Show Metadata Pane** (toggle)
- Shows details for selected evidence

**Bottom Notes/Info Pane**:
- **View → Show Notes Pane** (toggle)
- Contains Notes, Case Info, and Info tabs

### Zoom Level

Adjust the entire interface scale:

1. **View → Zoom → {percentage}**
2. Choose from:
   - 50%
   - 75%
   - 100% (default)
   - 125%
   - 150%
   - 175%
   - 200%

Useful for:
- High-DPI displays
- Projectors or presentations
- Accessibility (larger text)

### Window Layout

Resize panes by dragging the splitters:
- **Vertical splitter**: Between evidence list and metadata pane
- **Horizontal splitter**: Between main area and bottom pane

**Note**: Pane sizes and visibility save per-case.

### Theme

Change the application theme:

1. **Settings → Preferences**
2. Select **Theme** dropdown
3. Choose Light or Dark
4. Click **Save**
5. Restart the application for changes to take effect

---

## Data Management

### Where is My Data Stored?

**Case Data**: Stored in the folder you chose when creating the case

**Application Settings**: Stored in `%AppData%\EvidenceTimeline\`

### Backing Up Your Cases

**Recommended Backup Strategy**:

1. **Identify case folders**: Find the folders where you created your cases
2. **Copy entire folders**: Copy the whole case folder (includes all evidence, notes, attachments)
3. **Backup locations**:
   - External hard drive
   - Network storage
   - Cloud backup service (OneDrive, Dropbox, etc.)

**Automated Backups**:
- Use Windows Backup or third-party backup software
- Add your case folders to regular backup schedules

### Moving Cases

To move a case to a different location:

1. **Close Evidence Timeline** (important!)
2. Use Windows Explorer to move the entire case folder
3. Reopen Evidence Timeline
4. Use **"Browse..."** to navigate to the new location
5. The case will reappear in your recent cases list

### Sharing Cases

To share a case with colleagues:

1. Close the case
2. Copy the entire case folder to a USB drive, network share, or cloud storage
3. Share the folder with your colleague
4. They can open it using **"Browse..."**

**Note**: Both users can work on separate copies, but changes won't sync automatically.

### Exporting Data

Case data is stored in human-readable formats:

- **JSON files** (.json): Case and evidence metadata
- **Markdown files** (.md): Notes
- **Original files**: Attachments

You can:
- Open JSON files in any text editor
- Open Markdown files in text editors or Markdown viewers
- Access attachments directly in the `files/` folders

---

## Updates and Maintenance

### Checking for Updates

**Automatic Checks** (Release builds only):
- Evidence Timeline checks for updates on startup
- If an update is available, you'll see a dialog with:
  - Current version
  - New version
  - Changelog/what's new
  - Options: Update, Skip, Remind Later

**Manual Checks**:
1. **Help → Check for Updates**
2. If up-to-date, you'll see a confirmation message
3. If an update is available, you'll see the update dialog

### Viewing Changelog

To see version history and changes:

1. **Help → View Changelog**
2. A window opens showing all version history
3. Review what's new in each release

### Installing Updates

When an update is available:

1. Click **"Yes"** or **"Update"** in the update dialog
2. The installer downloads automatically
3. The application closes
4. The installer runs
5. Follow the installation prompts
6. Launch the updated application

**Your data is safe**: Updates only replace application files, never your case data.

### Update Options

- **Update**: Download and install now
- **Skip this version**: Don't show this update again
- **Remind me in 7 days**: Check again in one week

---

## Troubleshooting

### Application Won't Start

**Solution 1: Check .NET Runtime**
- Ensure .NET 10.0 Runtime is installed
- Download from: https://dotnet.microsoft.com/download

**Solution 2: Reinstall**
1. Uninstall Evidence Timeline
2. Delete `%AppData%\EvidenceTimeline\`
3. Reinstall from the latest installer

### Case Won't Open

**Check the case folder**:
1. Navigate to the case folder
2. Verify `case.json` exists
3. If corrupted, restore from backup

**Reset application settings**:
1. Close Evidence Timeline
2. Delete `%AppData%\EvidenceTimeline\settings.json`
3. Restart the application

### Evidence Not Saving

**Check file permissions**:
- Ensure you have write permissions to the case folder
- Try moving the case to a different location (like Documents)

**Check disk space**:
- Ensure you have available disk space

### Attachments Won't Open

**File association issue**:
- The file type may not have a default program
- Right-click the file → **Open with** → Choose a program

**File location**:
- Files are in: `CaseFolder/evidence/{evidence-id}/files/`
- Open the folder directly to access files

### Updates Not Working

**Check internet connection**:
- Updates require internet to check for new versions

**Manual update**:
1. Go to: https://github.com/LCRH1883/evidence_timeline/releases
2. Download the latest installer manually
3. Run the installer

### Slow Performance

**Large cases**:
- Cases with 1000+ evidence items may slow down
- Consider splitting into multiple cases

**Attachments**:
- Very large attachments (videos, large PDFs) can slow the interface
- Store large files externally and link to them in notes

**System resources**:
- Close other applications
- Restart your computer
- Check for Windows updates

---

## Keyboard Shortcuts

### General

| Action | Shortcut |
|--------|----------|
| New Case | Ctrl+N |
| Open Case | Ctrl+O |
| Save Case | Ctrl+S |
| Exit | Alt+F4 |

### Text Formatting

| Action | Shortcut |
|--------|----------|
| Bold | Ctrl+B |
| Italic | Ctrl+I |
| Underline | Ctrl+U |

### Navigation

| Action | Shortcut |
|--------|----------|
| Focus Search | Ctrl+F |
| Next Evidence | Down Arrow |
| Previous Evidence | Up Arrow |

### View

| Action | Shortcut |
|--------|----------|
| Zoom In | Ctrl++ |
| Zoom Out | Ctrl+- |
| Reset Zoom | Ctrl+0 |

---

## Getting Help

### Documentation

- **User Manual**: This document
- **FAQ**: See [FAQ.md](FAQ.md) for common questions
- **Privacy Policy**: See [PRIVACY.md](../PRIVACY.md)
- **Changelog**: See [CHANGELOG.md](../CHANGELOG.md)

### Support

For support inquiries:

- **GitHub Issues**: https://github.com/LCRH1883/evidence_timeline/issues
- **Company**: Intagri Technologies LLC

When requesting support, include:
- Evidence Timeline version (Help → About)
- Windows version
- Description of the problem
- Steps to reproduce the issue

---

## Best Practices

### Case Organization

1. **Use descriptive case names**: Include year and case type
   - Good: "State v. Smith 2025"
   - Bad: "Case 1"

2. **Create cases in a dedicated folder**:
   - Example: `Documents\Cases\` or `D:\Legal\Cases\`

3. **Regular backups**: Back up case folders weekly or after major updates

### Evidence Documentation

1. **Complete metadata**: Fill in all relevant fields for each piece of evidence

2. **Consistent numbering**: Use a standard format (e.g., E-001, E-002)

3. **Detailed notes**: Document context, relevance, and chain of custody

4. **Link related evidence**: Connect related items using the link feature

### Data Security

1. **Use encryption**: Store sensitive cases on encrypted drives (BitLocker)

2. **Access control**: Use Windows user accounts to restrict access

3. **Secure backups**: Store backups in secure, encrypted locations

4. **Network drives**: For shared access, use network drives with proper permissions

---

**End of User Manual**

© 2025 Intagri Technologies LLC. All rights reserved.

For the latest version of this manual, visit:
https://github.com/LCRH1883/evidence_timeline/blob/main/docs/USER_MANUAL.md
