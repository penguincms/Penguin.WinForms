using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors.Component
{
    public class InputText : Form
    {
        protected Button CancelSelectionButton;
        protected TextBox TextBox;
        protected Button SelectButton;
        public string Result { get; protected set; }

        public static DialogResult ShowDialog(string Title = "Please Select An Option", string defaultText = "", Action<string> onSelect = null)
        {
            InputText textdialog = new InputText(Title, defaultText, onSelect);

            return (textdialog as Form).ShowDialog();
        }

        public InputText(string Title = "Please Select An Option", string defaultText = "", Action<string> onSelect = null)
        {

            this.Text = Title;

            TextBox = new TextBox()
            {
                Text = defaultText
            };


            this.Controls.Add(TextBox);

            SelectButton = new Button()
            {
                Text = "OK"
            };

            CancelSelectionButton = new Button()
            {
                Text = "Cancel"
            };

            SelectButton.Click += (sender, e) =>
            {

                this.DialogResult = DialogResult.OK;
                Result = TextBox.Text;
                onSelect?.Invoke(Result);
                this.Close();
            };

            CancelSelectionButton.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };

            this.Controls.Add(SelectButton);
            this.Controls.Add(CancelSelectionButton);

            ResizeObjects();
        }

        protected override void OnResize(EventArgs e)
        {
            ResizeObjects();
            base.OnResize(e);
        }

        protected virtual void ResizeObjects()
        {
            TextBox.Width = this.ClientSize.Width;
            CancelSelectionButton.Width = SelectButton.Width = this.ClientSize.Width / 2;

            TextBox.Height = this.ClientSize.Height / 2;
            CancelSelectionButton.Height = SelectButton.Height = TextBox.Height;

            TextBox.Top = 0;
            CancelSelectionButton.Top = SelectButton.Top = TextBox.Height;

            SelectButton.Left = this.ClientSize.Width / 2;
            CancelSelectionButton.Left = 0;

            this.ClientSize = new Size()
            {
                Height = TextBox.Height + CancelSelectionButton.Height,
                Width = this.ClientSize.Width
            };
        }

        private void InitializeComponent()
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