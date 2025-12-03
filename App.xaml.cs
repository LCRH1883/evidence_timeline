using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using AutoUpdaterDotNET;
using Microsoft.Win32;
using MediaColor = System.Windows.Media.Color;
using MediaBrush = System.Windows.Media.SolidColorBrush;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace evidence_timeline
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly Uri LightSchemeUri = new Uri("pack://application:,,,/AdonisUI;component/ColorSchemes/Light.xaml");
        private static readonly Uri DarkSchemeUri = new Uri("pack://application:,,,/AdonisUI;component/ColorSchemes/Dark.xaml");

        private bool _isManualUpdateCheck;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if !DEBUG
            // Configure AutoUpdater event handlers
            AutoUpdater.CheckForUpdateEvent += OnCheckForUpdateEvent;

            // Only check for updates in Release builds
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.ShowRemindLaterButton = true;
            AutoUpdater.LetUserSelectRemindLater = true;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
            AutoUpdater.RemindLaterAt = 7;

            // Check for updates from GitHub
            StartUpdateCheck(isManual: false);
#endif

            ApplyTheme(null);
        }

        private void OnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            var isManualCheck = _isManualUpdateCheck;
            _isManualUpdateCheck = false;

            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    // Show custom update dialog with changelog
                    ShowUpdateDialog(args);
                }
                else if (isManualCheck)
                {
                    // App is already on the latest version
                    var currentVersion = args.CurrentVersion?.ToString() ?? "Unknown";
                    MessageBox.Show(
                        $"You are already using the latest version ({currentVersion}).\n\nNo updates are available at this time.",
                        "Up to Date",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                if (args.Error is System.Net.WebException)
                {
                    MessageBox.Show(
                        "Unable to check for updates. Please check your internet connection and try again.",
                        "Update Check Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(
                        args.Error.Message,
                        "Update Check Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void StartUpdateCheck(bool isManual)
        {
            _isManualUpdateCheck = isManual;
            AutoUpdater.Start("https://raw.githubusercontent.com/LCRH1883/evidence_timeline/main/update.xml");
        }

        public void StartManualUpdateCheck()
        {
            StartUpdateCheck(isManual: true);
        }

        public void ApplyTheme(string? preferredTheme)
        {
            var theme = ResolveTheme(preferredTheme);
            ApplyColorScheme(theme);
            ApplyBrushPalette(theme);
        }

        private static string ResolveTheme(string? preferredTheme)
        {
            if (string.IsNullOrWhiteSpace(preferredTheme) || string.Equals(preferredTheme, "System", StringComparison.OrdinalIgnoreCase))
            {
                return GetSystemTheme();
            }

            return preferredTheme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";
        }

        private static string GetSystemTheme()
        {
            try
            {
                const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                using var personalize = Registry.CurrentUser.OpenSubKey(keyPath);
                if (personalize?.GetValue("AppsUseLightTheme") is int value)
                {
                    return value == 0 ? "Dark" : "Light";
                }
            }
            catch
            {
            }

            return "Light";
        }

        private void ApplyColorScheme(string theme)
        {
            var desiredScheme = theme.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? DarkSchemeUri : LightSchemeUri;
            var colorSchemeDictionary = Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("AdonisUI;component/ColorSchemes/"));

            if (colorSchemeDictionary == null)
            {
                Resources.MergedDictionaries.Insert(0, new ResourceDictionary { Source = desiredScheme });
            }
            else if (colorSchemeDictionary.Source != desiredScheme)
            {
                colorSchemeDictionary.Source = desiredScheme;
            }
        }

        private void ApplyBrushPalette(string theme)
        {
            if (theme.Equals("Dark", StringComparison.OrdinalIgnoreCase))
            {
                UpdateBrush("BorderBrush", MediaColor.FromRgb(0x4a, 0x4f, 0x56));
                UpdateBrush("BackgroundLight", MediaColor.FromRgb(0x2e, 0x32, 0x38));
                UpdateBrush("SurfaceBackground", MediaColor.FromRgb(0x1e, 0x1f, 0x22));
                UpdateBrush("TextPrimary", MediaColor.FromRgb(0xe7, 0xea, 0xf0));
                UpdateBrush("TextSecondary", MediaColor.FromRgb(0xc0, 0xc4, 0xcc));
                UpdateBrush("ControlBackground", MediaColor.FromRgb(0x28, 0x2c, 0x32));
                UpdateBrush("ControlHoverBackground", MediaColor.FromRgb(0x34, 0x38, 0x3f));
                UpdateBrush("ControlPressedBackground", MediaColor.FromRgb(0x3f, 0x44, 0x4b));
                UpdateBrush("IconButtonBackground", MediaColor.FromRgb(0x2f, 0x34, 0x3b));
                UpdateBrush("IconButtonHoverBackground", MediaColor.FromRgb(0x3a, 0x40, 0x48));
                UpdateBrush("IconButtonPressedBackground", MediaColor.FromRgb(0x45, 0x4c, 0x55));
                UpdateBrush("IconButtonBorder", MediaColor.FromRgb(0x56, 0x5c, 0x65));
                UpdateBrush("IconForeground", MediaColor.FromRgb(0xf7, 0xf8, 0xfa));
                UpdateBrush("TabHeaderBackground", MediaColor.FromRgb(0x2b, 0x2f, 0x35));
                UpdateBrush("TabHeaderSelectedBackground", MediaColor.FromRgb(0x23, 0x25, 0x29));
                UpdateBrush("TabHeaderBorder", MediaColor.FromRgb(0x3a, 0x3f, 0x45));
                UpdateBrush("MenuItemHoverBackground", MediaColor.FromRgb(0x1f, 0x22, 0x28));
                UpdateBrush("MenuItemHoverForeground", MediaColor.FromRgb(0xf9, 0xfa, 0xfc));
                UpdateBrush("SelectionBackground", MediaColor.FromRgb(0x1f, 0x22, 0x28));
                UpdateBrush("SelectionForeground", MediaColor.FromRgb(0xf9, 0xfa, 0xfc));
                UpdateBrush(System.Windows.SystemColors.HighlightBrushKey, MediaColor.FromRgb(0x1f, 0x22, 0x28));
                UpdateBrush(System.Windows.SystemColors.MenuHighlightBrushKey, MediaColor.FromRgb(0x1f, 0x22, 0x28));
                UpdateBrush(System.Windows.SystemColors.HighlightTextBrushKey, MediaColor.FromRgb(0xf9, 0xfa, 0xfc));
            }
            else
            {
                UpdateBrush("BorderBrush", MediaColor.FromRgb(0xde, 0xe2, 0xe6));
                UpdateBrush("BackgroundLight", MediaColor.FromRgb(0xf8, 0xf9, 0xfa));
                UpdateBrush("SurfaceBackground", MediaColor.FromRgb(0xff, 0xff, 0xff));
                UpdateBrush("TextPrimary", MediaColor.FromRgb(0x21, 0x25, 0x29));
                UpdateBrush("TextSecondary", MediaColor.FromRgb(0x6c, 0x75, 0x7d));
                UpdateBrush("ControlBackground", MediaColor.FromRgb(0xff, 0xff, 0xff));
                UpdateBrush("ControlHoverBackground", MediaColor.FromRgb(0xf0, 0xf2, 0xf5));
                UpdateBrush("ControlPressedBackground", MediaColor.FromRgb(0xe2, 0xe6, 0xea));
                UpdateBrush("IconButtonBackground", MediaColor.FromRgb(0xef, 0xf1, 0xf3));
                UpdateBrush("IconButtonHoverBackground", MediaColor.FromRgb(0xe1, 0xe5, 0xea));
                UpdateBrush("IconButtonPressedBackground", MediaColor.FromRgb(0xd4, 0xd9, 0xde));
                UpdateBrush("IconButtonBorder", MediaColor.FromRgb(0xc5, 0xcb, 0xd1));
                UpdateBrush("IconForeground", MediaColor.FromRgb(0x21, 0x25, 0x29));
                UpdateBrush("TabHeaderBackground", MediaColor.FromRgb(0xf0, 0xf2, 0xf5));
                UpdateBrush("TabHeaderSelectedBackground", MediaColor.FromRgb(0xff, 0xff, 0xff));
                UpdateBrush("TabHeaderBorder", MediaColor.FromRgb(0xce, 0xd4, 0xda));
                UpdateBrush("MenuItemHoverBackground", MediaColor.FromRgb(0xe5, 0xe9, 0xee));
                UpdateBrush("MenuItemHoverForeground", MediaColor.FromRgb(0x21, 0x25, 0x29));
                UpdateBrush("SelectionBackground", MediaColor.FromRgb(0xe2, 0xe8, 0xf0));
                UpdateBrush("SelectionForeground", MediaColor.FromRgb(0x11, 0x11, 0x11));
                UpdateBrush(System.Windows.SystemColors.HighlightBrushKey, MediaColor.FromRgb(0xe2, 0xe8, 0xf0));
                UpdateBrush(System.Windows.SystemColors.MenuHighlightBrushKey, MediaColor.FromRgb(0xe2, 0xe8, 0xf0));
                UpdateBrush(System.Windows.SystemColors.HighlightTextBrushKey, MediaColor.FromRgb(0x11, 0x11, 0x11));
            }
        }

        private void UpdateBrush(object key, MediaColor color)
        {
            if (Resources[key] is MediaBrush brush && !brush.IsFrozen)
            {
                brush.Color = color;
            }
            else
            {
                Resources[key] = new MediaBrush(color);
            }
        }

        private void ShowUpdateDialog(UpdateInfoEventArgs args)
        {
            // Load changelog content
            var newVersion = args.CurrentVersion?.ToString() ?? "Unknown";
            var installedVersion = args.InstalledVersion?.ToString() ?? "Unknown";
            var changelogContent = LoadChangelogForVersion(newVersion, installedVersion);

            // Show custom update dialog
            var result = MessageBox.Show(
                $"A new version ({newVersion}) is available!\n\n" +
                $"Current version: {installedVersion}\n" +
                $"New version: {newVersion}\n\n" +
                $"Changelog:\n\n{changelogContent}\n\n" +
                $"Would you like to download and install the update now?",
                "Update Available",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (AutoUpdater.DownloadUpdate(args))
                    {
                        System.Windows.Application.Current.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error downloading update: {ex.Message}",
                        "Update Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private string LoadChangelogForVersion(string? newVersion, string? currentVersion)
        {
            try
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var changelogPath = Path.Combine(appDirectory, "CHANGELOG.md");

                if (File.Exists(changelogPath))
                {
                    var content = File.ReadAllText(changelogPath);

                    // Extract relevant section (simple approach - first 500 chars)
                    // You could enhance this to parse markdown and extract specific version section
                    if (content.Length > 500)
                    {
                        return content.Substring(0, 500) + "...\n\n(View full changelog in Help > View Changelog)";
                    }
                    return content;
                }
                else
                {
                    return "See full changelog at: https://github.com/LCRH1883/evidence_timeline/blob/main/CHANGELOG.md";
                }
            }
            catch
            {
                return "Changelog not available. See GitHub repository for details.";
            }
        }
    }
}
