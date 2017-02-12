namespace MachtAuthenticate.Localization.Wpf
{
    public class ResourceKeyPart : ResourceParameterBase
    {
        public string Key { get; set; }

        public ResourceKeyPart()
            : this(null)
        {
        }

        public ResourceKeyPart(string path)
            : base(path)
        {
        }
    }
}
