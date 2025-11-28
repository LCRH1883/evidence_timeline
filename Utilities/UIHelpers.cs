using System;
using WinForms = System.Windows.Forms;
using evidence_timeline.Views;

namespace evidence_timeline.Utilities
{
    public static class UIHelpers
    {
        public static string? PickFolder(string description = "Select a folder")
        {
            using var dialog = new WinForms.FolderBrowserDialog
            {
                Description = description,
                UseDescriptionForTitle = true
            };

            var result = dialog.ShowDialog();
            if (result == WinForms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                return dialog.SelectedPath;
            }

            return null;
        }

        public static string? PromptForText(string title, string prompt, string defaultValue = "")
        {
            var dialog = new InputDialog(title, prompt, defaultValue);
            var owner = System.Windows.Application.Current?.MainWindow;
            if (owner != null)
            {
                dialog.Owner = owner;
            }

            var result = dialog.ShowDialog();
            return result == true ? dialog.ResultText : null;
        }
    }
}
