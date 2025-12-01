# Application Icon Setup

## Current Status - ALL COMPLETE ✅

✅ **Window Icons**: All windows (MainWindow, StartDialog, EvidenceWindow) display the Intagri icon
✅ **Company Logo**: Start screen displays the Intagri logo (240x80px)
✅ **Executable Icon**: ICO file configured - `.exe` shows Intagri icon in Windows Explorer

## Implementation Complete

### Assets in Place
1. **assets/intagri icon.png** - Window icon for all windows (title bars)
2. **assets/intagri-icon.ico** - Executable icon (Windows Explorer)
3. **assets/Logo.png** - Company logo displayed on start screen (240x80px)

### Configuration Files
1. **evidence_timeline.csproj**:
   - `<ApplicationIcon>assets\intagri-icon.ico</ApplicationIcon>` - Executable icon
   - `<Resource Include="assets\**\*" />` - Embeds all assets

2. **StartDialog.xaml**:
   - `Icon="/assets/intagri icon.png"` - Window title bar icon
   - `<Image Source="/assets/Logo.png" Width="240" Height="80" />` - Large logo display

3. **MainWindow.xaml**:
   - `Icon="/assets/intagri icon.png"` - Window title bar icon

4. **EvidenceWindow.xaml**:
   - `Icon="/assets/intagri icon.png"` - Window title bar icon

## Window Icons (✅ Working)

The following windows now show the Intagri icon in their title bar:
- Start Dialog (startup window)
- Main Window (main application)
- Evidence Window (evidence details)

These use the PNG file directly via: `Icon="/assets/intagri icon.png"`

## Executable Icon (✅ Complete)

The `.exe` file icon is fully configured and working.

**Setup**:
- ICO file location: `assets/intagri-icon.ico`
- Project file: `<ApplicationIcon>assets\intagri-icon.ico</ApplicationIcon>`
- Result: Executable shows Intagri icon in Windows Explorer

### How to Update Icon (If Needed)
If you need to update the icon in the future:

1. Convert new PNG to ICO using online converter:
   - https://convertio.co/png-ico/ or https://www.icoconverter.com/
   - Select sizes: 16x16, 32x32, 48x48, 256x256 (multi-size ICO)

2. Replace `assets/intagri-icon.ico` with new file

3. Rebuild: `dotnet build`

## File Structure

```
assets/
├── intagri icon.png   ✅ (Window title bar icons)
├── intagri-icon.ico   ✅ (Executable icon)
└── Logo.png           ✅ (Start screen logo - 240x80px)
```

## Testing - All Verified ✅

### Window Icons
✅ Run the app - all windows show Intagri icon in title bar
✅ Start screen displays large Intagri logo (240x80px)

### Executable Icon
✅ Build Release version: `dotnet build -c Release`
✅ Navigate to `bin\Release\net10.0-windows\`
✅ `evidence_timeline.exe` shows Intagri icon in Windows Explorer

## Branding Summary

The application now has complete Intagri Technologies LLC branding:

1. **Start Screen**:
   - Large company logo (240x80px)
   - "Evidence Timeline" title
   - Copyright footer: "© 2025 Intagri Technologies LLC. All rights reserved."

2. **All Windows**:
   - Intagri icon in title bar

3. **Executable File**:
   - Intagri icon in Windows Explorer

4. **Help Menu**:
   - About dialog with company name and copyright
