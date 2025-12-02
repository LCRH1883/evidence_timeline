# Privacy Policy

**Evidence Timeline**
**Developed by Intagri Technologies LLC**
**Last Updated: December 1, 2025**

---

## Overview

Your privacy is important to us. This Privacy Policy explains how Evidence Timeline handles your data and what information is collected, stored, and transmitted when you use the application.

## TL;DR - Privacy Summary

**Evidence Timeline is a fully local, offline application:**
- ✅ **All your data stays on your computer**
- ✅ **We do NOT collect, access, or transmit your case data**
- ✅ **No accounts, logins, or user tracking**
- ✅ **No analytics or telemetry**
- ✅ **No advertisements**
- ✅ **No third-party data sharing**

The only internet connection used is for **checking for software updates** from our public GitHub repository.

---

## Data Storage and Ownership

### Your Data Belongs to You

All case data, evidence information, notes, and attachments are stored **locally on your computer** in folders that **you choose**. Intagri Technologies LLC does not have access to, collect, or store any of your case data.

### What Data is Stored Locally

#### 1. Case Data (User-Selected Folder)
When you create a case, you choose where to store it. This folder contains:

- **Case Information**: Case name, case number, court location, court level
- **Evidence Metadata**: Evidence descriptions, dates, types, court numbers
- **Notes**: All notes you write for cases and evidence
- **Attachments**: Files you attach to evidence items
- **Evidence Links**: Connections between related evidence
- **Person Associations**: People linked to evidence items
- **Case Settings**: UI preferences for that specific case (pane visibility, zoom, font size)

**Location**: Wherever you choose when creating a case
**Ownership**: You own and control this data
**Access**: Only you and users with access to your computer can access this data

#### 2. Application Settings (AppData)
The application stores minimal settings in your Windows user profile:

- **Recent Cases List**: Paths to recently opened cases for quick access
- **Last Opened Case**: The most recently used case
- **Application Preferences**: UI theme preference

**Location**:
- Release builds: `%AppData%\EvidenceTimeline\settings.json`
- Debug builds: `%AppData%\EvidenceTimeline-Dev\settings.json`

**Content**: Only file paths and UI preferences - no case data

---

## Data Collection

### We Do NOT Collect:

- ❌ Personal information (names, addresses, email, phone numbers)
- ❌ Case data or evidence information
- ❌ Notes or attachments
- ❌ User activity or behavior
- ❌ Analytics or telemetry
- ❌ Crash reports or error logs
- ❌ IP addresses or location data
- ❌ System information or device identifiers

### What We Access (Update Checks Only):

When the application checks for updates, it:

1. **Connects to GitHub** (https://raw.githubusercontent.com/LCRH1883/evidence_timeline/main/update.xml)
2. **Downloads a small XML file** containing version information
3. **Compares version numbers** to determine if an update is available
4. **If update available**: Downloads the installer from GitHub Releases (public URL)

**Data Transmitted**: Only the HTTP request to GitHub's servers (standard web request)

**GitHub's Privacy**: GitHub may log standard web server information (IP address, request time) as part of their service. See [GitHub's Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-privacy-statement) for details.

**Note**: Update checks are **disabled in Debug/Development builds** and only occur in Release builds.

---

## Internet Connectivity

### When Does the Application Connect to the Internet?

**Release Builds (Installed Version)**:
- On startup: Checks for software updates (can be skipped)
- Manual check: When you click Help → Check for Updates
- Update download: When you choose to download an available update

**Debug Builds (Development Version)**:
- Never connects to the internet (updates disabled)

### Can I Use the Application Offline?

**Yes!** The application works completely offline. All functionality is available without an internet connection:

- ✅ Create and manage cases
- ✅ Add and edit evidence
- ✅ Take notes and attach files
- ✅ Link evidence and associate people
- ✅ View case statistics and information

The only feature requiring internet is the **software update check**, which can be skipped or disabled.

---

## Third-Party Services

### GitHub (Update Distribution Only)

We use **GitHub Releases** to distribute software updates. When checking for updates:

- **Service**: GitHub.com (owned by Microsoft)
- **Purpose**: Hosting update information and installer files
- **Data Transmitted**: Standard HTTP request to public GitHub URLs
- **GitHub's Role**: GitHub may log web server requests as part of their service
- **Privacy Policy**: https://docs.github.com/en/site-policy/privacy-policies/github-privacy-statement

**No other third-party services are used.**

---

## Data Security

### Local Storage Security

Your data is secured by:

1. **Windows File System Permissions**: Only users with access to your Windows account can access the data
2. **No Cloud Storage**: Data is never transmitted to cloud servers
3. **No External Database**: All data stored in local JSON and Markdown files
4. **Separate Case Folders**: Each case is isolated in its own folder

### Recommendations for Enhanced Security

To protect sensitive case data:

- ✅ Use **Windows BitLocker** or other disk encryption
- ✅ Store cases on **encrypted drives** if handling sensitive information
- ✅ Use **strong Windows user account passwords**
- ✅ Enable **Windows Defender** or other antivirus software
- ✅ **Backup your case folders regularly** to prevent data loss
- ✅ Store backups on **encrypted external drives** or secure locations

---

## Updates and Data Safety

### Non-Destructive Updates

When you install a software update:

**What Gets Updated** (Application Only):
- Application executable files (.exe, .dll)
- Program resources (icons, UI files)
- Installation directory (e.g., C:\Program Files\Evidence Timeline\)

**What Is NEVER Touched** (Your Data):
- ✅ Case folders and all contents
- ✅ Evidence metadata and notes
- ✅ File attachments
- ✅ Application settings in AppData
- ✅ Recent cases list

**Why It's Safe**:
- User data is stored separately from the application
- Updates only replace program files, never data files
- You can reinstall or downgrade without losing data

---

## Data Retention and Deletion

### How Long Is Data Stored?

**Forever (Until You Delete It)**

The application does not automatically delete any data. All case data, evidence, notes, and attachments remain on your computer indefinitely until you manually delete them.

### How to Delete Your Data

**To Delete a Specific Case**:
1. Close the application
2. Navigate to the case folder (you chose this location when creating the case)
3. Delete the folder

**To Delete All Application Settings**:
1. Close the application
2. Navigate to `%AppData%\EvidenceTimeline\` (or `EvidenceTimeline-Dev` for debug builds)
3. Delete the `settings.json` file or entire folder

**To Completely Uninstall**:
1. Uninstall Evidence Timeline from Windows Settings → Apps
2. Delete any remaining case folders (wherever you created them)
3. Delete `%AppData%\EvidenceTimeline\` folder
4. All data is now removed from your computer

---

## Children's Privacy

Evidence Timeline is designed for professional legal use and is not intended for children under 13 years of age. We do not knowingly collect any information from children.

---

## Changes to This Privacy Policy

We may update this Privacy Policy from time to time. Changes will be:

- Posted in this file (PRIVACY.md) in the software repository
- Included in software updates
- Effective immediately upon posting

Continued use of the application after changes constitutes acceptance of the updated Privacy Policy.

---

## Your Rights and Control

You have complete control over your data:

- **Access**: You can access all your data at any time (stored in folders you chose)
- **Modify**: You can edit or update any data using the application
- **Export**: You can copy case folders to share or backup data
- **Delete**: You can delete any or all data at any time
- **Portability**: Case folders are portable - move them to any location or computer

---

## Legal Compliance

### Data Protection Regulations

Since Evidence Timeline:
- Does not collect personal data
- Does not transmit data to servers
- Stores all data locally on your computer
- Does not track users

It generally falls outside the scope of regulations like GDPR, CCPA, and similar data protection laws. However, **you are responsible** for ensuring your use of the software complies with any applicable laws regarding the data you store in it.

### Professional Responsibility

If you use Evidence Timeline for legal practice:
- Ensure compliance with attorney-client privilege requirements
- Follow bar association rules for data security
- Implement appropriate security measures for sensitive case data
- Maintain proper backups and disaster recovery procedures

---

## Contact Information

For questions about this Privacy Policy or data practices:

- **GitHub Issues**: https://github.com/LCRH1883/evidence_timeline/issues
- **Company**: Intagri Technologies LLC

---

## Transparency Commitment

Intagri Technologies LLC is committed to transparency:

- **Open Source Consideration**: While Evidence Timeline is proprietary, we provide detailed documentation about data handling
- **No Hidden Data Collection**: We explicitly state what data is and is not collected
- **Local-First Design**: Your data stays on your computer, period
- **User Control**: You have complete control over your data at all times

---

**Last Updated**: December 1, 2025
**Version**: 1.0

© 2025 Intagri Technologies LLC. All rights reserved.
