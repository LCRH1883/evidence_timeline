using System;
using System.Windows;
using System.Windows.Interop;
using WinForms = System.Windows.Forms;

namespace evidence_timeline.Utilities
{
    /// <summary>
    /// Wraps a WPF window as an <see cref="WinForms.IWin32Window"/> so it can be passed as
    /// the owner of WinForms dialogs (like <see cref="WinForms.OpenFileDialog"/> or
    /// <see cref="WinForms.FolderBrowserDialog"/>). Passing an owner ensures that dialogs
    /// are positioned consistently relative to their parent window and appear on the
    /// same monitor instead of randomly across multiple screens.
    /// </summary>
    public sealed class Wpf32Window : WinForms.IWin32Window
    {
        private readonly IntPtr _handle;

        /// <summary>
        /// Creates a new wrapper around the specified WPF window.
        /// </summary>
        /// <param name="window">The window whose HWND should be used as the owner.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="window"/> is null.</exception>
        public Wpf32Window(Window window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));
            // WindowInteropHelper converts a WPF Window into a Win32 HWND.
            _handle = new WindowInteropHelper(window).Handle;
        }

        /// <inheritdoc />
        public IntPtr Handle => _handle;
    }
}
