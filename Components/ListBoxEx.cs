using Penguin.WinForms.Components.Dependencies;
using Penguin.WinForms.Components.Enumerations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

// Dragging items in a ListBox control with visual insertion guides
// http://www.cyotek.com/blog/dragging-items-in-a-listbox-control-with-visual-insertion-guides

namespace Penguin.WinForms.Components
{
    public class ListBoxEx : System.Windows.Forms.ListBox
    {
        #region Constants

        private const int INVALID_INDEX = -1;
        private const int WM_PAINT = 0xF;
        private const int WM_VSCROLL = 0x115;

        #endregion Constants

        #region Instance Fields

        private bool _allowItemDrag;

        private Color _insertionLineColor;

        #endregion Instance Fields

        #region Public Constructors

        public ListBoxEx()
        {
            DoubleBuffered = true;
            InsertionLineColor = Color.Red;
            InsertionIndex = INVALID_INDEX;
        }

        #endregion Public Constructors

        #region Events

        /// <summary>
        /// Occurs when the AllowItemDrag property value changes.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler AllowItemDragChanged;

        /// <summary>
        /// Occurs when the InsertionLineColor property value changes.
        /// </summary>
        [Category("Property Changed")]
        public event EventHandler InsertionLineColorChanged;

        /// <summary>
        /// Occurs when the user begins dragging an item.
        /// </summary>
        [Category("Drag Drop")]
        public event EventHandler<ListBoxExItemDragEventArgs> ItemDrag;

        /// <summary>
        /// Occurs when a drag-and-drop operation for an item is completed.
        /// </summary>
        [Category("Drag Drop")]
        public event EventHandler<CancelListBoxExItemDragEventArgs> ItemDragging;

        public event ScrollEventHandler Scroll;

        #endregion Events

        #region Overridden Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.DragDrop"/> event.
        /// </summary>
        /// <param name="drgevent">A <see cref="T:System.Windows.Forms.DragEventArgs"/> that contains the event data. </param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            if (drgevent is null)
            {
                throw new ArgumentNullException(nameof(drgevent));
            }

            if (IsDragging)
            {
                try
                {
                    if (InsertionIndex != INVALID_INDEX)
                    {
                        int dragIndex;
                        int dropIndex;

                        dragIndex = (int)drgevent.Data.GetData(typeof(int));
                        dropIndex = InsertionIndex;

                        if (dragIndex < dropIndex)
                        {
                            dropIndex--;
                        }
                        if (InsertionMode == InsertionMode.After && dragIndex < Items.Count - 1)
                        {
                            dropIndex++;
                        }

                        if (dropIndex != dragIndex)
                        {
                            ListBoxExItemDragEventArgs args;
                            Point clientPoint;

                            clientPoint = PointToClient(new Point(drgevent.X, drgevent.Y));
                            args = new ListBoxExItemDragEventArgs(dragIndex, dropIndex, InsertionMode, clientPoint.X, clientPoint.Y);

                            OnItemDrag(args);

                            if (!args.Cancel)
                            {
                                object dragItem;

                                dragItem = Items[dragIndex];

                                if (Items.Contains(dragItem))
                                {
                                    try
                                    {
                                        Items.Remove(dragItem);
                                    }
                                    catch (IndexOutOfRangeException)
                                    {
                                        //It was there, and now its not?
                                    }
                                }

                                Items.Insert(dropIndex, dragItem);
                                SelectedItem = dragItem;
                            }
                        }

                        DragIndex = INVALID_INDEX;
                    }
                }
                finally
                {
                    Invalidate(InsertionIndex);
                    InsertionIndex = INVALID_INDEX;
                    InsertionMode = InsertionMode.None;
                    IsDragging = false;
                }
            }

            base.OnDragDrop(drgevent);
        }

        protected override void OnDragLeave(EventArgs e)
        {
            Invalidate(InsertionIndex);
            InsertionIndex = INVALID_INDEX;
            InsertionMode = InsertionMode.None;

            base.OnDragLeave(e);
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            if (drgevent is null)
            {
                throw new ArgumentNullException(nameof(drgevent));
            }

            if (IsDragging)
            {
                int insertionIndex;
                InsertionMode insertionMode;
                Point clientPoint;

                clientPoint = PointToClient(new Point(drgevent.X, drgevent.Y));
                insertionIndex = IndexFromPoint(clientPoint);

                if (insertionIndex != INVALID_INDEX)
                {
                    Rectangle bounds;

                    bounds = GetItemRectangle(insertionIndex);
                    insertionMode = clientPoint.Y < bounds.Top + (bounds.Height / 2) ? InsertionMode.Before : InsertionMode.After;

                    drgevent.Effect = DragDropEffects.Move;
                }
                else
                {
                    insertionIndex = INVALID_INDEX;
                    insertionMode = InsertionMode.None;

                    drgevent.Effect = DragDropEffects.None;
                }

                if (insertionIndex != InsertionIndex || insertionMode != InsertionMode)
                {
                    Invalidate(InsertionIndex); // clear the previous item
                    InsertionMode = insertionMode;
                    InsertionIndex = insertionIndex;
                    Invalidate(InsertionIndex); // draw the new item
                }
            }

            base.OnDragOver(drgevent);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data. </param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                DragOrigin = e.Location;
                DragIndex = IndexFromPoint(e.Location);
            }
            else
            {
                DragOrigin = Point.Empty;
                DragIndex = INVALID_INDEX;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"/> that contains the event data. </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (AllowItemDrag && !IsDragging && e.Button == MouseButtons.Left && IsOutsideDragZone(e.Location) && DragIndex != INVALID_INDEX)
            {
                CancelListBoxExItemDragEventArgs args;

                args = new CancelListBoxExItemDragEventArgs(DragIndex);

                OnItemDragging(args);

                if (!args.Cancel)
                {
                    IsDragging = true;
                    _ = DoDragDrop(DragIndex, DragDropEffects.Move);
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            DragIndex = INVALID_INDEX;
        }

        protected virtual void OnScroll(ScrollEventArgs e)
        {
            Scroll?.Invoke(this, e);
        }

        /// <summary>
        /// Overrides <see cref="M:System.Windows.Forms.Control.WndProc(System.Windows.Forms.Message@)" />.
        /// </summary>
        /// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
        [DebuggerStepThrough]
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch (m.Msg)
            {
                case WM_PAINT:
                    OnWmPaint(ref m);
                    break;

                case WM_VSCROLL:
                    OnScroll(new ScrollEventArgs((ScrollEventType)(m.WParam.ToInt32() & 0xffff), 0));
                    break;
            }
        }

        #endregion Overridden Methods

        #region Public Properties

        [Category("Behavior")]
        [DefaultValue(false)]
        public virtual bool AllowItemDrag
        {
            get => _allowItemDrag;
            set
            {
                if (AllowItemDrag != value)
                {
                    _allowItemDrag = value;

                    OnAllowItemDragChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the insertion line drawn when dragging items within the control.
        /// </summary>
        /// <value>The color of the insertion line.</value>
        [Category("Appearance")]
        [DefaultValue(typeof(Color), "Red")]
        public virtual Color InsertionLineColor
        {
            get => _insertionLineColor;
            set
            {
                if (InsertionLineColor != value)
                {
                    _insertionLineColor = value;

                    OnInsertionLineColorChanged(EventArgs.Empty);
                }
            }
        }

        #endregion Public Properties

        #region Protected Properties

        protected int DragIndex { get; set; } = INVALID_INDEX;

        protected Point DragOrigin { get; set; }

        protected int InsertionIndex { get; set; }

        protected InsertionMode InsertionMode { get; set; }

        protected bool IsDragging { get; set; }

        #endregion Protected Properties

        #region Protected Members

        /// <summary>
        /// Invalidates the specified item, including the adjacent item depending on the current <see cref="InsertionMode"/>.
        /// </summary>
        /// <param name="index">The index of the item to invalidate.</param>
        protected void Invalidate(int index)
        {
            if (index != INVALID_INDEX)
            {
                Rectangle bounds;

                bounds = GetItemRectangle(index);
                if (InsertionMode == InsertionMode.Before && index > 0)
                {
                    bounds = Rectangle.Union(bounds, GetItemRectangle(index - 1));
                }
                else if (InsertionMode == InsertionMode.After && index < Items.Count - 1)
                {
                    bounds = Rectangle.Union(bounds, GetItemRectangle(index + 1));
                }

                Invalidate(bounds);
            }
        }

        /// <summary>
        /// Raises the <see cref="AllowItemDragChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnAllowItemDragChanged(EventArgs e)
        {
            EventHandler handler;

            handler = AllowItemDragChanged;

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="InsertionLineColorChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnInsertionLineColorChanged(EventArgs e)
        {
            EventHandler handler;

            handler = InsertionLineColorChanged;

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ItemDrag" /> event.
        /// </summary>
        /// <param name="e">The <see cref="ListBoxExItemDragEventArgs" /> instance containing the event data.</param>
        protected virtual void OnItemDrag(ListBoxExItemDragEventArgs e)
        {
            EventHandler<ListBoxExItemDragEventArgs> handler;

            handler = ItemDrag;

            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ItemDragging" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CancelListBoxItemDragEventArgs" /> instance containing the event data.</param>
        protected virtual void OnItemDragging(CancelListBoxExItemDragEventArgs e)
        {
            EventHandler<CancelListBoxExItemDragEventArgs> handler;

            handler = ItemDragging;

            handler?.Invoke(this, e);
        }

        protected virtual void OnWmPaint(ref Message m)
        {
            DrawInsertionLine();
        }

        #endregion Protected Members

        #region Private Members

        private void DrawInsertionLine()
        {
            if (InsertionIndex != INVALID_INDEX)
            {
                int index;

                index = InsertionIndex;

                if (index >= 0 && index < Items.Count)
                {
                    Rectangle bounds;
                    int x;
                    int y;
                    int width;

                    bounds = GetItemRectangle(InsertionIndex);
                    x = 0; // always fit the line to the client area, regardless of how the user is scrolling
                    y = InsertionMode == InsertionMode.Before ? bounds.Top : bounds.Bottom;
                    width = Math.Min(bounds.Width - bounds.Left, ClientSize.Width); // again, make sure the full width fits in the client area

                    DrawInsertionLine(x, y, width);
                }
            }
        }

        private void DrawInsertionLine(int x1, int y, int width)
        {
            using Graphics g = CreateGraphics();
            Point[] leftArrowHead;
            Point[] rightArrowHead;
            int arrowHeadSize;
            int x2;

            x2 = x1 + width;
            arrowHeadSize = 7;
            leftArrowHead = new[]
                            {
                          new Point(x1, y - (arrowHeadSize / 2)), new Point(x1 + arrowHeadSize, y), new Point(x1, y + (arrowHeadSize / 2))
                        };
            rightArrowHead = new[]
                             {
                           new Point(x2, y - (arrowHeadSize / 2)), new Point(x2 - arrowHeadSize, y), new Point(x2, y + (arrowHeadSize / 2))
                         };

            using (Pen pen = new(InsertionLineColor))
            {
                g.DrawLine(pen, x1, y, x2 - 1, y);
            }

            using Brush brush = new SolidBrush(InsertionLineColor);
            g.FillPolygon(brush, leftArrowHead);
            g.FillPolygon(brush, rightArrowHead);
        }

        private bool IsOutsideDragZone(Point location)
        {
            Rectangle dragZone;
            int dragWidth;
            int dragHeight;

            dragWidth = SystemInformation.DragSize.Width;
            dragHeight = SystemInformation.DragSize.Height;
            dragZone = new Rectangle(DragOrigin.X - (dragWidth / 2), DragOrigin.Y - (dragHeight / 2), dragWidth, dragHeight);

            return !dragZone.Contains(location);
        }

        #endregion Private Members
    }
}