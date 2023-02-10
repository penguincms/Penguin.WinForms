namespace Penguin.WinForms.Editors.Dependencies.Interfaces
{
    public interface IPropertyEditor
    {
        int Height { get; set; }

        int Left { get; set; }

        int Top { get; set; }

        object Value { get; set; }

        int Width { get; set; }
    }
}