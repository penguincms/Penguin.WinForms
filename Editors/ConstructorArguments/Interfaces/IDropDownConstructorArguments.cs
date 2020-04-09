using System.Collections.Generic;

namespace Penguin.WinForms.Editors.ConstructorArguments.Interfaces
{
    public interface IDropDownConstructorArguments : ITextBoxConstructorArguments
    {
        IEnumerable<string> Values { get; }
    }
}