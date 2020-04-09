using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors
{
    internal class EditorCache : IDisposable
    {
        private static ConcurrentDictionary<string, CachedEditor> editorCache = new ConcurrentDictionary<string, CachedEditor>();

        internal EditorCache(string Id, Panel container, bool multiThread = true)
        {
            if (!editorCache.TryGetValue(Id, out CachedEditor _cachedEditor))
            {
                cachedEditor = _cachedEditor;

                Container = new Panel();
                ComponentFactory = new ComponentFactory(Container, multiThread);

                container.HorizontalScroll.Maximum = 0;
                container.AutoScroll = false;
                container.VerticalScroll.Visible = false;
                container.AutoScroll = true;
                Container.Width = container.Width;

                container.Resize += (sender, e) =>
                {
                    Container.Width = container.Width;
                    Container.Top = 0;
                    Container.Left = 0;
                };



                cachedEditor = new CachedEditor()
                {
                    ComponentFactory = ComponentFactory,
                    Container = Container
                };

                editorCache.TryAdd(Id, cachedEditor);
            }
            else
            {
                cachedEditor = _cachedEditor;

                if (!cachedEditor.IsDisposed)
                {
                    throw new AccessViolationException($"Editor with Id {Id} has not been properly disposed");
                }
                else
                {
                    cachedEditor.IsDisposed = false;
                }

                Container = cachedEditor.Container;
                ComponentFactory = cachedEditor.ComponentFactory;
            }

            container.Controls.Add(Container);
        }

        internal Panel Container { get; set; }
        internal ComponentFactory ComponentFactory { get; set; }

        private CachedEditor cachedEditor { get; set; }

        private class CachedEditor
        {
            internal Panel Container { get; set; }
            internal ComponentFactory ComponentFactory { get; set; }
            internal bool IsDisposed { get; set; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {

                this.cachedEditor.IsDisposed = true;

                ComponentFactory.ParentDisposed();


                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~EditorCache()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }


}
