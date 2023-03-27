using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Loxifi;

namespace Penguin.WinForms.Editors.Component
{
    public class SelectTypeForm : SelectForm<Type>
    {
        public SelectTypeForm(Type baseType, SelectTypeDisplayStyle style = SelectTypeDisplayStyle.Name, Action<Type> onSelect = null) : base(GetOptions(baseType), "Please Select A Type", onSelect, GetDisplayFunc(style))
        {
        }

        public static List<Type> GetOptions(Type baseType)
        {
            if (baseType is null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            List<Type> TypeOptions = new()
                {
                    baseType
                };

            if (baseType.IsInterface)
            {
                TypeOptions.AddRange(TypeFactory.Default.GetAllImplementations(baseType));
            }
            else
            {
                TypeOptions.AddRange(TypeFactory.Default.GetDerivedTypes(baseType));
            }

            return TypeOptions.Distinct().ToList();
        }

        private static Func<Type, string> GetDisplayFunc(SelectTypeDisplayStyle style)
        {
            return style switch
            {
                SelectTypeDisplayStyle.Name => new Func<Type, string>((t) => t.Name),
                SelectTypeDisplayStyle.FullName => new Func<Type, string>((t) => t.FullName),
                SelectTypeDisplayStyle.ToString => new Func<Type, string>((t) => t.ToString()),
                _ => throw new ArgumentException($"Unhandled type display style {style}", nameof(style)),
            };
        }
    }
}