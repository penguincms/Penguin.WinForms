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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors
{
    public class ObjectEditor<T> : IDisposable
    {
        private readonly Panel Container;
        private readonly Action<T> OnSave;
        private readonly List<Action> ResizeActions = new();
        private readonly Dictionary<Type, string> ToolTipCache = new();
        private int _currentTop;

        public int CurrentTop
        {
            get => _currentTop;
            protected set
            {
                Container.Height = value;
                _currentTop = value;
            }
        }

        public int ITEMHEIGHT { get; set; } = 35;
        public int ITEMSPACING { get; set; } = 3;
        public int LISTPADDING { get; set; } = 25;
        public int PanelPadding => (int)(Container.Width * WIDTHPADDINGPER);
        public T TemporaryObject { get; protected set; }
        public float WIDTHPADDINGPER { get; set; } = .05f;

        public ObjectEditor(T toEdit, Panel container, Action<T> onSave)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            Container = container;

            Container.Resize += (o, s) =>
            {
                foreach (Action a in ResizeActions)
                {
                    a.Invoke();
                }
            };

            TemporaryObject = toEdit.JsonClone();

            OnSave = onSave;

            Load();
        }

        public int AddBoolItem(IBoolRowConstructorArguments arguments)
        {
            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            AddLabel(arguments);

            CheckBox value = Container.AddControl(new CheckBox
            {
                Height = ITEMHEIGHT,
                Top = CurrentTop,
                Name = arguments.Name,
                Checked = arguments.Value
            });

            Action resize = new(() =>
            {
                value.Left = Container.Width / 2;
                value.Width = (Container.Width / 2) - PanelPadding;
            });

            resize.Invoke();

            ResizeActions.Add(resize);

            value.CheckedChanged += (sender, e) =>
            {
                CheckBox sText = sender as CheckBox;

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
                return AddTextBoxItem(arguments);
            }

            AddLabel(arguments);

            ComboBox value = Container.AddControl(new ComboBox
            {
                Height = ITEMHEIGHT,
                Top = CurrentTop,
                Name = arguments.Name,
                DropDownStyle = ComboBoxStyle.DropDownList
            });

            Action resize = new(() =>
            {
                value.Left = Container.Width / 2;
                value.Width = (Container.Width / 2) - PanelPadding;
            });

            resize.Invoke();
            ResizeActions.Add(resize);

            value.Items.AddRange(arguments.Values.ToArray());

            value.SelectedIndex = value.Items.IndexOf(arguments.Value);

            value.SelectedIndexChanged += (sender, e) =>
            {
                ComboBox sText = sender as ComboBox;

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

            int realLeft = PanelPadding + arguments.LeftOffset;

            Label label = Container.AddControl(new Label
            {
                Height = ITEMHEIGHT,
                Top = CurrentTop,
                Left = realLeft,
                Text = arguments.Name,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            });

            Action resize = new(() => label.Width = (Container.Width / 2) - realLeft);

            resize.Invoke();
            ResizeActions.Add(resize);

            if (!string.IsNullOrWhiteSpace(arguments.ToolTip))
            {
                ToolTip toolTip = new()
                {
                    Tag = label,
                    AutoPopDelay = 32767
                };

                toolTip.SetToolTip(label, arguments.ToolTip);
            }
        }

        public int AddTextBoxItem(ITextBoxRowConstructorArguments arguments)
        {
            AddLabel(arguments);

            AutoSizeTextBox value = Container.AddControl(new AutoSizeTextBox
            {
                Height = ITEMHEIGHT * (arguments.Value ?? string.Empty).Count(c => c == '\n'),
                Top = CurrentTop,
                Name = arguments.Name,
                Text = arguments.Value ?? string.Empty,
                Multiline = (arguments.Value ?? string.Empty).Contains(System.Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            });

            Action resize = new(() =>
            {
                value.Left = Container.Width / 2;
                value.Width = (Container.Width / 2) - PanelPadding;
            });
            resize.Invoke();
            ResizeActions.Add(resize);

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

                if (value.Height > ITEMHEIGHT)
                {
                    sizeChange += value.ClientSize.Height;

                    foreach (Control c in Container.Controls)
                    {
                        if (c.Top > value.Top)
                        {
                            c.Top += sizeChange;
                        }
                    }
                }
                else
                {
                    value.Height = ITEMHEIGHT;
                }
            }

            value.KeyUp += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter && !value.Multiline)
                {
                    value.Multiline = true;

                    string text = $"{value.Text[..value.SelectionStart]}{System.Environment.NewLine}{value.Text[value.SelectionStart..]}";

                    value.Text = text;
                }
            };

            value.TextChanged += (sender, e) =>
            {
                AutoSizeTextBox sText = sender as AutoSizeTextBox;

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
            if (!ToolTipCache.TryGetValue(objectType, out string toReturn))
            {
                toReturn = string.Empty;

                Queue<Type> typesToCheck = new();

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

                ToolTipCache.Add(objectType, toReturn);
            }
            return toReturn;
        }

        public void Load()
        {
            Container.SuspendDrawing();

            Container.Controls.Clear();

            CurrentTop = 0;

            Button saveButton = Container.AddControl(new Button
            {
                Text = "Save",
                Top = CurrentTop,
                Left = PanelPadding,
                Height = 35,
                Width = 85
            });

            //FixMe
            saveButton.Parent.Top = 0;
            saveButton.Click += (sender, e) => OnSave.Invoke(Retrieve());

            CurrentTop += saveButton.Height + ITEMSPACING;

            AddLabel(new LabelConstructorArguments(TemporaryObject.GetType().Name));

            CurrentTop += ITEMHEIGHT;

            RenderProperties(TemporaryObject, TemporaryObject.GetType(), 0);

            Container.ResumeDrawing();
        }

        public object Render(object value, Type objectType, string Name, int left, Action<ValueChangedEventArgs> onChange, bool readOnly = false)
        {
            if (value is not null)
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

            string toolTipText = GetToolTipText(objectType);

            if (objectType == typeof(bool))
            {
                BoolRowConstructorArguments boolConstructorArguments = new()
                {
                    Name = Name,
                    Value = (bool)value,
                    ReadOnly = readOnly,
                    OnChange = onChange,
                    LeftOffset = left,
                    ToolTip = toolTipText
                };

                _ = AddBoolItem(boolConstructorArguments);

                CurrentTop += ITEMHEIGHT + ITEMSPACING;
            }
            else if (objectType.IsEnum)
            {
                List<string> values = new();

                foreach (object EnumVal in Enum.GetValues(objectType))
                {
                    values.Add(EnumVal.ToString());
                }

                DropDownRowConstructorArguments arguments = new()
                {
                    LeftOffset = left,
                    Name = Name,
                    Values = values,
                    OnChange = onChange,
                    ToolTip = toolTipText,
                    Value = value?.ToString(),
                    ReadOnly = readOnly
                };

                _ = AddDropDownItem(arguments);
                CurrentTop += ITEMHEIGHT + ITEMSPACING;
            }
            else if (isCollection)
            {
                Type collectionType = objectType.GetCollectionType();

                AddLabel(new LabelConstructorArguments(Name)
                {
                    LeftOffset = left,
                    ToolTip = toolTipText
                });

                Button addButton = Container.AddControl(new Button
                {
                    Text = "Add",
                    Top = CurrentTop,
                    Height = 35,
                    Width = 85
                });

                Action resize = new(() => addButton.Left = Container.Width / 2);

                resize.Invoke();
                ResizeActions.Add(resize);

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
                            SelectTypeForm selectForm = new(collectionType);

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
                        _ = (value as IList).Add(toAdd);
                        Load();
                    }
                };

                if (!string.IsNullOrWhiteSpace(Name))
                {
                    CurrentTop += ITEMHEIGHT + ITEMSPACING;
                }

                value ??= Activator.CreateInstance(objectType);

                IList vList = value as IList;
                int index = 0;

                foreach (object o in vList.Cast<object>().ToList())
                {
                    int thisIndex = index;

                    Button removeButton = Container.AddControl(new Button
                    {
                        Text = "-",
                        Left = left + PanelPadding,
                        Top = CurrentTop,
                        Width = LISTPADDING,
                        Height = ITEMHEIGHT
                    });

                    removeButton.Click += (sender, e) =>
                    {
                        vList.RemoveAt(thisIndex);
                        Load();
                    };

                    Action<ValueChangedEventArgs> entryChanged = (t) =>
                    {
                        if (TryCast(t, collectionType, out object result))
                        {
                            vList.RemoveAt(thisIndex);
                            vList.Insert(thisIndex, result);
                        }
                    };

                    _ = Render(o, collectionType, string.Empty, left + LISTPADDING, entryChanged, readOnly);

                    index++;
                }
            }
            else if (isValue)
            {
                TextBoxRowConstructorArguments arguments = new()
                {
                    Name = Name,
                    Value = value?.ToString(),
                    ReadOnly = readOnly,
                    LeftOffset = left,
                    OnChange = onChange,
                    ToolTip = toolTipText
                };

                int vHeight = AddTextBoxItem(arguments);
                CurrentTop += vHeight + ITEMSPACING;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    AddLabel(new LabelConstructorArguments(Name) { LeftOffset = left, ToolTip = toolTipText });
                    CurrentTop += ITEMHEIGHT + ITEMSPACING;
                }

                if (value is null)
                {
                    if (!objectType.IsAbstract)
                    {
                        value = Activator.CreateInstance(objectType);
                    }
                }

                if (value is not null)
                {
                    RenderProperties(value, objectType, left + LISTPADDING);
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
                        if (TryCast(t, pi.PropertyType, out object result))
                        {
                            pi.SetValue(item, t.Value.Convert(pi.PropertyType));
                        }
                    };
                }

                void RenderException(Exception ex)
                {
                    RenderExceptionWrapper renderException = new(ex);

                    _ = Render(renderException, typeof(RenderExceptionWrapper), pi.Name, left, (v) => { }, true);
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
                        object renderableObject = Render(pi.GetValue(item), pi.PropertyType, pi.Name, left, onPropertyChange, readOnly);

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

        public T Retrieve()
        {
            return TemporaryObject.JsonClone();
        }

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

        private bool disposedValue; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);// TODO: uncomment the following line if the finalizer is overridden above.// GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                try
                {
                    //this.Container.Dispose();
                }
                catch (Exception ex)
                {
                    // Split Container Dispose Error
                    // ref https://stackoverflow.com/questions/19055526/splitcontainer-error
                    Debug.WriteLine(ex.Message);
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~ObjectEditor()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        #endregion IDisposable Support
    }
}