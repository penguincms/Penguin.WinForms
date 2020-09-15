using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Events;
using System;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class BoolConstructorArguments : IBoolConstructorArguments
    {
        public string Name { get; set; }
        public Action<ValueChangedEventArgs> OnChange { get; set; }
        public bool ReadOnly { get; set; }
        public bool Value { get; set; }
    }
}