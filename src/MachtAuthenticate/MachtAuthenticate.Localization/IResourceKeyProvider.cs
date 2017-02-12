using System.Collections.Generic;

namespace MachtAuthenticate.Localization
{
    public interface IResourceKeyProvider
    {
        /// <summary>
        ///     Provides key for Resource Managers.
        ///     Key can be composed from <paramref name="parameters" />
        /// </summary>
        string ProvideKey(ICollection<object> parameters);
    }
}