using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace evidence_timeline.ViewModels
{
    public class SelectableItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Id { get; }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public SelectableItem(string id, string name, bool isSelected = false)
        {
            Id = id;
            _name = name;
            _isSelected = isSelected;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
