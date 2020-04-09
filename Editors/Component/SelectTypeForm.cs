using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.WinForms.Editors.Component
{
    public class SelectTypeForm : SelectForm<Type>
    {
        public SelectTypeForm(Type baseType, SelectTypeDisplayStyle style = SelectTypeDisplayStyle.Name, Action<Type> onSelect = null) : base(GetOptions(baseType), "Please Select A Type", onSelect, GetDisplayFunc(style))
        {
        }

        public static List<Type> GetOptions(Type baseType)
        {
            List<Type> TypeOptions = new List<Type>()
                {
                    baseType
                };

            if (baseType.IsInterface)
            {
                TypeOptions.AddRange(TypeFactory.GetAllImplementations(baseType));
            }
            else
            {
                TypeOptions.AddRange(TypeFactory.GetDerivedTypes(baseType));
            }

            return TypeOptions.Distinct().ToList();
        }

        private static Func<Type, string> GetDisplayFunc(SelectTypeDisplayStyle style)
        {
            switch (style)
            {
                case SelectTypeDisplayStyle.Name:
                    return new Func<Type, string>((t) => t.Name);

                case SelectTypeDisplayStyle.FullName:
                    return new Func<Type, string>((t) => t.FullName);

                case SelectTypeDisplayStyle.ToString:
                    return new Func<Type, string>((t) => t.ToString());

                default:
                    throw new ArgumentException($"Unhandled type display style {style}", "style");
            }
        }
    }

    public enum SelectTypeDisplayStyle
    {
        Name,
        FullName,
        ToString
    }
}