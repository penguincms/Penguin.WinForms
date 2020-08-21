using System;
using System.Windows.Forms;

namespace Penguin.WinForms.Extensions
{
    public static class ContextMenuStripExtensions
    {
        public static ToolStripMenuItem AddItem(this ContextMenuStrip menu, string DisplayText, Action toExecute) => menu.AddItem(DisplayText, (e) => toExecute.Invoke());
        public static ToolStripMenuItem AddItem(this ContextMenuStrip menu, string DisplayText, Action<EventArgs> toExecute) => menu.AddItem(DisplayText, (sender, e) => toExecute.Invoke(e));
        public static ToolStripMenuItem AddItem(this ContextMenuStrip menu, string DisplayText, Action<object, EventArgs> toExecute)
        {
            if (menu is null)
            {
                throw new ArgumentNullException(nameof(menu));
            }

            if (DisplayText is null)
            {
                throw new ArgumentNullException(nameof(DisplayText));
            }

            if (toExecute is null)
            {
                throw new ArgumentNullException(nameof(toExecute));
            }

            ToolStripMenuItem newItem = new ToolStripMenuItem { Text = DisplayText };
            newItem.Click += (o, e) => toExecute.Invoke(o, e);
            menu.Items.Add(newItem);
            return newItem;
        }
    }
}
