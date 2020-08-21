using Penguin.WinForms.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors.Component
{
    public class PromptBool : Form
    {
        protected Button CancelSelectionButton;
        protected TextBox TextBox;
        protected Button SelectButton;
        public bool Result { get; protected set; }

        public static DialogResult ShowDialog(string text, string trueText = "Yes", string falseText = "No", string Title = "Please Select An Option", Action<bool> onSelect = null)
        {
            PromptBool textdialog = new PromptBool(text, trueText, falseText, Title, onSelect);

            return textdialog.ShowDialog();
        }

        public PromptBool(string text, string trueText = "Yes", string falseText = "No", string Title = "Please Select An Option", Action<bool> onSelect = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("message", nameof(text));
            }

            if (string.IsNullOrEmpty(trueText))
            {
                throw new ArgumentException("message", nameof(trueText));
            }

            if (string.IsNullOrEmpty(falseText))
            {
                throw new ArgumentException("message", nameof(falseText));
            }

            if (string.IsNullOrEmpty(Title))
            {
                throw new ArgumentException("message", nameof(Title));
            }


            this.Text = Title;

            this.TextBox = new TextBox()
            {
                Text = text,
                ReadOnly = true,
                Multiline = true,
                BackColor = System.Drawing.SystemColors.Control
            };


            this.Controls.Add(this.TextBox);

            this.SelectButton = new Button()
            {
                Text = trueText
            };

            this.CancelSelectionButton = new Button()
            {
                Text = falseText
            };

            this.SelectButton.Click += (sender, e) =>
            {

                this.DialogResult = DialogResult.OK;
                this.Result = true;
                onSelect?.Invoke(this.Result);
                this.Close();
            };

            this.CancelSelectionButton.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Result = false;
                onSelect?.Invoke(this.Result);
                this.Close();
            };

            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.CancelSelectionButton);

            this.ResizeObjects();
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

            this.TextBox.FitText();

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