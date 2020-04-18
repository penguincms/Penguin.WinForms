using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class BoolRowConstructorArguments : BoolConstructorArguments, IBoolRowConstructorArguments
    {
        public int LeftOffset { get; set; }
        public string ToolTip { get; set; }
    }
}
