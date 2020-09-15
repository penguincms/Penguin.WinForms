using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Penguin.WinForms.Extensions
{
    public static class ControlExtensions
    {
        private const int WM_SETREDRAW = 11;

        public static T AddControl<T>(this Control parent, T toAdd) where T : Control
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            if (toAdd is null)
            {
                throw new ArgumentNullException(nameof(toAdd));
            }

            parent.Controls.Add(toAdd);

            return toAdd;
        }

        public static void ResumeDrawing(this Control parent)
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
            parent.Refresh();
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        public static void SuspendDrawing(this Control parent)
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
        }
    }
}