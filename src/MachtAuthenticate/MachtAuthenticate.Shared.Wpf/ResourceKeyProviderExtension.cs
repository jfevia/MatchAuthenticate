using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace MachtAuthenticate.Localization.Wpf
{
    public class ResourceKeyProviderExtension<T> : MarkupExtension, IResourceKeyProvider where T : class, IResourceKeyProvider, new()
    {
        protected internal readonly T ResKeyProvider = new T();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public string ProvideKey(ICollection<object> parameters)
        {
            return ResKeyProvider.ProvideKey(parameters);
        }
    }
}
