using System;

namespace KeyboardSwitch.Settings.Core.Models
{
    public sealed class LayoutModel
    {
        public string LanguageName { get; set; } = String.Empty;
        public string KeyboardName { get; set; } = String.Empty;
        public int Index { get; set; }
        public int Id { get; set; }
        public bool IsNew { get; set; }

        public string Chars { get; set; } = String.Empty;
    }
}
