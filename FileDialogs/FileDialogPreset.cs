using Penguin.WinForms.Strings;
using System.IO;

namespace Penguin.WinForms.FileDialogs
{
    public class FileDialogPreset
    {
        public string Filter { get; set; }
        public string Extension { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string InitialDirectory { get; set; } = Directory.GetCurrentDirectory();
        public static FileDialogPreset Json { get; set; } = new FileDialogPreset()
        {
            Filter = DialogFilters.Json,
            Extension = ".json"
        };
    }
}
