using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Events;
using System;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class LabelConstructorArguments : ILabelConstructorArguments
    {
        public int LeftOffset { get; set; }

        public string Name { get; set; }

        Action<ValueChangedEventArgs> IItemConstructorArguments.OnChange => null;

        bool IItemConstructorArguments.ReadOnly => true;

        public string ToolTip { get; set; }

        public LabelConstructorArguments(string labelText)
        {
            Name = labelText;
        }
    }
}