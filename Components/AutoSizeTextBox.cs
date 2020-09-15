using System;
using System.Drawing;
using System.Windows.Forms;

namespace Penguin.WinForms.Components
{
    public class AutoSizeTextBox : TextBox
    {
        public AutoSizeTextBox()
        {
            // Register the TextChanged event handler.
            TextChanged += this.TxtContents_TextChanged;
            MultilineChanged += this.TxtContents_TextChanged;

            // Make the TextBox fit its initial text.
            this.CallAutoSize();
        }

        // Make the TextBox fit its contents.
        public void CallAutoSize()
        {
            if (this.Text.Contains("\n"))
            {
                this.Multiline = true;
            }

            if (this.Multiline)
            {
                this.ScrollBars = ScrollBars.None;
                //const int x_margin = 0;
                //const int y_margin = 2;
                //Size size = TextRenderer.MeasureText(this.Text, this.Font);
                int height = TextRenderer.MeasureText("X", this.Font).Height;

                //this.ClientSize = new Size(size.Width + x_margin, size.Height + y_margin);

                Point end = this.GetPositionFromCharIndex(this.TextLength - 1);

                this.ClientSize = new Size(this.ClientSize.Width, end.Y + height);
            }
        }


        // Make the TextBox fit its new contents.
        private void TxtContents_TextChanged(object sender, EventArgs e) => this.CallAutoSize();
    }
}
