using System.Windows;

namespace evidence_timeline.Views
{
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow()
        {
            InitializeComponent();
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
