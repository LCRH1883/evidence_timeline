using System.Linq;
using System.Windows;
using AdonisUI.Controls;
using evidence_timeline.ViewModels;

namespace evidence_timeline.Views
{
    public partial class OpenRecentWindow : AdonisWindow
    {
        public string? SelectedPath { get; private set; }

        public OpenRecentWindow()
        {
            InitializeComponent();
        }

        private void OnOpenClicked(object sender, RoutedEventArgs e)
        {
            SelectedPath = RecentList.SelectedItem as string;
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnRecentCaseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (RecentList.SelectedItem == null)
            {
                return;
            }

            OnOpenClicked(sender, e);
        }
    }
}
