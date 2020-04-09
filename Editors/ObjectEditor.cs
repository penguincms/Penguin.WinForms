using Newtonsoft.Json;
using Penguin.Json.Extensions;
using Penguin.Reflection.Extensions;
using Penguin.WinForms.Editors.Component;
using Penguin.WinForms.Editors.ConstructorArguments;
using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Penguin.WinForms.Editors
{
    public class ObjectEditor<T> : IDisposable
    {
        public string EditorId { get; set; }
        public int ITEM_HEIGHT = 25;
        public int ITEM_SPACING = 3;
        public int LIST_PADDING = 15;
        public float WIDTH_PADDING_PER = .05f;

        private int _currentTop = 0;

        private Action<T> OnSave;
        private Dictionary<Type, string> ToolTipCache = new Dictionary<Type, string>();

        public int CurrentTop
        {
            get
            {
                return _currentTop;
            }
            protected set
            {
                EditorCache.Container.Height = value;
                _currentTop = value;
            }
        }

        public int PanelPadding
        {
            get
            {
                return (int)(EditorCache.Container.Width * WIDTH_PADDING_PER);
            }
        }

        public T TemporaryObject { get; protected set; }

        private JsonSerializerSettings DefaultSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings()
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
            }
        }

        EditorCache EditorCache;

        public ObjectEditor(string editorId, T toEdit, Panel container, Action<T> onSave, bool multiThread = true)
        {
            EditorId = editorId;

            EditorCache = new EditorCache(editorId, container, multiThread);

            TemporaryObject = toEdit.JsonClone();

            OnSave = onSave;

            

            Load();
        }

        public int AddDropDownItem(IDropDownRowConstructorArguments arguments)
        {
            if (arguments.ReadOnly)
            {
                return AddTextBoxItem(arguments);
            }

            AddLabel(arguments);

            ComboBox value = EditorCache.ComponentFactory.Request<ComboBox>();

            value.Height = ITEM_HEIGHT;
            value.Top = CurrentTop;
            value.Left = (EditorCache.Container.Width / 2);
            value.Width = (EditorCache.Container.Width / 2) - PanelPadding;
            value.Name = arguments.Name;
            value.DropDownStyle = ComboBoxStyle.DropDownList;

            value.Items.AddRange(arguments.Values.ToArray());

            value.SelectedIndex = value.Items.IndexOf(arguments.Value);

            value.TextChanged += (sender, e) =>
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
            if (string.IsNullOrWhiteSpace(arguments.Name))
            {
                return;
            }

            int realLeft = PanelPadding + arguments.LeftOffset;

            Label label = EditorCache.ComponentFactory.Request<Label>();

            label.Height = ITEM_HEIGHT;
            label.Top = CurrentTop;
            label.Left = realLeft;
            label.Text = arguments.Name;
            label.Width = (EditorCache.Container.Width / 2) - realLeft;
            label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

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
            AddLabel(arguments);

            TextBox value = EditorCache.ComponentFactory.Request<TextBox>();

            value.Height = ITEM_HEIGHT * (arguments.Value ?? string.Empty).Count(c => c == '\n');
            value.Top = CurrentTop;
            value.Left = (EditorCache.Container.Width / 2);
            value.Width = (EditorCache.Container.Width / 2) - PanelPadding;
            value.Name = arguments.Name;
            value.Text = (arguments.Value ?? string.Empty);
            value.Multiline = (arguments.Value ?? string.Empty).Contains(System.Environment.NewLine);

            if (arguments.ReadOnly)
            {
                value.ReadOnly = true;
                value.BackColor = System.Drawing.SystemColors.Control;
            }

            ResizeTextBox();

            void ResizeTextBox()
            {
                int sizeChange = 0 - value.ClientSize.Height;
                const int y_margin = 2;
                Size size = TextRenderer.MeasureText(value.Text, value.Font);

                value.ClientSize = new Size(value.ClientSize.Width, size.Height + y_margin);

                if (value.Height > ITEM_HEIGHT)
                {
                    sizeChange += value.ClientSize.Height;

                    foreach (Control c in EditorCache.ComponentFactory.ActiveControls)
                    {
                        if (c.Top > value.Top)
                        {
                            c.Top += sizeChange;
                        }
                    }
                }
                else
                {
                    value.Height = ITEM_HEIGHT;
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
                TextBox sText = (sender as TextBox);

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

                ToolTipCache.Add(objectType, toReturn);
            }
            return toReturn;
        }

        public void Load()
        {
            EditorCache.ComponentFactory.Clear();

            CurrentTop = 0;

            AddLabel(new LabelConstructorArguments(this.TemporaryObject.GetType().Name));

            this.CurrentTop += ITEM_HEIGHT;

            RenderProperties(TemporaryObject, TemporaryObject.GetType(), 0);

            Button saveButton = EditorCache.ComponentFactory.Request<Button>();

            saveButton.Text = "Save";
            saveButton.Top = CurrentTop;
            saveButton.Left = PanelPadding;

            saveButton.Height = 35;
            saveButton.Width = 45;

            saveButton.Click += (sender, e) =>
            {
                OnSave.Invoke(this.Retrieve());
            };

            CurrentTop += saveButton.Height + ITEM_SPACING;
        }

        public object Render(object value, Type objectType, string Name, int left, Action<ValueChangedEventArgs> onChange, bool readOnly = false)
        {
            if (!(value is null))
            {
                objectType = value.GetType();
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

            if (objectType.IsEnum)
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
                    Value = value?.ToString()
                };

                AddDropDownItem(arguments);
                CurrentTop += ITEM_HEIGHT + ITEM_SPACING;
            }
            else if (isCollection)
            {
                Type collectionType = objectType.GetCollectionType();

                int labelWidth = (EditorCache.Container.Width / 2) - (PanelPadding + left);

                AddLabel(new LabelConstructorArguments(Name)
                {
                    LeftOffset = left,
                    ToolTip = toolTipText
                });

                int optionLeft = (EditorCache.Container.Width / 2);

                Button addButton = EditorCache.ComponentFactory.Request<Button>();

                addButton.Text = "Add";
                addButton.Top = CurrentTop;
                addButton.Left = optionLeft;
                addButton.Height = 20;
                addButton.Width = 45;

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
                        Load();
                    }
                };

                if (!string.IsNullOrWhiteSpace(Name))
                {
                    CurrentTop += ITEM_HEIGHT + ITEM_SPACING;
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

                    Button removeButton = EditorCache.ComponentFactory.Request<Button>();

                    removeButton.Text = "-";

                    removeButton.Left = left + PanelPadding;
                    removeButton.Top = CurrentTop;
                    removeButton.Width = LIST_PADDING;
                    removeButton.Height = ITEM_HEIGHT;

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

                    Render(o, collectionType, string.Empty, left + LIST_PADDING, entryChanged, readOnly);

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

                int vHeight = AddTextBoxItem(arguments);
                CurrentTop += vHeight + ITEM_SPACING;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    AddLabel(new LabelConstructorArguments(Name) { LeftOffset = left, ToolTip = toolTipText });
                    CurrentTop += ITEM_HEIGHT + ITEM_SPACING;
                }

                if (value is null)
                {
                    value = Activator.CreateInstance(objectType);
                }

                RenderProperties(value, objectType, left + LIST_PADDING);
            }

            return value;
        }

        public void RenderProperties(object item, Type objectType, int left)
        {
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

                object renderableObject = Render(pi.GetValue(item), pi.PropertyType, pi.Name, left, onPropertyChange, readOnly);

                if (!readOnly)
                {
                    pi.SetValue(item, renderableObject);
                }
            }
        }

        public T Retrieve()
        {
            return TemporaryObject.JsonClone();
        }

        public bool TryCast(ValueChangedEventArgs valueChangeEventArgs, Type type, out object result)
        {
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {

                 this.EditorCache.Dispose();

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