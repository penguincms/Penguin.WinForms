using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Penguin.WinForms.Extensions
{
    public static class TextBoxExtensions
    {
        public static void FitText(this TextBox textBox)
        {
            const int y_margin = 2;

            Size size = TextRenderer.MeasureText(textBox.Text, textBox.Font);

            textBox.ClientSize = new Size(textBox.ClientSize.Width, size.Height + y_margin);
            
        }
    }
}
