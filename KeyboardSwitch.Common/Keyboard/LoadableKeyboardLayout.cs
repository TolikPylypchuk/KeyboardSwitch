namespace KeyboardSwitch.Common.Keyboard
{
    public sealed class LoadableKeyboardLayout
    {
        public LoadableKeyboardLayout(string tag, string name)
        {
            this.Tag = tag;
            this.Name = name;
        }

        public string Tag { get; }
        public string Name { get; }
    }
}
