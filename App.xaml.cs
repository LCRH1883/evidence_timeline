using System.Configuration;
using System.Data;
using System.Windows;
using AutoUpdaterDotNET;

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
    }
}
