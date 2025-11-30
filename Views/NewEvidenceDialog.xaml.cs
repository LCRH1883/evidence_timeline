using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AdonisUI.Controls;
using evidence_timeline.Models;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace evidence_timeline.Views
{
    public partial class NewEvidenceDialog : AdonisWindow
    {
        public NewEvidenceDialog(IEnumerable<EvidenceType> types)
        {
            InitializeComponent();
            TypeBox.ItemsSource = types.ToList();
            DateModeBox.SelectedIndex = 0;
        }

        public Evidence? Result { get; private set; }

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
    }
}
