using System;
using System.Drawing;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors.Component
{
    public class InputText : Form
    {
        protected Button CancelSelectionButton;
        protected Button SelectButton;
        protected TextBox TextBox;
        public string Result { get; protected set; }

        public InputText(string Title = "Please Select An Option", string defaultText = "", Action<string> onSelect = null)
        {
            this.Text = Title;

            this.TextBox = new TextBox()
            {
                Text = defaultText
            };

            this.Controls.Add(this.TextBox);

            this.SelectButton = new Button()
            {
                Text = "OK"
            };

            this.CancelSelectionButton = new Button()
            {
                Text = "Cancel"
            };

            this.SelectButton.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Result = this.TextBox.Text;
                onSelect?.Invoke(this.Result);
                this.Close();
            };

            this.CancelSelectionButton.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.CancelSelectionButton);

            this.ResizeObjects();
        }

        public static DialogResult ShowDialog(string Title = "Please Select An Option", string defaultText = "", Action<string> onSelect = null)
        {
            InputText textdialog = new InputText(Title, defaultText, onSelect);

            return (textdialog as Form).ShowDialog();
        }

        protected override void OnResize(EventArgs e)
        {
            this.ResizeObjects();
            base.OnResize(e);
        }

        protected virtual void ResizeObjects()
        {
            this.TextBox.Width = this.ClientSize.Width;
            this.CancelSelectionButton.Width = this.SelectButton.Width = this.ClientSize.Width / 2;

            this.TextBox.Height = this.ClientSize.Height / 2;
            this.CancelSelectionButton.Height = this.SelectButton.Height = this.TextBox.Height;

            this.TextBox.Top = 0;
            this.CancelSelectionButton.Top = this.SelectButton.Top = this.TextBox.Height;

            this.SelectButton.Left = this.ClientSize.Width / 2;
            this.CancelSelectionButton.Left = 0;

            this.ClientSize = new Size()
            {
                Height = this.TextBox.Height + this.CancelSelectionButton.Height,
                Width = this.ClientSize.Width
            };
        }

        public void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // SelectForm
            //
            this.ClientSize = new System.Drawing.Size(320, 77);
            this.Name = "SelectForm";
            this.ResumeLayout(false);
        }
    }
}