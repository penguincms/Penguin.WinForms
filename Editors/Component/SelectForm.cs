using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors.Component
{
    public class SelectForm<T> : Form
    {
        protected Button CancelSelectionButton;
        protected ComboBox SelectBox;
        protected Button SelectButton;
        public IReadOnlyList<T> Options { get; set; }
        public T Result { get; protected set; }

        public SelectForm(IList<T> options, string Title = "Please Select An Option", Action<T> onSelect = null, Func<T, string> displayText = null)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            displayText = displayText ?? new Func<T, string>((t) =>
            {
                return t.ToString();
            });

            Options = options.ToList();

            if (options.Count == 0)
            {
                throw new ArgumentException("Select Box must have one or more options");
            }

            //if(options.Count == 1)
            //{
            //    Result = options.Single();
            //    DialogResult = DialogResult.OK;
            //    this.Close();
            //    return;
            //}

            this.Text = Title;

            SelectBox = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            SelectBox.Items.Add("...");
            SelectBox.SelectedIndex = 0;

            foreach (T o in options)
            {
                SelectBox.Items.Add(displayText.Invoke(o));
            }

            SelectBox.SelectedIndexChanged += (sender, e) =>
            {
                SelectButton.Enabled = SelectBox.SelectedIndex != 0;
            };

            this.Controls.Add(SelectBox);

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
                if (SelectBox.SelectedIndex != 0)
                {
                    this.DialogResult = DialogResult.OK;
                    Result = Options.ElementAt(SelectBox.SelectedIndex - 1);
                    onSelect?.Invoke(Result);
                    this.Close();
                }
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
            SelectBox.Width = this.ClientSize.Width;
            CancelSelectionButton.Width = SelectButton.Width = this.ClientSize.Width / 2;

            SelectBox.Height = this.ClientSize.Height / 2;
            CancelSelectionButton.Height = SelectButton.Height = SelectBox.Height;

            SelectBox.Top = 0;
            CancelSelectionButton.Top = SelectButton.Top = SelectBox.Height;

            SelectButton.Left = this.ClientSize.Width / 2;
            CancelSelectionButton.Left = 0;

            this.ClientSize = new Size()
            {
                Height = SelectBox.Height + CancelSelectionButton.Height,
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