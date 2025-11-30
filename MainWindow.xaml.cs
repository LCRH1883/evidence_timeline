using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AdonisUI.Controls;
using evidence_timeline.ViewModels;
using evidence_timeline.Views;

namespace evidence_timeline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            if (vm.CurrentCase != null)
            {
                return;
            }

            var start = new StartDialog
            {
                Owner = this
            };

            var result = start.ShowDialog();
            if (result == true)
            {
                if (start.IsCreate)
                {
                    vm.CreateCaseCommand.Execute(null);
                }
                else
                {
                    vm.OpenCaseCommand.Execute(null);
                }
            }
        }

        private void OnEvidenceRowDoubleClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm || vm.SelectedSummary == null || vm.CurrentCase == null)
            {
                return;
            }

            var evidenceId = vm.SelectedSummary.Id;
            vm.TryOpenEvidenceWindow(evidenceId, this);
        }

        private void OnMetadataEnterKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave(true);
            }

            e.Handled = true;
        }

        private void OnMetadataSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave(true);
            }
        }

        private void OnMetadataFieldLostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave();
            }
        }

        private void OnMetadataCheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.CheckBox checkBox || checkBox.DataContext is not SelectableItem)
            {
                return;
            }

            if (DataContext is MainViewModel vm)
            {
                vm.RequestMetadataAutoSave();
            }
        }

        private void OnNotesRichTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.RichTextBox rtb || DataContext is not MainViewModel vm)
            {
                return;
            }

            // Load existing notes text into RichTextBox
            if (!string.IsNullOrEmpty(vm.NotesText))
            {
                LoadRtfFromPlainText(rtb, vm.NotesText);
            }
        }

        private void OnNotesTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.RichTextBox rtb || DataContext is not MainViewModel vm)
            {
                return;
            }

            // Save RichTextBox content back to view model
            vm.NotesText = GetRtfAsPlainText(rtb);
        }

        private void OnNotesFormatBold(object sender, RoutedEventArgs e)
        {
            if (NotesRichTextBox.Selection != null && !NotesRichTextBox.Selection.IsEmpty)
            {
                var currentWeight = NotesRichTextBox.Selection.GetPropertyValue(System.Windows.Documents.Inline.FontWeightProperty);
                var newWeight = (currentWeight is System.Windows.FontWeight fw && fw == System.Windows.FontWeights.Bold)
                    ? System.Windows.FontWeights.Normal
                    : System.Windows.FontWeights.Bold;
                NotesRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.Inline.FontWeightProperty, newWeight);
            }
        }

        private void OnNotesFormatItalic(object sender, RoutedEventArgs e)
        {
            if (NotesRichTextBox.Selection != null && !NotesRichTextBox.Selection.IsEmpty)
            {
                var currentStyle = NotesRichTextBox.Selection.GetPropertyValue(System.Windows.Documents.Inline.FontStyleProperty);
                var newStyle = (currentStyle is System.Windows.FontStyle fs && fs == System.Windows.FontStyles.Italic)
                    ? System.Windows.FontStyles.Normal
                    : System.Windows.FontStyles.Italic;
                NotesRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.Inline.FontStyleProperty, newStyle);
            }
        }

        private void OnNotesFormatUnderline(object sender, RoutedEventArgs e)
        {
            if (NotesRichTextBox.Selection != null && !NotesRichTextBox.Selection.IsEmpty)
            {
                var currentDecoration = NotesRichTextBox.Selection.GetPropertyValue(System.Windows.Documents.Inline.TextDecorationsProperty);
                var newDecoration = (currentDecoration is System.Windows.TextDecorationCollection tdc && tdc == System.Windows.TextDecorations.Underline)
                    ? null
                    : System.Windows.TextDecorations.Underline;
                NotesRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.Inline.TextDecorationsProperty, newDecoration);
            }
        }

        private void OnNotesFormatBulletList(object sender, RoutedEventArgs e)
        {
            if (NotesRichTextBox.Selection != null && !NotesRichTextBox.Selection.IsEmpty)
            {
                // Get selected text and split into lines
                var selectedText = NotesRichTextBox.Selection.Text;
                var lines = selectedText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Add bullet to each line
                var bulletedText = string.Join("\n", lines.Select(line => "• " + line.TrimStart()));

                NotesRichTextBox.Selection.Text = bulletedText;
            }
            else
            {
                // Insert bullet at current position
                NotesRichTextBox.CaretPosition.InsertTextInRun("• ");
            }
        }

        private void OnNotesFormatNumberedList(object sender, RoutedEventArgs e)
        {
            if (NotesRichTextBox.Selection != null && !NotesRichTextBox.Selection.IsEmpty)
            {
                // Get selected text and split into lines
                var selectedText = NotesRichTextBox.Selection.Text;
                var lines = selectedText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Add numbers to each line
                var numberedText = string.Join("\n", lines.Select((line, index) => $"{index + 1}. {line.TrimStart()}"));

                NotesRichTextBox.Selection.Text = numberedText;
            }
            else
            {
                // Insert number at current position
                NotesRichTextBox.CaretPosition.InsertTextInRun("1. ");
            }
        }

        private void LoadRtfFromPlainText(System.Windows.Controls.RichTextBox rtb, string text)
        {
            rtb.Document.Blocks.Clear();
            rtb.Document.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(text)));
        }

        private string GetRtfAsPlainText(System.Windows.Controls.RichTextBox rtb)
        {
            var textRange = new System.Windows.Documents.TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            return textRange.Text;
        }
    }
}
