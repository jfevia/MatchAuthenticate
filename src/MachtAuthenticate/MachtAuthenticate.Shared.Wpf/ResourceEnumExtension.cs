namespace MachtAuthenticate.Localization.Wpf
{
    public class ResourceEnumExtension : ResourceExtension
    {
        protected override IResourceKeyProvider CreateDefaultKeyProvider()
        {
            return new ResourceKeyProvider(ResourceKeyProvider.EnumPrefix);
        }
    }
}
