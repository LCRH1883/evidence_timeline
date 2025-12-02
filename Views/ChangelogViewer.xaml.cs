using System;
using System.IO;
using System.Windows;
using AdonisUI.Controls;

namespace evidence_timeline.Views
{
    public partial class ChangelogViewer : AdonisWindow
    {
        public ChangelogViewer()
        {
            InitializeComponent();
            LoadChangelog();
        }

        private void LoadChangelog()
        {
            try
            {
                // Get the changelog path relative to the executable
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                var changelogPath = Path.Combine(appDirectory, "CHANGELOG.md");

                if (File.Exists(changelogPath))
                {
                    var changelogContent = File.ReadAllText(changelogPath);
                    ChangelogTextBlock.Text = changelogContent;
                }
                else
                {
                    ChangelogTextBlock.Text = "Changelog file not found.\n\nThe CHANGELOG.md file should be located in the application directory.";
                }
            }
            catch (Exception ex)
            {
                ChangelogTextBlock.Text = $"Error loading changelog: {ex.Message}";
            }
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
