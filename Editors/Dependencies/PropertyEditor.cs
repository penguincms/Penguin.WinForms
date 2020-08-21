//using Penguin.Reflection.Extensions;
//using Penguin.WinForms.Editors.Dependencies.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using System.Text;
//using System.Windows.Forms;

//namespace Penguin.WinForms.Editors.Dependencies
//{
//    public static class PropertyEditor
//    {
//        public static IPropertyEditor<TControl> Create<TControl>(object parent, PropertyInfo property, Func<TControl> factory) where TControl : TextBox
//        {
//            TextBox tb = factory?.Invoke() ?? new TextBox();

//            IPropertyEditor<TControl> pe = new PropertyEditorInstance<TextBox, string>(tb, parent, property,
//                (tb) => tb.Text,
//                (tb, v) => tb.Text = v
//            );

//            bool disableEvents = false;

//            tb.ReadOnly = IsReadOnly(property);

//            if (!tb.ReadOnly)
//            {

//                tb.TextChanged += (o, s) =>
//                {
//                    if (!disableEvents)
//                    {
//                        disableEvents = true;

//                        pe.Value = (o as TextBox).Text;

//                        disableEvents = false;
//                    }
//                };
//            } else
//            {
//                tb.BackColor = System.Drawing.SystemColors.Control;
//            }

//            return pe;
//        }

//        public static bool IsReadOnly(PropertyInfo property)
//        {
//            if (property is null)
//            {
//                throw new ArgumentNullException(nameof(property));
//            }

//            return property.GetSetMethod() is null;
//        }


//        private class PropertyEditorInstance<TControl, TValue> : IPropertyEditor, IPropertyEditor<TControl> where TControl : Control
//        {
//            private Func<TControl, TValue> GetControlValue;
//            private Action<TControl, TValue> SetControlValue;

//            public PropertyEditorInstance(TControl control, object parent, PropertyInfo propertyInfo, Func<TControl, TValue> getControlValue, Action<TControl, TValue> setControlValue)
//            {
//                Control = control;
//                Parent = parent;
//                PropertyInfo = propertyInfo;
//                GetControlValue = getControlValue;
//                SetControlValue = setControlValue;

//                SetControlValue(control, Value);
//            }

//            public int Top
//            {
//                get => Control.Top;
//                set => Control.Top = value;
//            }

//            public int Width
//            {
//                get => Control.Width;
//                set => Control.Width = value;
//            }

//            public int Height
//            {
//                get => Control.Height;
//                set => Control.Height = value;
//            }

//            public int Left
//            {
//                get => Control.Left;
//                set => Control.Left = value;
//            }

//            public TValue Value
//            {
//                get => (TValue)PropertyInfo.GetValue(Parent);
//                set
//                {
//                    SetControlValue(Control, value);
//                    PropertyInfo.SetValue(Parent, value);
//                }
//            }

//            public TControl Control { get; private set; }
//            public PropertyInfo PropertyInfo { get; set; }
//            public object Parent { get; set; }
//            public bool ReadOnly
//            {
//                get
//                {
//                    if (!readOnly.HasValue)
//                    {
//                        readOnly = PropertyInfo.GetSetMethod() is null;
//                    }

//                    return readOnly.Value;
//                }
//            }

//            object IPropertyEditor.Value
//            {
//                get => PropertyInfo.GetValue(Parent);
//                set
//                {
//                    object typedValue = value.ToString().Convert(this.PropertyInfo.PropertyType);

//                    SetControlValue(Control, (TValue)typedValue);
//                    PropertyInfo.SetValue(Parent, typedValue);
//                }
//            }

//            private bool? readOnly;
//        }
//    }
//}
