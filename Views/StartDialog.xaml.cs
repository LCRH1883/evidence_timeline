using System.Windows;
using AdonisUI.Controls;

namespace evidence_timeline.Views
{
    public partial class StartDialog : AdonisWindow
    {
        public StartDialog()
        {
            InitializeComponent();
        }

        public bool IsCreate { get; private set; }

        private void OnCreateClicked(object sender, RoutedEventArgs e)
        {
            IsCreate = true;
            DialogResult = true;
        }

        private void OnOpenClicked(object sender, RoutedEventArgs e)
        {
            IsCreate = false;
            DialogResult = true;
        }
    }
}
