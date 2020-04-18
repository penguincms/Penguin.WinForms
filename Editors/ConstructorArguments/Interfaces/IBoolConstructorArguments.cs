using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.WinForms.Editors.ConstructorArguments.Interfaces
{
    public interface IBoolConstructorArguments : IItemConstructorArguments
    {
        bool Value { get; }
    }
}
