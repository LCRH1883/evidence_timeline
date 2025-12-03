using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using evidence_timeline.Models;

namespace evidence_timeline.ViewModels
{
    public class PreferencesViewModel : BaseViewModel
    {
        public PreferencesViewModel(AppSettings appSettings)
        {
            ThemeOptions = new ObservableCollection<string>(new[] { "System", "Light", "Dark" });
            SelectedTheme = string.IsNullOrWhiteSpace(appSettings.Theme) ? "System" : appSettings.Theme;
            RecentCases = new ObservableCollection<string>(appSettings.RecentCases);

            ClearRecentCasesCommand = new RelayCommand(ClearRecentCases, () => RecentCases.Any());
            RemoveRecentCaseCommand = new RelayCommand<string>(RemoveRecentCase);
        }

        public ObservableCollection<string> ThemeOptions { get; }
        public ObservableCollection<string> RecentCases { get; }

        private string _selectedTheme = "System";
        public string SelectedTheme
        {
            get => _selectedTheme;
            set => SetProperty(ref _selectedTheme, value);
        }

        public ICommand ClearRecentCasesCommand { get; }
        public ICommand RemoveRecentCaseCommand { get; }

        private void RemoveRecentCase(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            RecentCases.Remove(path);
            if (ClearRecentCasesCommand is RelayCommand relay)
            {
                relay.RaiseCanExecuteChanged();
            }

            OnPropertyChanged(nameof(RecentCases));
        }

        private void ClearRecentCases()
        {
            RecentCases.Clear();
            if (ClearRecentCasesCommand is RelayCommand relay)
            {
                relay.RaiseCanExecuteChanged();
            }
        }

        public AppSettings ToAppSettings()
        {
            return new AppSettings
            {
                Theme = SelectedTheme == "System" ? null : SelectedTheme,
                RecentCases = RecentCases.ToList()
            };
        }
    }
}
