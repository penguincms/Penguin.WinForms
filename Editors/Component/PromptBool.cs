using Penguin.WinForms.Extensions;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors.Component
{
    public class PromptBool : Form
    {
        protected Button CancelSelectionButton;
        protected Button SelectButton;
        protected TextBox TextBox;
        public bool Result { get; protected set; }

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

            Text = Title;

            TextBox = new TextBox()
            {
                Text = text,
                ReadOnly = true,
                Multiline = true,
                BackColor = System.Drawing.SystemColors.Control
            };

            Controls.Add(TextBox);

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
                DialogResult = DialogResult.OK;
                Result = true;
                onSelect?.Invoke(Result);
                Close();
            };

            CancelSelectionButton.Click += (sender, e) =>
            {
                DialogResult = DialogResult.OK;
                Result = false;
                onSelect?.Invoke(Result);
                Close();
            };

            Controls.Add(SelectButton);
            Controls.Add(CancelSelectionButton);

            ResizeObjects();
        }

        public static DialogResult ShowDialog(string text, string trueText = "Yes", string falseText = "No", string Title = "Please Select An Option", Action<bool> onSelect = null)
        {
            PromptBool textdialog = new(text, trueText, falseText, Title, onSelect);

            return textdialog.ShowDialog();
        }

        protected override void OnResize(EventArgs e)
        {
            ResizeObjects();
            base.OnResize(e);
        }

        protected virtual void ResizeObjects()
        {
            TextBox.Width = ClientSize.Width;
            CancelSelectionButton.Width = SelectButton.Width = ClientSize.Width / 2;

            TextBox.FitText();

            CancelSelectionButton.Height = SelectButton.Height = TextBox.Height;

            TextBox.Top = 0;
            CancelSelectionButton.Top = SelectButton.Top = TextBox.Height;

            SelectButton.Left = ClientSize.Width / 2;
            CancelSelectionButton.Left = 0;

            ClientSize = new Size()
            {
                Height = TextBox.Height + CancelSelectionButton.Height,
                Width = ClientSize.Width
            };
        }

        public void InitializeComponent()
        {
            SuspendLayout();
            //
            // SelectForm
            //
            ClientSize = new System.Drawing.Size(320, 77);
            Name = "SelectForm";
            ResumeLayout(false);
        }
    }
}