using Newtonsoft.Json;
using Penguin.Json.Extensions;
using Penguin.Reflection.Extensions;
using Penguin.WinForms.Components;
using Penguin.WinForms.Editors.Component;
using Penguin.WinForms.Editors.ConstructorArguments;
using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Dependencies;
using Penguin.WinForms.Editors.Events;
using Penguin.WinForms.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors
{
    public class ObjectEditor<T> : IDisposable
    {
        private readonly Panel Container;
        private readonly Action<T> OnSave;
        private readonly List<Action> ResizeActions = new List<Action>();
        private readonly Dictionary<Type, string> ToolTipCache = new Dictionary<Type, string>();
        private int _currentTop;

        public int CurrentTop
        {
            get => this._currentTop;
            protected set
            {
                this.Container.Height = value;
                this._currentTop = value;
            }
        }

        public int ITEM_HEIGHT { get; set; } = 35;
        public int ITEM_SPACING { get; set; } = 3;
        public int LIST_PADDING { get; set; } = 25;
        public int PanelPadding => (int)(this.Container.Width * this.WIDTH_PADDING_PER);
        public T TemporaryObject { get; protected set; }
        public float WIDTH_PADDING_PER { get; set; } = .05f;

        public ObjectEditor(T toEdit, Panel container, Action<T> onSave)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            this.Container = container;

            this.Container.Resize += (o, s) =>
            {
                foreach (Action a in this.ResizeActions)
                {
                    a.Invoke();
                }
            };

            this.TemporaryObject = toEdit.JsonClone();

            this.OnSave = onSave;

            this.Load();
        }

        public int AddBoolItem(IBoolRowConstructorArguments arguments)
        {
            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            this.AddLabel(arguments);

            CheckBox value = this.Container.AddControl(new CheckBox
            {
                Height = this.ITEM_HEIGHT,
                Top = this.CurrentTop,
                Name = arguments.Name,
                Checked = arguments.Value
            });

            Action resize = new Action(() =>
            {
                value.Left = (this.Container.Width / 2);
                value.Width = (this.Container.Width / 2) - this.PanelPadding;
            });

            resize.Invoke();

            this.ResizeActions.Add(resize);

            value.CheckedChanged += (sender, e) =>
            {
                CheckBox sText = (sender as CheckBox);

                arguments.OnChange?.Invoke(new ValueChangedEventArgs()
                {
                    SourceControl = sText,
                    Value = sText.Checked.ToString()
                });
            };

            return value.Height;
        }

        public int AddDropDownItem(IDropDownRowConstructorArguments arguments)
        {
            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (arguments.ReadOnly)
            {
                return this.AddTextBoxItem(arguments);
            }

            this.AddLabel(arguments);

            ComboBox value = this.Container.AddControl(new ComboBox
            {
                Height = this.ITEM_HEIGHT,
                Top = this.CurrentTop,
                Name = arguments.Name,
                DropDownStyle = ComboBoxStyle.DropDownList
            });

            Action resize = new Action(() =>
            {
                value.Left = (this.Container.Width / 2);
                value.Width = (this.Container.Width / 2) - this.PanelPadding;
            });

            resize.Invoke();
            this.ResizeActions.Add(resize);

            value.Items.AddRange(arguments.Values.ToArray());

            value.SelectedIndex = value.Items.IndexOf(arguments.Value);

            value.SelectedIndexChanged += (sender, e) =>
            {
                ComboBox sText = (sender as ComboBox);

                arguments.OnChange?.Invoke(new ValueChangedEventArgs()
                {
                    SourceControl = sText,
                    Value = sText.SelectedItem.ToString()
                });
            };

            return value.Height;
        }

        public void AddLabel(ILabelConstructorArguments arguments)
        {
            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (string.IsNullOrWhiteSpace(arguments.Name))
            {
                return;
            }

            int realLeft = this.PanelPadding + arguments.LeftOffset;

            Label label = this.Container.AddControl(new Label
            {
                Height = this.ITEM_HEIGHT,
                Top = this.CurrentTop,
                Left = realLeft,
                Text = arguments.Name,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            });

            Action resize = new Action(() => label.Width = (this.Container.Width / 2) - realLeft);

            resize.Invoke();
            this.ResizeActions.Add(resize);

            if (!string.IsNullOrWhiteSpace(arguments.ToolTip))
            {
                ToolTip toolTip = new ToolTip()
                {
                    Tag = label,
                    AutoPopDelay = 32767
                };

                toolTip.SetToolTip(label, arguments.ToolTip);
            }
        }

        public int AddTextBoxItem(ITextBoxRowConstructorArguments arguments)
        {
            this.AddLabel(arguments);

            AutoSizeTextBox value = this.Container.AddControl(new AutoSizeTextBox
            {
                Height = this.ITEM_HEIGHT * (arguments.Value ?? string.Empty).Count(c => c == '\n'),
                Top = this.CurrentTop,
                Name = arguments.Name,
                Text = (arguments.Value ?? string.Empty),
                Multiline = (arguments.Value ?? string.Empty).Contains(System.Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            });

            Action resize = new Action(() =>
            {
                value.Left = (this.Container.Width / 2);
                value.Width = (this.Container.Width / 2) - this.PanelPadding;
            });
            resize.Invoke();
            this.ResizeActions.Add(resize);

            if (arguments.ReadOnly)
            {
                value.ReadOnly = true;
                value.BackColor = System.Drawing.SystemColors.Control;
            }

            ResizeTextBox();

            void ResizeTextBox()
            {
                int sizeChange = 0 - value.ClientSize.Height;

                value.CallAutoSize();

                if (value.Height > this.ITEM_HEIGHT)
                {
                    sizeChange += value.ClientSize.Height;

                    foreach (Control c in this.Container.Controls)
                    {
                        if (c.Top > value.Top)
                        {
                            c.Top += sizeChange;
                        }
                    }
                }
                else
                {
                    value.Height = this.ITEM_HEIGHT;
                }
            }

            value.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && !value.Multiline)
                {
                    value.Multiline = true;

                    string text = $"{value.Text.Substring(0, value.SelectionStart)}{System.Environment.NewLine}{value.Text.Substring(value.SelectionStart)}";

                    value.Text = text;
                }
            };

            value.TextChanged += (sender, e) =>
            {
                AutoSizeTextBox sText = (sender as AutoSizeTextBox);

                arguments.OnChange?.Invoke(new ValueChangedEventArgs()
                {
                    SourceControl = sText,
                    Value = sText.Text
                });

                ResizeTextBox();
            };

            return value.Height;
        }

        public string GetToolTipText(Type objectType)
        {
            if (!this.ToolTipCache.TryGetValue(objectType, out string toReturn))
            {
                toReturn = string.Empty;

                Queue<Type> typesToCheck = new Queue<Type>();

                typesToCheck.Enqueue(objectType);
                bool hasNext;
                do
                {
                    Type toCheck = typesToCheck.Dequeue();

                    toReturn += $"{toCheck}{System.Environment.NewLine}";

                    if (toCheck.BaseType != null && toCheck.BaseType != typeof(object))
                    {
                        typesToCheck.Enqueue(toCheck.BaseType);
                    }

                    foreach (Type parameterType in toCheck.GetGenericArguments())
                    {
                        typesToCheck.Enqueue(parameterType);
                    }

                    hasNext = typesToCheck.Any();

                    if (hasNext)
                    {
                        toReturn += $"----------------------{System.Environment.NewLine}";
                    }
                } while (hasNext);

                this.ToolTipCache.Add(objectType, toReturn);
            }
            return toReturn;
        }

        public void Load()
        {
            this.Container.SuspendDrawing();

            this.Container.Controls.Clear();

            this.CurrentTop = 0;

            Button saveButton = this.Container.AddControl(new Button
            {
                Text = "Save",
                Top = this.CurrentTop,
                Left = this.PanelPadding,
                Height = 35,
                Width = 85
            });

            //FixMe
            saveButton.Parent.Top = 0;
            saveButton.Click += (sender, e) => this.OnSave.Invoke(this.Retrieve());

            this.CurrentTop += saveButton.Height + this.ITEM_SPACING;

            this.AddLabel(new LabelConstructorArguments(this.TemporaryObject.GetType().Name));

            this.CurrentTop += this.ITEM_HEIGHT;

            this.RenderProperties(this.TemporaryObject, this.TemporaryObject.GetType(), 0);

            this.Container.ResumeDrawing();
        }

        public object Render(object value, Type objectType, string Name, int left, Action<ValueChangedEventArgs> onChange, bool readOnly = false)
        {
            if (!(value is null))
            {
                objectType = value.GetType();
            }

            if (objectType is null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            bool isString = objectType == typeof(string);

            bool isCollection = objectType.ImplementsInterface(typeof(IList<>));

            bool isValue = objectType.IsValueType || isString;

            bool isObject = !isString && !isCollection;

            if (!isString && !isCollection && !isObject)
            {
                throw new Exception("Something went terribly wrong");
            }

            string toolTipText = this.GetToolTipText(objectType);

            if (objectType == typeof(bool))
            {
                BoolRowConstructorArguments boolConstructorArguments = new BoolRowConstructorArguments()
                {
                    Name = Name,
                    Value = (bool)value,
                    ReadOnly = readOnly,
                    OnChange = onChange,
                    LeftOffset = left,
                    ToolTip = toolTipText
                };

                this.AddBoolItem(boolConstructorArguments);

                this.CurrentTop += this.ITEM_HEIGHT + this.ITEM_SPACING;
            }
            else if (objectType.IsEnum)
            {
                List<string> values = new List<string>();

                foreach (object EnumVal in Enum.GetValues(objectType))
                {
                    values.Add(EnumVal.ToString());
                }

                DropDownRowConstructorArguments arguments = new DropDownRowConstructorArguments()
                {
                    LeftOffset = left,
                    Name = Name,
                    Values = values,
                    OnChange = onChange,
                    ToolTip = toolTipText,
                    Value = value?.ToString(),
                    ReadOnly = readOnly
                };

                this.AddDropDownItem(arguments);
                this.CurrentTop += this.ITEM_HEIGHT + this.ITEM_SPACING;
            }
            else if (isCollection)
            {
                Type collectionType = objectType.GetCollectionType();

                this.AddLabel(new LabelConstructorArguments(Name)
                {
                    LeftOffset = left,
                    ToolTip = toolTipText
                });

                Button addButton = this.Container.AddControl(new Button
                {
                    Text = "Add",
                    Top = this.CurrentTop,
                    Height = 35,
                    Width = 85
                });

                Action resize = new Action(() => addButton.Left = (this.Container.Width / 2));

                resize.Invoke();
                this.ResizeActions.Add(resize);

                addButton.Click += (sender, e) =>
                {
                    object toAdd = null;

                    if (collectionType == typeof(string))
                    {
                        toAdd = string.Empty;
                    }
                    else if (collectionType.IsValueType)
                    {
                        toAdd = collectionType.GetDefaultValue();
                    }
                    else
                    {
                        if (SelectTypeForm.GetOptions(collectionType).Count > 1)
                        {
                            SelectTypeForm selectForm = new SelectTypeForm(collectionType);

                            if (selectForm.ShowDialog() == DialogResult.OK)
                            {
                                toAdd = Activator.CreateInstance(selectForm.Result);
                            }
                        }
                        else
                        {
                            toAdd = Activator.CreateInstance(collectionType);
                        }
                    }

                    if (toAdd != null)
                    {
                        (value as IList).Add(toAdd);
                        this.Load();
                    }
                };

                if (!string.IsNullOrWhiteSpace(Name))
                {
                    this.CurrentTop += this.ITEM_HEIGHT + this.ITEM_SPACING;
                }

                if (value is null)
                {
                    value = Activator.CreateInstance(objectType);
                }

                IList vList = value as IList;
                int index = 0;

                foreach (object o in vList.Cast<object>().ToList())
                {
                    int thisIndex = index;

                    Button removeButton = this.Container.AddControl(new Button
                    {
                        Text = "-",
                        Left = left + this.PanelPadding,
                        Top = this.CurrentTop,
                        Width = this.LIST_PADDING,
                        Height = this.ITEM_HEIGHT
                    });

                    removeButton.Click += (sender, e) =>
                    {
                        vList.RemoveAt(thisIndex);
                        this.Load();
                    };

                    Action<ValueChangedEventArgs> entryChanged = (t) =>
                    {
                        if (this.TryCast(t, collectionType, out object result))
                        {
                            vList.RemoveAt(thisIndex);
                            vList.Insert(thisIndex, result);
                        }
                    };

                    this.Render(o, collectionType, string.Empty, left + this.LIST_PADDING, entryChanged, readOnly);

                    index++;
                }
            }
            else if (isValue)
            {
                TextBoxRowConstructorArguments arguments = new TextBoxRowConstructorArguments()
                {
                    Name = Name,
                    Value = value?.ToString(),
                    ReadOnly = readOnly,
                    LeftOffset = left,
                    OnChange = onChange,
                    ToolTip = toolTipText
                };

                int vHeight = this.AddTextBoxItem(arguments);
                this.CurrentTop += vHeight + this.ITEM_SPACING;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    this.AddLabel(new LabelConstructorArguments(Name) { LeftOffset = left, ToolTip = toolTipText });
                    this.CurrentTop += this.ITEM_HEIGHT + this.ITEM_SPACING;
                }

                if (value is null)
                {
                    if (!objectType.IsAbstract)
                    {
                        value = Activator.CreateInstance(objectType);
                    }
                }

                if (!(value is null))
                {
                    this.RenderProperties(value, objectType, left + this.LIST_PADDING);
                }
            }

            return value;
        }

        public void RenderProperties(object item, Type objectType, int left)
        {
            if (objectType is null)
            {
                throw new ArgumentNullException(nameof(objectType));
            }

            IEnumerable<PropertyInfo> properties = objectType.GetProperties().OrderBy(pi => pi.GetCustomAttribute<DisplayAttribute>()?.Order ?? 0);
            foreach (PropertyInfo pi in properties)
            {
                if (pi.GetGetMethod() is null)
                {
                    continue;
                }
                Action<ValueChangedEventArgs> onPropertyChange = null;

                bool readOnly = pi.GetSetMethod() is null;

                if (!readOnly)
                {
                    onPropertyChange = (t) =>
                    {
                        if (this.TryCast(t, pi.PropertyType, out object result))
                        {
                            pi.SetValue(item, t.Value.Convert(pi.PropertyType));
                        }
                    };
                }

                void RenderException(Exception ex)
                {
                    RenderExceptionWrapper renderException = new RenderExceptionWrapper(ex);

                    this.Render(renderException, typeof(RenderExceptionWrapper), pi.Name, left, (v) => { }, true);
                }

                try
                {
                    if (typeof(Exception).IsAssignableFrom(pi.PropertyType))
                    {
                        if (pi.GetValue(item) is Exception ex)
                        {
                            RenderException(ex);
                        }
                    }
                    else
                    {
                        object renderableObject = this.Render(pi.GetValue(item), pi.PropertyType, pi.Name, left, onPropertyChange, readOnly);

                        if (!readOnly)
                        {
                            pi.SetValue(item, renderableObject);
                        }
                    }
                }
                catch (Exception ex)
                {
                    RenderException(ex);
                }
            }
        }

        public T Retrieve() => this.TemporaryObject.JsonClone();

        public bool TryCast(ValueChangedEventArgs valueChangeEventArgs, Type type, out object result)
        {
            if (valueChangeEventArgs is null)
            {
                throw new ArgumentNullException(nameof(valueChangeEventArgs));
            }

            result = null;
            try
            {
                result = valueChangeEventArgs.Value.Convert(type);

                valueChangeEventArgs.SourceControl.BackColor = System.Drawing.Color.White;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine(ex.Message);

                valueChangeEventArgs.SourceControl.BackColor = System.Drawing.Color.Red;
                return false;
            }
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    //this.Container.Dispose();
                } catch(Exception ex) 
                {
                    // Split Container Dispose Error
                    // ref https://stackoverflow.com/questions/19055526/splitcontainer-error
                    Debug.WriteLine(ex.Message);
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~ObjectEditor()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(false);
        }

        #endregion IDisposable Support
    }
}