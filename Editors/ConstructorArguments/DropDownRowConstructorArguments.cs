using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Events;
using System;
using System.Collections.Generic;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class DropDownRowConstructorArguments : IDropDownRowConstructorArguments
    {
        public int LeftOffset { get; set; }

        public string Name { get; set; }

        public Action<ValueChangedEventArgs> OnChange { get; set; }

        public bool ReadOnly { get; set; }

        public string ToolTip { get; set; }

        public string Value { get; set; }

        public IEnumerable<string> Values { get; set; }
    }
}