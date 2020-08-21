using System.ComponentModel;

namespace Penguin.WinForms.Components.Dependencies
{
    public class CancelListBoxExItemDragEventArgs : CancelEventArgs
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelListBoxItemDragEventArgs"/> class.
        /// </summary>
        /// <param name="dragIndex">The index of the item the event data relates to.</param>
        public CancelListBoxExItemDragEventArgs(int dragIndex) => this.DragIndex = dragIndex;

        #endregion Public Constructors

        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CancelListBoxItemDragEventArgs"/> class.
        /// </summary>
        protected CancelListBoxExItemDragEventArgs()
        { }

        #endregion Protected Constructors

        #region Public Properties

        /// <summary>
        /// Gets the index of the item that is the source of the drag operation.
        /// </summary>
        /// <value>The index of the item that initiated the drag operation.</value>
        public int DragIndex { get; protected set; }

        #endregion Public Properties
    }
}