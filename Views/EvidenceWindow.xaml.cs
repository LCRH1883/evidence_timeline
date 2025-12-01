using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AdonisUI.Controls;
using evidence_timeline.ViewModels;

namespace evidence_timeline.Views
{
    public partial class EvidenceWindow : AdonisWindow
    {
        private DispatcherTimer? _notesAutoSaveTimer;
        private DispatcherTimer? _mainSaveTimer;
        private DateTime? _lastNotesSaveTime;
        private DateTime? _lastMainSaveTime;

        public EvidenceWindow()
        {
            InitializeComponent();
            InitializeAutoSaveTimers();
        }

        private void InitializeAutoSaveTimers()
        {
            // Main save autosave timer (2 second delay)
            _mainSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _mainSaveTimer.Tick += OnMainAutoSaveTick;

            // Notes autosave timer (2 second delay)
            _notesAutoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _notesAutoSaveTimer.Tick += OnNotesAutoSaveTick;
        }

        private void OnMetadataEnterKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            TriggerMainAutoSave();
            e.Handled = true;
        }

        private void OnMetadataSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TriggerMainAutoSave();
        }

        private void OnMetadataFieldLostFocus(object sender, RoutedEventArgs e)
        {
            TriggerMainAutoSave();
        }

        private void OnMetadataCheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.CheckBox checkBox || checkBox.DataContext is not SelectableItem)
            {
                return;
            }

            TriggerMainAutoSave();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void OnCloseClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnNotesRichTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.RichTextBox rtb || DataContext is not EvidenceWindowViewModel vm)
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
            if (sender is not System.Windows.Controls.RichTextBox rtb || DataContext is not EvidenceWindowViewModel vm)
            {
                return;
            }

            // Save RichTextBox content back to view model
            vm.NotesText = GetRtfAsPlainText(rtb);

            // Trigger autosave
            TriggerNotesAutoSave();
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

        private void TriggerMainAutoSave()
        {
            _mainSaveTimer?.Stop();
            _mainSaveTimer?.Start();
            UpdateMainSaveStatus("Saving...", System.Windows.Media.Brushes.Orange);
        }

        private void TriggerNotesAutoSave()
        {
            _notesAutoSaveTimer?.Stop();
            _notesAutoSaveTimer?.Start();
            UpdateNotesSaveStatus("Saving...", System.Windows.Media.Brushes.Orange);
        }

        private void OnMainAutoSaveTick(object? sender, EventArgs e)
        {
            _mainSaveTimer?.Stop();

            if (DataContext is EvidenceWindowViewModel vm && vm.SaveCommand != null)
            {
                try
                {
                    vm.SaveCommand.Execute(null);
                    _lastMainSaveTime = DateTime.Now;
                    UpdateMainSaveStatus($"Saved at {_lastMainSaveTime:HH:mm:ss}", System.Windows.Media.Brushes.Gray);
                }
                catch (Exception ex)
                {
                    UpdateMainSaveStatus("Save failed", System.Windows.Media.Brushes.Red);
                    System.Diagnostics.Debug.WriteLine($"Main save error: {ex.Message}");
                }
            }
        }

        private void OnNotesAutoSaveTick(object? sender, EventArgs e)
        {
            _notesAutoSaveTimer?.Stop();

            if (DataContext is EvidenceWindowViewModel vm && vm.SaveNotesCommand != null)
            {
                try
                {
                    vm.SaveNotesCommand.Execute(null);
                    _lastNotesSaveTime = DateTime.Now;
                    UpdateNotesSaveStatus($"Saved at {_lastNotesSaveTime:HH:mm:ss}", System.Windows.Media.Brushes.Gray);
                }
                catch (Exception ex)
                {
                    UpdateNotesSaveStatus("Save failed", System.Windows.Media.Brushes.Red);
                    System.Diagnostics.Debug.WriteLine($"Notes save error: {ex.Message}");
                }
            }
        }

        private void UpdateMainSaveStatus(string text, System.Windows.Media.Brush foreground)
        {
            if (SaveStatusText != null)
            {
                SaveStatusText.Text = text;
                SaveStatusText.Foreground = foreground;
            }
        }

        private void UpdateNotesSaveStatus(string text, System.Windows.Media.Brush foreground)
        {
            if (NotesSaveStatusText != null)
            {
                NotesSaveStatusText.Text = text;
                NotesSaveStatusText.Foreground = foreground;
            }
        }
    }
}
