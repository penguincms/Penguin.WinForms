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
            TextChanged += TxtContents_TextChanged;
            MultilineChanged += TxtContents_TextChanged;

            // Make the TextBox fit its initial text.
            CallAutoSize();
        }

        // Make the TextBox fit its contents.
        public void CallAutoSize()
        {
            if (Text.Contains('\n'))
            {
                Multiline = true;
            }

            if (Multiline)
            {
                ScrollBars = ScrollBars.None;
                //const int x_margin = 0;
                //const int y_margin = 2;
                //Size size = TextRenderer.MeasureText(this.Text, this.Font);
                int height = TextRenderer.MeasureText("X", Font).Height;

                //this.ClientSize = new Size(size.Width + x_margin, size.Height + y_margin);

                Point end = GetPositionFromCharIndex(TextLength - 1);

                ClientSize = new Size(ClientSize.Width, end.Y + height);
            }
        }

        // Make the TextBox fit its new contents.
        private void TxtContents_TextChanged(object sender, EventArgs e)
        {
            CallAutoSize();
        }
    }
}