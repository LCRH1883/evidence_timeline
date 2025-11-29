using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using evidence_timeline.Models;

namespace evidence_timeline.Views
{
    public partial class LinkEvidenceDialog : Window
    {
        private readonly List<EvidenceSummary> _all;
        private List<ListItem> _items = new();

        public LinkEvidenceDialog(IEnumerable<EvidenceSummary> evidence, IEnumerable<string> initiallySelected)
        {
            InitializeComponent();
            _all = evidence.ToList();
            PopulateList(initiallySelected);
        }

        public List<string> SelectedIds { get; private set; } = new();

        private void PopulateList(IEnumerable<string> selectedIds)
        {
            var selectedSet = new HashSet<string>(selectedIds ?? Enumerable.Empty<string>(), System.StringComparer.OrdinalIgnoreCase);

            _items = _all
                .Select(e => new ListItem
                {
                    Id = e.Id,
                    Text = $"#{e.EvidenceNumber} - {e.Title}",
                    IsSelected = selectedSet.Contains(e.Id)
                })
                .OrderBy(i => i.Text)
                .ToList();

            EvidenceListBox.ItemsSource = _items;
            foreach (var item in _items.Where(i => i.IsSelected))
            {
                EvidenceListBox.SelectedItems.Add(item);
            }
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            SelectedIds = EvidenceListBox.SelectedItems
                .OfType<ListItem>()
                .Select(i => i.Id)
                .ToList();
            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnSearchChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            var filtered = string.IsNullOrWhiteSpace(query)
                ? _items
                : _items.Where(i => i.Text.ToLowerInvariant().Contains(query)).ToList();
            EvidenceListBox.ItemsSource = filtered;
            foreach (var item in filtered.Where(i => i.IsSelected))
            {
                EvidenceListBox.SelectedItems.Add(item);
            }
        }

        private class ListItem
        {
            public string Id { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public bool IsSelected { get; set; }
        }
    }
}
