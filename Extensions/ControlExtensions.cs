using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Penguin.WinForms.Extensions
{
    public static class ControlExtensions
    {

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);

        private const int WM_SETREDRAW = 11;

        public static void SuspendDrawing(this Control parent)
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
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

    }
}
