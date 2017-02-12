namespace MachtAuthenticate.Localization.Wpf
{
    public class EnumResourceKeyProviderExtension : ResourceKeyProviderExtension<ResourceKeyProvider>
    {
        public EnumResourceKeyProviderExtension()
        {
            ResKeyProvider.BaseKey = Common.Res.ResKeyProvider.EnumPrefix;
        }
    }
}
