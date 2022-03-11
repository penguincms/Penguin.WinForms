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

            displayText ??= new Func<T, string>((t) => t.ToString());

            this.Options = options.ToList();

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

            this.SelectBox = new ComboBox()
            {
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            this.SelectBox.Items.Add("...");
            this.SelectBox.SelectedIndex = 0;

            foreach (T o in options)
            {
                this.SelectBox.Items.Add(displayText.Invoke(o));
            }

            this.SelectBox.SelectedIndexChanged += (sender, e) => this.SelectButton.Enabled = this.SelectBox.SelectedIndex != 0;

            this.Controls.Add(this.SelectBox);

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
                if (this.SelectBox.SelectedIndex != 0)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Result = this.Options.ElementAt(this.SelectBox.SelectedIndex - 1);
                    onSelect?.Invoke(this.Result);
                    this.Close();
                }
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

        protected override void OnResize(EventArgs e)
        {
            this.ResizeObjects();
            base.OnResize(e);
        }

        protected virtual void ResizeObjects()
        {
            this.SelectBox.Width = this.ClientSize.Width;
            this.CancelSelectionButton.Width = this.SelectButton.Width = this.ClientSize.Width / 2;

            this.SelectBox.Height = this.ClientSize.Height / 2;
            this.CancelSelectionButton.Height = this.SelectButton.Height = this.SelectBox.Height;

            this.SelectBox.Top = 0;
            this.CancelSelectionButton.Top = this.SelectButton.Top = this.SelectBox.Height;

            this.SelectButton.Left = this.ClientSize.Width / 2;
            this.CancelSelectionButton.Left = 0;

            this.ClientSize = new Size()
            {
                Height = this.SelectBox.Height + this.CancelSelectionButton.Height,
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