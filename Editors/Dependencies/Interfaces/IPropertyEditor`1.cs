namespace Penguin.WinForms.Editors.Dependencies.Interfaces
{
    public interface IPropertyEditor<out TControl> : IPropertyEditor
    {
        TControl Control { get; }
    }
}