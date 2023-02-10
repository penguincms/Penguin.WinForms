using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Events;
using System;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class TextBoxRowConstructorArguments : ITextBoxRowConstructorArguments
    {
        public int LeftOffset { get; set; }

        public string Name { get; set; }

        public Action<ValueChangedEventArgs> OnChange { get; set; }

        public bool ReadOnly { get; set; }

        public string ToolTip { get; set; }

        public string Value { get; set; }
    }
}