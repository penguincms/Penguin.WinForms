using System.Drawing;
using System.Windows.Forms;

namespace Penguin.WinForms.Components // May need to set to something else
{
    /// <summary>
    /// A ListView with DragDrop reordering.
    /// http://support.microsoft.com/kb/822483/en-us
    /// </summary>
    public class ListViewEx : ListView
    {
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (drgevent is null)
            {
                throw new System.ArgumentNullException(nameof(drgevent));
            }

            base.OnDragDrop(drgevent);
            //Return if the items are not selected in the ListView control.
            if (SelectedItems.Count == 0)
            {
                return;
            }
            //Returns the location of the mouse pointer in the ListView control.
            Point cp = PointToClient(new Point(drgevent.X, drgevent.Y));
            //Obtain the item that is located at the specified location of the mouse pointer.
            ListViewItem dragToItem = GetItemAt(cp.X, cp.Y);
            if (dragToItem == null)
            {
                return;
            }
            //Obtain the index of the item at the mouse pointer.
            int dragIndex = dragToItem.Index;
            ListViewItem[] sel = new ListViewItem[SelectedItems.Count];
            for (int i = 0; i <= SelectedItems.Count - 1; i++)
            {
                sel[i] = SelectedItems[i];
            }
            for (int i = 0; i < sel.GetLength(0); i++)
            {
                //Obtain the ListViewItem to be dragged to the target location.
                ListViewItem dragItem = sel[i];
                int itemIndex = dragIndex;
                if (itemIndex == dragItem.Index)
                {
                    return;
                }
                if (dragItem.Index < itemIndex)
                {
                    itemIndex++;
                }
                else
                {
                    itemIndex = dragIndex + i;
                }
                //Insert the item at the mouse pointer.
                ListViewItem insertItem = (ListViewItem)dragItem.Clone();
                _ = Items.Insert(itemIndex, insertItem);
                //Removes the item from the initial location while
                //the item is moved to the new location.
                Items.Remove(dragItem);
            }
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            if (drgevent is null)
            {
                throw new System.ArgumentNullException(nameof(drgevent));
            }

            base.OnDragEnter(drgevent);
            int len = drgevent.Data.GetFormats().Length - 1;
            int i;
            for (i = 0; i <= len; i++)
            {
                if (drgevent.Data.GetFormats()[i].Equals("System.Windows.Forms.ListView+SelectedListViewItemCollection", System.StringComparison.Ordinal))
                {
                    //The data from the drag source is moved to the target.
                    drgevent.Effect = DragDropEffects.Move;
                }
            }
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            base.OnItemDrag(e);
            //Begins a drag-and-drop operation in the ListView control.
            _ = DoDragDrop(SelectedItems, DragDropEffects.Move);
        }
    }
}