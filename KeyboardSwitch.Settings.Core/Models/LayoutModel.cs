using System;
using System.Collections.Generic;

namespace KeyboardSwitch.Settings.Core.Models
{
    public sealed class LayoutModel
    {
        public string LanguageName { get; set; } = String.Empty;
        public string KeyboardName { get; set; } = String.Empty;
        public int Index { get; set; }
        public int Id { get; set; }

        public List<CharacterModel> Chars { get; set; } = new List<CharacterModel>();
    }
}
