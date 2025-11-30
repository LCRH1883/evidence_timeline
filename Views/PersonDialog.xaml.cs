using System.Windows;
using AdonisUI.Controls;

namespace evidence_timeline.Views
{
    public partial class PersonDialog : AdonisWindow
    {
        public PersonDialog()
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
