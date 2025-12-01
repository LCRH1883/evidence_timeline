using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using AdonisUI.Controls;

namespace evidence_timeline.Views
{
    public partial class StartDialog : AdonisWindow
    {
        public ObservableCollection<RecentCaseEntry> RecentCases { get; } = new();

        public StartDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool IsCreate { get; private set; }
        public string? SelectedCasePath { get; private set; }
        public RecentCaseEntry? SelectedCase { get; set; }

        private void OnCreateClicked(object sender, RoutedEventArgs e)
        {
            IsCreate = true;
            DialogResult = true;
        }

        private void OnOpenClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedCase != null)
            {
                SelectedCasePath = SelectedCase.FullPath;
            }
            else
            {
                SelectedCasePath = null;
            }

            IsCreate = false;
            DialogResult = true;
        }

        private void OnBrowseClicked(object sender, RoutedEventArgs e)
        {
            SelectedCasePath = null;
            IsCreate = false;
            DialogResult = true;
        }

        private void OnRecentCaseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedCase == null)
            {
                return;
            }

            OnOpenClicked(sender, e);
        }

        public void LoadRecentCases(IEnumerable<string> paths)
        {
            RecentCases.Clear();
            foreach (var path in paths ?? Enumerable.Empty<string>())
            {
                RecentCases.Add(new RecentCaseEntry(path));
            }
        }

        public class RecentCaseEntry
        {
            public RecentCaseEntry(string path)
            {
                FullPath = path;
                Name = System.IO.Path.GetFileName(path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
            }

            public string Name { get; }
            public string FullPath { get; }
        }
    }

    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Windows.Data.Binding.DoNothing;
        }
    }
}
