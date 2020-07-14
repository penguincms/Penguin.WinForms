using Penguin.WinForms.FileDialogs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Penguin.WinForms.Extensions
{
    public static class FileDialogExtensions
    {
        public static void ApplyPreset(this FileDialog dialog, FileDialogPreset preset, string fileName = null)
        {
            if (dialog is null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            if (preset is null)
            {
                throw new ArgumentNullException(nameof(preset));
            }

            dialog.DefaultExt = preset.Extension;
            dialog.Filter = preset.Filter;
            dialog.InitialDirectory = preset.InitialDirectory;
            dialog.FileName = fileName ?? preset.FileName;
        }

        public static bool ShowDialog(this FileDialog dialog, string FileName) => dialog.ShowDialog(FileName, null);

        public static bool ShowDialog(this FileDialog dialog, string FileName, Action<string> ifOk)
        {
            if (dialog is null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            dialog.FileName = FileName;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ifOk?.Invoke(dialog.FileName);
                return true;
            }

            return false;
        }

        public static void ShowDialog(this FileDialog dialog, Action<string> ifOk)
        {
            if (dialog is null)
            {
                throw new ArgumentNullException(nameof(dialog));
            }

            dialog.ShowDialog(dialog.FileName, ifOk);
        }
    }
}