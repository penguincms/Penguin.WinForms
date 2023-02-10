using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Events;
using System;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class TextBoxConstructorArguments : IItemConstructorArguments
    {
        public string Name { get; set; }

        public Action<ValueChangedEventArgs> OnChange { get; }

        public bool ReadOnly { get; }
    }
}