using System;
using System.Windows.Forms;

namespace Penguin.WinForms.Dialogs
{
    public class ExceptionDialog : Form
    {
        private const string DEFAULT_TITLE = "An Exception Has Occurred";

        public ExceptionDialog(Exception ex, string Title = DEFAULT_TITLE) : this($"{(ex ?? throw new ArgumentNullException(nameof(ex))).Message}{System.Environment.NewLine}{System.Environment.NewLine}{ex.StackTrace}", Title)
        {
        }

        public ExceptionDialog(string Message, string Title = DEFAULT_TITLE)
        {
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Text = Title;
            MinimizeBox = false;

            TextBox errorMessage = new()
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.Color.Black,
                Multiline = true,
                Text = Message
            };

            Controls.Add(errorMessage);
        }

        public static ExceptionDialog Show(Exception ex, string Title = DEFAULT_TITLE)
        {
            ExceptionDialog dialog = new(ex, Title);
            dialog.Show();
            return dialog;
        }

        public static ExceptionDialog Show(string Message, string Title = DEFAULT_TITLE)
        {
            ExceptionDialog dialog = new(Message, Title);
            dialog.Show();
            return dialog;
        }
    }
}