namespace KeyboardSwitch.Common
{
    public class KeyboardLayout
    {
        public KeyboardLayout(int id, ushort languageId, ushort keyboardId, string languageName, string keyboardName)
        {
            this.Id = id;
            this.LanguageId = languageId;
            this.KeyboardId = keyboardId;
            this.LanguageName = languageName;
            this.KeyboardName = keyboardName;
        }

        public int Id { get; }

        public ushort LanguageId { get; }
        public ushort KeyboardId { get; }

        public string LanguageName { get; }
        public string KeyboardName { get; }

        public override string ToString()
            => $"{this.LanguageName} - {this.KeyboardName}";
    }
}
