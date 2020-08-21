namespace Penguin.WinForms.Editors.Dependencies.Interfaces
{
    public interface IPropertyEditor
    {
        object Value { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        int Top { get; set; }
        int Left { get; set; }
    }

    public interface IPropertyEditor<out TControl> : IPropertyEditor
    {
        TControl Control { get; }
    }
}
