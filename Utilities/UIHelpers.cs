using System;
using System.Linq;
using System.Windows;
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

            var ownerWindow = System.Windows.Application.Current?.Windows.OfType<System.Windows.Window>()
                .FirstOrDefault(w => w.IsActive) ?? System.Windows.Application.Current?.MainWindow;

            WinForms.DialogResult result;
            if (ownerWindow != null)
            {
                var win32Owner = new Wpf32Window(ownerWindow);
                result = dialog.ShowDialog(win32Owner);
            }
            else
            {
                result = dialog.ShowDialog();
            }

            if (result == WinForms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                return dialog.SelectedPath;
            }

            return null;
        }

        public static string? PromptForText(string title, string prompt, string defaultValue = "", Window? owner = null)
        {
            // Determine the active window; fall back to the main window if no window is active.
            var ownerWindow = owner
                ?? System.Windows.Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                ?? System.Windows.Application.Current?.MainWindow;

            var dialog = new InputDialog(title, prompt, defaultValue);
            dialog.Owner = ownerWindow;

            var result = dialog.ShowDialog();
            return result == true ? dialog.ResultText : null;
        }

        public static Window? GetActiveWindow()
        {
            var app = System.Windows.Application.Current;
            if (app == null)
            {
                return null;
            }

            var windows = app.Windows.OfType<Window>().Where(w => w.IsVisible).ToList();
            if (windows.Count == 0)
            {
                return app.MainWindow;
            }

            return windows.LastOrDefault() ?? app.MainWindow;
        }
    }
}
