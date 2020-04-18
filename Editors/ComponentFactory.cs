using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors
{
    internal class ComponentFactory : IDisposable
    {
        private const bool REUSE_COMPONENTS = false;

        private object bwLock = new object();
        private static ConcurrentDictionary<Type, ConcurrentQueue<Control>> CachedComponents = new ConcurrentDictionary<Type, ConcurrentQueue<Control>>();
        private Panel Container;
        private ConcurrentDictionary<Type, Control> Defaults = new ConcurrentDictionary<Type, Control>();

        private Queue<Control> ReturnQueue = new Queue<Control>();

        private BackgroundWorker returnThread;

        public void Clear()
        {
            if (!REUSE_COMPONENTS)
            {
                Container.Controls.Clear();
            }
            else
            {
                foreach (Control c in this.ActiveControls)
                {
                    this.Return(c);
                }
            }
        }

        internal IEnumerable<Control> ActiveControls
        {
            get
            {
                foreach (Control c in this.Container.Controls)
                {
                    if (c.Visible)
                    {
                        yield return c;
                    }
                }
            }
        }
        private bool MultiThread;
        internal ComponentFactory(Panel container, bool multiThread = true)
        {

            this.MultiThread = multiThread;
            this.Container = container;

            if (this.MultiThread)
            {
                this.returnThread = new BackgroundWorker();
                this.returnThread.DoWork += this.ReturnThread_DoWork;

            }
        }

        internal T Request<T>() where T : Control
        {
            ConcurrentQueue<Control> controls = this.GetQueue(typeof(T));

            if (!controls.TryDequeue(out Control result) || !REUSE_COMPONENTS)
            {
                result = Activator.CreateInstance<T>();
                this.Container.Controls.Add(result);
            }
            else
            {
                result.Show();
            }

            return result as T;
        }

        internal void Return<T>(T control) where T : Control
        {
            if(!REUSE_COMPONENTS)
            {
                this.Container.Controls.Remove(control);
                return;
            }

            control.Hide();

            if (this.MultiThread)
            {
                lock (this.bwLock)
                {
                    this.ReturnQueue.Enqueue(control);

                    if (!this.returnThread.IsBusy)
                    {
                        this.returnThread.RunWorkerAsync();
                    }
                }
            }
            else
            {
                this.ReturnItem(control);
            }
        }

        private ConcurrentQueue<Control> GetQueue(Type controlType)
        {
            if (!CachedComponents.TryGetValue(controlType, out ConcurrentQueue<Control> controls))
            {
                controls = new ConcurrentQueue<Control>();
                CachedComponents.TryAdd(controlType, controls);
            }

            return controls;
        }

        private void ReturnItem(Control toProcess)
        {
            Type controlType = toProcess.GetType();

            if (!this.Defaults.TryGetValue(controlType, out Control @default))
            {
                @default = Activator.CreateInstance(controlType) as Control;

                this.Defaults.TryAdd(controlType, @default);
            }


            List<FieldInfo> fields = new List<FieldInfo>();
            List<EventInfo> events = new List<EventInfo>();

            Type t = controlType;

            do
            {
                fields.AddRange(t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
                events.AddRange(t.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
                t = t.BaseType;
            } while (t != null && t != typeof(object));


            fields = fields.OrderBy(f => f.Name).ToList();
            events = events.OrderBy(e => e.Name).ToList();

            FieldInfo eventsField = fields.Single(f => f.Name == "events");

            EventHandlerList eventHandlerList = (EventHandlerList)eventsField.GetValue(toProcess);

            foreach (FieldInfo fi in fields)
            {

                if(!fi.Name.StartsWith("Event"))
                {
                    continue;
                }

                object key = fi.GetValue(toProcess);

                object val = eventHandlerList[key];

                if(val is null)
                {
                    continue;
                }

                eventHandlerList.RemoveHandler(key, eventHandlerList[key]);
            }

            //foreach (PropertyInfo pi in controlType.GetProperties())
            //{
            //    if (pi.GetGetMethod() is null || pi.GetSetMethod() is null)
            //    {
            //        continue;
            //    }

            //    object oldValue = pi.GetValue(toProcess);
            //    object newValue = pi.GetValue(@default);

            //    if(object.Equals(oldValue, newValue))
            //    {
            //        continue;
            //    }

            //    Action resetProperty = delegate { pi.SetValue(toProcess, newValue); };

            //    if (this.MultiThread)
            //    {
            //        toProcess.Invoke(resetProperty);
            //    }
            //    else
            //    {
            //        resetProperty.Invoke();
            //    }
            //}

            this.GetQueue(controlType).Enqueue(toProcess);
        }

        private void ReturnThread_DoWork(object sender, DoWorkEventArgs e)
        {
            Control toProcess = null;

            bool GetNext()
            {
                lock (this.bwLock)
                {
                    if (!this.ReturnQueue.Any())
                    {
                        return false;
                    }

                    toProcess = this.ReturnQueue.Dequeue();
                    return true;
                }
            }

            do
            {
                if (!GetNext())
                {
                    return;
                }

                this.ReturnItem(toProcess);
            } while (true);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        internal virtual void ParentDisposed()
        {
            if (REUSE_COMPONENTS)
            {
                foreach (Control c in this.ActiveControls)
                {
                    this.Return(c);
                }
            } else
            {
                this.Container.Controls.Clear();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.ParentDisposed();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ComponentFactory()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}