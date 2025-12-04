using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdonisUI.Controls;
using evidence_timeline.Models;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using WinForms = System.Windows.Forms;

namespace evidence_timeline.Views
{
    public partial class NewEvidenceDialog : AdonisWindow
    {
        private readonly ObservableCollection<AttachmentSelection> _attachments = new();

        public NewEvidenceDialog(IEnumerable<EvidenceType> types)
        {
            InitializeComponent();
            DataContext = this;
            TypeBox.ItemsSource = types.ToList();
            DateModeBox.SelectedIndex = 0;
        }

        public Evidence? Result { get; private set; }
        public ObservableCollection<AttachmentSelection> Attachments => _attachments;
        public IReadOnlyList<string> AttachmentPaths => _attachments.Select(a => a.FullPath).ToList();

        private void OnCreateClicked(object sender, RoutedEventArgs e)
        {
            var title = TitleBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedTypeId = TypeBox.SelectedValue as string ?? string.Empty;
            var dateMode = (EvidenceDateMode)((DateModeBox.SelectedValue as EvidenceDateMode?) ?? EvidenceDateMode.Exact);
            int? aroundAmount = null;
            if (int.TryParse(AroundAmountBox.Text, out var parsed))
            {
                aroundAmount = parsed;
            }

            var dateInfo = new EvidenceDateInfo
            {
                Mode = dateMode,
                ExactDate = ExactDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(ExactDatePicker.SelectedDate.Value) : null,
                StartDate = StartDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(StartDatePicker.SelectedDate.Value) : null,
                EndDate = EndDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(EndDatePicker.SelectedDate.Value) : null,
                AroundAmount = aroundAmount,
                AroundUnit = GetAroundUnit(),
            };

            dateInfo.SortDate = ResolveSortDate(dateInfo);

            Result = new Evidence
            {
                Title = title,
                CourtNumber = CourtBox.Text.Trim(),
                TypeId = selectedTypeId,
                DateInfo = dateInfo
            };

            DialogResult = true;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void OnAddAttachmentsClicked(object sender, RoutedEventArgs e)
        {
            using var dialog = new WinForms.OpenFileDialog
            {
                Title = "Select attachment(s)",
                Multiselect = true
            };

            var result = dialog.ShowDialog();
            if (result != WinForms.DialogResult.OK || dialog.FileNames.Length == 0)
            {
                return;
            }

            foreach (var path in dialog.FileNames)
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    continue;
                }

                if (_attachments.Any(a => string.Equals(a.FullPath, path, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                _attachments.Add(new AttachmentSelection(path));
            }
        }

        private void OnRemoveAttachmentClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button { DataContext: AttachmentSelection selection })
            {
                return;
            }

            _attachments.Remove(selection);
        }

        private string GetAroundUnit()
        {
            if (AroundUnitBox.SelectedItem is ComboBoxItem item && item.Content is string text)
            {
                return text;
            }

            return AroundUnitBox.Text ?? string.Empty;
        }

        private static DateOnly ResolveSortDate(EvidenceDateInfo dateInfo)
        {
            return dateInfo.ExactDate
                ?? dateInfo.StartDate
                ?? dateInfo.EndDate
                ?? DateOnly.FromDateTime(DateTime.UtcNow);
        }

        public class AttachmentSelection
        {
            public AttachmentSelection(string fullPath)
            {
                FullPath = fullPath;
                FileName = Path.GetFileName(fullPath);
            }

            public string FullPath { get; }
            public string FileName { get; }
        }
    }
}
