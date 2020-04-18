﻿using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using Penguin.WinForms.Editors.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class BoolConstructorArguments : IBoolConstructorArguments
    {
        public bool Value { get; set; }
        public string Name { get; set; }
        public Action<ValueChangedEventArgs> OnChange { get; set; }
        public bool ReadOnly { get; set; }
    }
}
