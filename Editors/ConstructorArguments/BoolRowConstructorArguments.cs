using Penguin.WinForms.Editors.ConstructorArguments.Interfaces;

namespace Penguin.WinForms.Editors.ConstructorArguments
{
    public class BoolRowConstructorArguments : BoolConstructorArguments, IBoolRowConstructorArguments
    {
        public int LeftOffset { get; set; }
        public string ToolTip { get; set; }
    }
}
