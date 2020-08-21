using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors
{
    internal class EditorCache : IDisposable
    {
        private static readonly ConcurrentDictionary<string, CachedEditor> editorCache = new ConcurrentDictionary<string, CachedEditor>();
        private readonly Panel ParentContainer;
        public string Id;
        internal EditorCache(string id, Panel container, bool multiThread = true)
        {
            this.Id = id;

            if (!editorCache.TryGetValue(id, out CachedEditor _cachedEditor))
            {
                this.cachedEditor = _cachedEditor;

                this.Container = new Panel();
                this.ComponentFactory = new ComponentFactory(this.Container, multiThread);

                container.HorizontalScroll.Maximum = 0;
                container.VerticalScroll.Visible = false;
                container.AutoScroll = true;
                this.Container.Width = container.Width;

                container.Resize += (sender, e) =>
                {
                    this.Container.Width = container.Width;
                    this.Container.Top = 0;
                    this.Container.Left = 0;
                };



                this.cachedEditor = new CachedEditor()
                {
                    ComponentFactory = ComponentFactory,
                    Container = Container
                };

                editorCache.TryAdd(id, this.cachedEditor);
            }
            else
            {
                this.cachedEditor = _cachedEditor;

                if (!this.cachedEditor.IsDisposed)
                {
                    throw new AccessViolationException($"Editor with Id {id} has not been properly disposed");
                }
                else
                {
                    this.cachedEditor.IsDisposed = false;
                }

                this.Container = this.cachedEditor.Container;
                this.ComponentFactory = this.cachedEditor.ComponentFactory;
            }

            this.ParentContainer = container;
            container.Controls.Add(this.Container);
        }

        public Panel Container { get; private set; }
        private ComponentFactory ComponentFactory { get; set; }

        private CachedEditor cachedEditor { get; set; }
        public IEnumerable<Control> ActiveControls => this.ComponentFactory.ActiveControls;

        private class CachedEditor
        {
            internal Panel Container { get; set; }
            internal ComponentFactory ComponentFactory { get; set; }
            internal bool IsDisposed { get; set; }
        }

        internal T Request<T>() where T : Control
        {
            T c = this.ComponentFactory.Request<T>();

            return c;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {

                //editorCache.TryRemove(Id, out _);

                this.cachedEditor.IsDisposed = true;

                this.ComponentFactory.ParentDisposed();


                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~EditorCache()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        internal void Clear()
        {
            this.ParentContainer.VerticalScroll.Value = 0;
            this.Container.Top = 0;
            this.ComponentFactory.Clear();
        }
        #endregion
    }


}
