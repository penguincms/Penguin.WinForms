using System;
using System.Windows.Forms;

namespace Penguin.WinForms.Dialogs
{
    public class ExceptionDialog : Form
    {
        private const string DEFAULT_TITLE = "An Exception Has Occurred";

        public ExceptionDialog(Exception ex, string Title = DEFAULT_TITLE) : this($"{ex.Message}{System.Environment.NewLine}{System.Environment.NewLine}{ex.StackTrace}", Title)
        {
        }

        public ExceptionDialog(string Message, string Title = DEFAULT_TITLE)
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = Title;
            this.MinimizeBox = false;

            TextBox errorMessage = new TextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = System.Drawing.SystemColors.Control,
                ForeColor = System.Drawing.Color.Black,
                Multiline = true,
                Text = Message
            };

            this.Controls.Add(errorMessage);
        }

        public static ExceptionDialog Show(Exception ex, string Title = DEFAULT_TITLE)
        {
            ExceptionDialog dialog = new ExceptionDialog(ex, Title);
            dialog.Show();
            return dialog;
        }

        public static ExceptionDialog Show(string Message, string Title = DEFAULT_TITLE)
        {
            ExceptionDialog dialog = new ExceptionDialog(Message, Title);
            dialog.Show();
            return dialog;
        }
    }
}