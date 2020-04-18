using Penguin.WinForms.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
            PromptBool textdialog = new PromptBool(text, trueText , falseText, Title, onSelect);

            return (textdialog as Form).ShowDialog();
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

            TextBox = new TextBox()
            {
                Text = text,
                ReadOnly = true,
                Multiline = true,
                BackColor = System.Drawing.SystemColors.Control
            };


            this.Controls.Add(TextBox);

            SelectButton = new Button()
            {
                Text = trueText
            };

            CancelSelectionButton = new Button()
            {
                Text = falseText
            };

            SelectButton.Click += (sender, e) =>
            {

                this.DialogResult = DialogResult.OK;
                Result = true;
                onSelect?.Invoke(Result);
                this.Close();
            };

            CancelSelectionButton.Click += (sender, e) =>
            {
                this.DialogResult = DialogResult.OK;
                Result = false;
                onSelect?.Invoke(Result);
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

            TextBox.FitText();

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