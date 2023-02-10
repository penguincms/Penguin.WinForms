using System.Windows.Forms;

namespace Penguin.WinForms.Editors.Events
{
    public class ValueChangedEventArgs
    {
        public Control SourceControl { get; set; }

        public string Value { get; internal set; }
    }
}