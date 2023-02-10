using Penguin.WinForms.Editors.Events;
using System;

namespace Penguin.WinForms.Editors.ConstructorArguments.Interfaces
{
    public interface IItemConstructorArguments
    {
        string Name { get; }

        Action<ValueChangedEventArgs> OnChange { get; }

        bool ReadOnly { get; }
    }
}