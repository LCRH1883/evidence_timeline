using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using AutoUpdaterDotNET;
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
            AutoUpdater.Start("https://raw.githubusercontent.com/LCRH1883/evidence_timeline/main/update.xml");
#endif
        }

        private void OnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    // Show custom update dialog with changelog
                    ShowUpdateDialog(args);
                }
                else
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
