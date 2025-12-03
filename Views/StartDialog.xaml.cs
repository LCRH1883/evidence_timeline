using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using AdonisUI.Controls;
using System.Windows.Data;
using evidence_timeline.ViewModels;
using DataBinding = System.Windows.Data.Binding;

namespace evidence_timeline.Views
{
    public partial class StartDialog : AdonisWindow
    {
        public StartDialog()
        {
            InitializeComponent();
        }

        public bool IsCreate { get; private set; }
        public string? SelectedCasePath { get; private set; }

        private void OnCreateClicked(object sender, RoutedEventArgs e)
        {
            IsCreate = true;
            DialogResult = true;
        }

        private void OnOpenClicked(object sender, RoutedEventArgs e)
        {
            SelectedCasePath = RecentCasesList.SelectedItem as string;
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
            if (RecentCasesList.SelectedItem == null)
            {
                return;
            }

            OnOpenClicked(sender, e);
        }

        private void OnRemoveRecentCaseClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement fe || fe.DataContext is not string path)
            {
                return;
            }

            if (DataContext is MainViewModel vm)
            {
                vm.RemoveRecentCaseCommand?.Execute(path);
            }
        }
    }

    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Windows.Data.Binding.DoNothing;
        }
    }

    public class PathToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => DataBinding.DoNothing;
    }
}
