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
            Text = Title;

            TextBox = new TextBox()
            {
                Text = defaultText
            };

            Controls.Add(TextBox);

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
                DialogResult = DialogResult.OK;
                Result = TextBox.Text;
                onSelect?.Invoke(Result);
                Close();
            };

            CancelSelectionButton.Click += (sender, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.Add(SelectButton);
            Controls.Add(CancelSelectionButton);

            ResizeObjects();
        }

        public static DialogResult ShowDialog(string Title = "Please Select An Option", string defaultText = "", Action<string> onSelect = null)
        {
            InputText textdialog = new(Title, defaultText, onSelect);

            return (textdialog as Form).ShowDialog();
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

            TextBox.Height = ClientSize.Height / 2;
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