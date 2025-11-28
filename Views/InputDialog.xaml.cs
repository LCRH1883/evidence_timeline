using System.Windows;

namespace evidence_timeline.Views
{
    public partial class InputDialog : Window
    {
        public string ResultText { get; private set; } = string.Empty;

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            InitializeComponent();
            Title = title;
            PromptText.Text = prompt;
            InputBox.Text = defaultValue;
            Loaded += (_, _) => InputBox.Focus();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            ResultText = InputBox.Text;
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
