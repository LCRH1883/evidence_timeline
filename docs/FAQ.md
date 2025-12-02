# Frequently Asked Questions (FAQ)

**Evidence Timeline**
**Version 1.0.2**
**© 2025 Intagri Technologies LLC**

---

## Table of Contents

- [General Questions](#general-questions)
- [Installation & Setup](#installation--setup)
- [Privacy & Security](#privacy--security)
- [Case Management](#case-management)
- [Evidence & Data](#evidence--data)
- [Updates & Maintenance](#updates--maintenance)
- [Technical Issues](#technical-issues)
- [Features & Functionality](#features--functionality)

---

## General Questions

### What is Evidence Timeline?

Evidence Timeline is a Windows desktop application for legal professionals to organize, manage, and analyze case evidence using a timeline-based interface. It helps you track evidence, maintain notes, attach files, and manage case information all in one place.

### Who is Evidence Timeline for?

Evidence Timeline is designed for:
- Attorneys and legal professionals
- Paralegals and legal assistants
- Law enforcement investigators
- Legal researchers
- Anyone managing case-based evidence and documentation

### How much does it cost?

Pricing information and licensing details are available through Intagri Technologies LLC. Contact us via the GitHub repository for inquiries.

### What platforms does it run on?

Evidence Timeline currently runs on:
- Windows 10 (64-bit)
- Windows 11 (64-bit)

**Not supported**: macOS, Linux, mobile devices

### Do I need an internet connection?

**No!** Evidence Timeline works completely offline. The only internet requirement is for:
- Checking for software updates (optional)
- Downloading updates (optional)

All case data, evidence, and notes work without any internet connection.

---

## Installation & Setup

### How do I install Evidence Timeline?

1. Download the latest installer from the [Releases page](https://github.com/LCRH1883/evidence_timeline/releases)
2. Run `EvidenceTimeline-Setup-v{version}.exe`
3. Follow the installation wizard
4. Launch from the Start menu or desktop shortcut

### Do I need to install .NET separately?

No, the installer includes the .NET 10.0 Runtime. If it's not already on your system, it will be installed automatically.

### Can I install it without administrator rights?

The default installation to `C:\Program Files\` requires administrator rights. If you don't have admin access, contact your IT department.

### Can I install it on a network drive?

The application itself must be installed locally on your computer. However, you can store your case data on network drives or shared folders.

### How do I uninstall Evidence Timeline?

1. Windows Settings → Apps → Installed apps
2. Find "Evidence Timeline"
3. Click the three dots → Uninstall
4. Follow the uninstallation wizard

**Note**: Uninstalling does NOT delete your case data (it's stored in folders you chose).

---

## Privacy & Security

### Does Evidence Timeline collect my data?

**No.** Evidence Timeline does not collect, transmit, or access your case data. All information stays on your computer. See [PRIVACY.md](../PRIVACY.md) for complete details.

### Where is my data stored?

**Case data**: Stored in folders you choose when creating cases
**Application settings**: Stored in `%AppData%\EvidenceTimeline\`

### Is my data encrypted?

Evidence Timeline stores data in plain text files (JSON and Markdown). For encryption, we recommend:
- Windows BitLocker for full disk encryption
- Encrypted external drives for storing cases
- Encrypted network shares for shared access

### Can Evidence Timeline access my cases remotely?

**No.** There is no remote access capability. Intagri Technologies LLC cannot access your data.

### Is Evidence Timeline GDPR/CCPA compliant?

Since Evidence Timeline:
- Doesn't collect personal data
- Doesn't transmit data to servers
- Stores all data locally

It generally falls outside the scope of GDPR/CCPA. However, **you** are responsible for ensuring your use complies with applicable regulations.

### Can I use it for attorney-client privileged information?

Yes, but **you** are responsible for:
- Implementing appropriate security measures (encryption, access controls)
- Following bar association rules for data security
- Maintaining proper backups and disaster recovery procedures
- Complying with ethical obligations regarding client data

Evidence Timeline provides the tools; you must implement appropriate security practices.

---

## Case Management

### How many cases can I create?

There is no limit. You can create as many cases as your disk space allows.

### Can I work on multiple cases at once?

Yes! You can open multiple instances of Evidence Timeline, each with a different case.

### Can I move a case to a different folder?

Yes:
1. Close Evidence Timeline
2. Use Windows Explorer to move the entire case folder
3. Reopen Evidence Timeline
4. Use "Browse..." to navigate to the new location

### Can I rename a case?

**Case folder**: Yes, you can rename the folder (close the app first)
**Case name** (displayed in the app): Go to Case Info tab and edit the case name

### How do I delete a case?

1. Close Evidence Timeline
2. Navigate to the case folder in Windows Explorer
3. Delete the folder
4. **Warning**: This permanently deletes all evidence, notes, and attachments

### Can I share a case with a colleague?

Yes:
1. Close the case
2. Copy the entire case folder to a USB drive, network share, or cloud storage
3. Share the folder
4. Your colleague can open it using "Browse..."

**Note**: If both work on separate copies, changes won't sync automatically.

---

## Evidence & Data

### How many evidence items can I add?

There is no hard limit. However, performance may slow down with 1000+ items in a single case. Consider splitting very large cases.

### What file types can I attach?

All file types are supported: documents, images, videos, audio, PDFs, spreadsheets, etc.

### How large can attachments be?

There is no size limit, but very large files (100+ MB) may slow down the interface. For large video files, consider:
- Storing them externally
- Linking to them in notes with file paths

### Are attachments moved or copied?

**Copied.** When you attach a file, it's copied into the case folder. Original files remain in their location.

### Can I delete an attachment without deleting the evidence?

Yes. Click the delete button (🗑) next to the attachment in the attachments list.

### How do I export evidence data?

Case data is stored in open formats:
- **JSON files**: Case and evidence metadata (open with any text editor)
- **Markdown files**: Notes (open with text editors)
- **Original files**: Attachments in the `files/` folders

You can copy these files directly or use tools to convert them.

### Can I import data from other systems?

Not currently. You would need to manually enter data or create JSON files matching the Evidence Timeline format.

---

## Updates & Maintenance

### How do updates work?

**Automatic checks** (Release builds):
- Checks on startup
- Shows dialog if update available

**Manual checks**:
- Help → Check for Updates

### Will updates delete my data?

**No!** Updates only replace application files, never your case data. See [PRIVACY.md](../PRIVACY.md) for details on what gets updated vs. what's preserved.

### Can I skip an update?

Yes. When the update dialog appears, you can:
- Skip this version
- Remind me later (in 7 days)
- Update now

### How do I update manually?

1. Go to https://github.com/LCRH1883/evidence_timeline/releases
2. Download the latest installer
3. Run it to install over your current version

### What happens if an update fails?

- Your data is safe (it's stored separately)
- Reinstall the previous version from the [Releases page](https://github.com/LCRH1883/evidence_timeline/releases)
- Report the issue on GitHub Issues

### Where can I see what's new in each version?

- **Help → View Changelog** (in the application)
- **[CHANGELOG.md](../CHANGELOG.md)** (in the repository)

---

## Technical Issues

### The application won't start. What should I do?

**Try these steps**:

1. **Restart your computer**
2. **Check .NET Runtime**: Ensure .NET 10.0 is installed
   - Download: https://dotnet.microsoft.com/download
3. **Reinstall Evidence Timeline**:
   - Uninstall completely
   - Delete `%AppData%\EvidenceTimeline\`
   - Reinstall from latest installer

### A case won't open. What's wrong?

**Check the folder**:
- Navigate to the case folder
- Verify `case.json` exists and isn't corrupted
- If corrupted, restore from backup

**Reset recent cases**:
- File → Preferences → Clear recent cases
- Use "Browse..." to navigate to the case folder directly

### Evidence isn't saving. Help!

**Check permissions**:
- Ensure you have write permissions to the case folder
- Try moving the case to `Documents\`

**Check disk space**:
- Ensure you have available disk space

**Check file locks**:
- Make sure the case isn't opened in multiple instances
- Close other programs that might lock files (antivirus, backup software)

### Attachments won't open. Why?

**File association**:
- The file type may not have a default program
- Right-click the file → "Open with" → Choose a program

**Missing programs**:
- Install the program needed to open the file type
- Example: Install Adobe Reader for PDFs

**Manual access**:
- Open: `CaseFolder/evidence/{evidence-id}/files/`
- Access files directly

### The interface is slow. How can I speed it up?

**Try these**:

1. **Close other applications**
2. **Reduce evidence count** (split large cases)
3. **Minimize large attachments** (store videos externally)
4. **Restart your computer**
5. **Check for Windows updates**
6. **Disable animations** (Windows Settings → Ease of Access → Display)

### I accidentally deleted evidence. Can I recover it?

**If you have a backup**:
- Restore the case folder from backup

**If no backup**:
- Evidence deletion is permanent
- Check Windows Recycle Bin (only if you deleted the entire case folder)

**Prevention**:
- Regular backups!
- Before major deletions, backup the case folder

---

## Features & Functionality

### Can I print evidence or reports?

Not directly from the application currently. To print:

**Workaround**:
1. Open the evidence window
2. Copy notes and information
3. Paste into Word or another document
4. Print from there

**Attachments**:
- Open attached documents
- Print from their native applications

### Can I export to PDF?

Not directly. Use the printing workaround above and "Print to PDF" in your word processor.

### Can I customize evidence types?

Yes:
- **Data → Types → Add Type** (add new)
- **Data → Types → Rename Type** (rename existing)
- **Data → Types → Delete Type** (remove)

### Can I add custom fields to evidence?

Not currently. Evidence fields are fixed. Use the notes section for additional information.

### Does it support multi-user collaboration?

No, not with simultaneous editing. However, you can:
- Store cases on network shares
- Take turns editing (not simultaneously)
- Share case folders via cloud storage

**Future consideration**: Multi-user features may be added in future versions.

### Can I integrate with other legal software?

Not currently. Evidence Timeline is a standalone application. Integration with other software may be considered for future versions.

### Can I add photographs or images inline in notes?

Not currently. You can:
- Attach images as file attachments
- Reference them in notes (e.g., "See attached photo IMG_001.jpg")

### Is there a mobile version?

Not currently. Evidence Timeline is Windows-only. Mobile versions may be considered for future development.

### Can I set reminders or deadlines?

Not currently. Use Windows Calendar or other task management tools for reminders.

### Can I track time spent on a case?

Not currently. Use dedicated time-tracking software and reference case names.

---

## Backup & Recovery

### How should I backup my cases?

**Best practices**:

1. **Identify case folders**: Know where you created each case
2. **Regular schedule**: Weekly or after major updates
3. **Multiple locations**:
   - External hard drive
   - Network storage
   - Cloud backup (OneDrive, Dropbox, Google Drive)

**Automated backups**:
- Use Windows Backup or third-party tools
- Add case folders to backup schedules

### What should I backup?

**Required**:
- Entire case folders (everything inside)

**Optional**:
- `%AppData%\EvidenceTimeline\settings.json` (application preferences)

### How do I restore from backup?

1. Close Evidence Timeline
2. Copy the backed-up case folder to your desired location
3. Open Evidence Timeline
4. Use "Browse..." to open the restored case folder

### Can I use cloud storage for cases?

**Yes, but with caution**:

**Supported cloud storage**:
- OneDrive
- Dropbox
- Google Drive
- Network drives

**Considerations**:
- Ensure proper sync before closing the application
- Don't open the same case on multiple computers simultaneously
- Use encryption for sensitive cases
- Check bar association rules for cloud storage of client data

### What if my hard drive fails?

**If you have backups**:
- Restore case folders from backup to a new drive

**If no backups**:
- Data may be unrecoverable
- Consider professional data recovery services (expensive)

**Prevention**:
- Regular backups are essential!
- Use reliable storage (SSDs, redundant drives)

---

## Licensing & Legal

### What license does Evidence Timeline use?

Evidence Timeline is proprietary software. See [LICENSE](../LICENSE) for complete terms.

### Can I use it for commercial purposes?

Yes, subject to the license agreement. Contact Intagri Technologies LLC for commercial licensing information.

### Can I modify or customize the application?

The software is proprietary and may not be modified or reverse-engineered. See the LICENSE file for details.

### Where can I report bugs or request features?

- **GitHub Issues**: https://github.com/LCRH1883/evidence_timeline/issues

Please include:
- Evidence Timeline version
- Windows version
- Detailed description of the bug or feature request

---

## Getting More Help

### Where can I find more documentation?

- **[User Manual](USER_MANUAL.md)** - Complete guide
- **[README.md](../README.md)** - Project overview and quick start
- **[PRIVACY.md](../PRIVACY.md)** - Privacy policy
- **[CHANGELOG.md](../CHANGELOG.md)** - Version history

### How do I contact support?

- **GitHub Issues**: https://github.com/LCRH1883/evidence_timeline/issues
- **Company**: Intagri Technologies LLC

### Is there a community forum?

Check the GitHub repository for community discussions and announcements.

### Can I contribute to development?

Evidence Timeline is proprietary software. For contributions or collaboration inquiries, contact Intagri Technologies LLC.

---

**Still have questions?**

Open an issue on GitHub: https://github.com/LCRH1883/evidence_timeline/issues

---

© 2025 Intagri Technologies LLC. All rights reserved.

**Last Updated**: December 1, 2025
