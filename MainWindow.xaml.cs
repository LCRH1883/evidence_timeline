using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using AdonisUI.Controls;
using evidence_timeline.ViewModels;
using evidence_timeline.Views;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace evidence_timeline
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        private DispatcherTimer? _metadataAutoSaveTimer;
        private DispatcherTimer? _notesAutoSaveTimer;
        private DateTime? _lastMetadataSaveTime;
        private DateTime? _lastNotesSaveTime;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            InitializeAutoSaveTimers();
        }

        private void InitializeAutoSaveTimers()
        {
            // Metadata autosave timer (2 second delay)
            _metadataAutoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _metadataAutoSaveTimer.Tick += OnMetadataAutoSaveTick;

            // Notes autosave timer (2 second delay)
            _notesAutoSaveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _notesAutoSaveTimer.Tick += OnNotesAutoSaveTick;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
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
                start.LoadRecentCases(vm.RecentCasePaths);

                var result = start.ShowDialog();
                if (result == true)
                {
                    if (start.IsCreate)
                    {
                        vm.CreateCaseCommand.Execute(null);
                    }
                    else if (!string.IsNullOrWhiteSpace(start.SelectedCasePath))
                    {
                        await vm.OpenCaseFromPathAsync(start.SelectedCasePath);
                    }
                    else
                    {
                        vm.OpenCaseCommand.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to start application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            TriggerMetadataAutoSave();
            e.Handled = true;
        }

        private void OnMetadataSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TriggerMetadataAutoSave();
        }

        private void OnMetadataFieldLostFocus(object sender, RoutedEventArgs e)
        {
            TriggerMetadataAutoSave();
        }

        private void OnMetadataCheckboxChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.CheckBox checkBox || checkBox.DataContext is not SelectableItem)
            {
                return;
            }

            TriggerMetadataAutoSave();
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
            if (EditingCommands.ToggleBullets.CanExecute(null, NotesRichTextBox))
            {
                EditingCommands.ToggleBullets.Execute(null, NotesRichTextBox);
            }
        }

        private void OnNotesFormatNumberedList(object sender, RoutedEventArgs e)
        {
            if (EditingCommands.ToggleNumbering.CanExecute(null, NotesRichTextBox))
            {
                EditingCommands.ToggleNumbering.Execute(null, NotesRichTextBox);
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

        private void TriggerMetadataAutoSave()
        {
            _metadataAutoSaveTimer?.Stop();
            _metadataAutoSaveTimer?.Start();
            UpdateMetadataSaveStatus("Saving...", System.Windows.Media.Brushes.Orange);
        }

        private void TriggerNotesAutoSave()
        {
            _notesAutoSaveTimer?.Stop();
            _notesAutoSaveTimer?.Start();
            UpdateNotesSaveStatus("Saving...", System.Windows.Media.Brushes.Orange);
        }

        private void OnMetadataAutoSaveTick(object? sender, EventArgs e)
        {
            _metadataAutoSaveTimer?.Stop();

            if (DataContext is MainViewModel vm && vm.SaveMetadataCommand != null)
            {
                try
                {
                    vm.SaveMetadataCommand.Execute(null);
                    _lastMetadataSaveTime = DateTime.Now;
                    UpdateMetadataSaveStatus($"Saved at {_lastMetadataSaveTime:HH:mm:ss}", System.Windows.Media.Brushes.Gray);
                }
                catch (Exception ex)
                {
                    UpdateMetadataSaveStatus("Save failed", System.Windows.Media.Brushes.Red);
                    System.Diagnostics.Debug.WriteLine($"Metadata save error: {ex.Message}");
                }
            }
        }

        private void OnNotesAutoSaveTick(object? sender, EventArgs e)
        {
            _notesAutoSaveTimer?.Stop();

            if (DataContext is MainViewModel vm && vm.SaveNotesCommand != null)
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

        private void UpdateMetadataSaveStatus(string text, System.Windows.Media.Brush foreground)
        {
            if (MetadataSaveStatusText != null)
            {
                MetadataSaveStatusText.Text = text;
                MetadataSaveStatusText.Foreground = foreground;
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

        private void OnAboutClicked(object sender, RoutedEventArgs e)
        {
            var aboutMessage = "Evidence Timeline\n" +
                             "Version 1.0.0\n\n" +
                             "Developed by Intagri Technologies LLC\n\n" +
                             "© 2025 Intagri Technologies LLC\n" +
                             "All rights reserved.";

            MessageBox.Show(aboutMessage, "About Evidence Timeline", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
