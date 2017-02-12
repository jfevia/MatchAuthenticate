using System.Globalization;
using System.Reflection;

namespace MachtAuthenticate.Localization
{
    public class FileResourceManager : IResourceManager
    {
        private readonly string _prefix;
        private readonly System.Resources.ResourceManager _resourceManager;

        /// <summary>
        ///     Constructor for resource manager which keys has no prefix.
        /// </summary>
        /// <param name="resourceFileName">
        ///     The root name of the resource file
        ///     without its extension but including any fully qualified namespace name.
        ///     For example, the root name for the resource file named
        ///     MyApplication.MyResource.en-US.resources is MyApplication.MyResource.
        /// </param>
        /// <param name="assembly">The main assembly for the resources. </param>
        public FileResourceManager(string resourceFileName, Assembly assembly)
            : this(string.Empty, resourceFileName, assembly)
        {
        }

        /// <summary>
        ///     Constructor for resource manager which keys has no prefix.
        /// </summary>
        /// <param name="resourceKeyPrefix">Prefix for all keys in this file.</param>
        /// <param name="resourceFileName">
        ///     The root name of the resource file
        ///     without its extension but including any fully qualified namespace name.
        ///     For example, the root name for the resource file named
        ///     MyApplication.MyResource.en-US.resources is MyApplication.MyResource.
        /// </param>
        /// <param name="assembly">The main assembly for the resources. </param>
        public FileResourceManager(string resourceKeyPrefix, string resourceFileName, Assembly assembly)
        {
            _prefix = resourceKeyPrefix;
            _resourceManager = new System.Resources.ResourceManager(resourceFileName, assembly);
        }

        private System.Resources.ResourceManager FindResourceManager(string resourceKey)
        {
            return !string.IsNullOrEmpty(resourceKey) && resourceKey.StartsWith(_prefix) ? _resourceManager : null;
        }

        #region IResourceManager Members

        /// <summary>
        ///     Returns true if resource manager contains resource for <paramref name="resourceKey" />.
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        public bool Match(string resourceKey)
        {
            return FindResourceManager(resourceKey) != null;
        }

        /// <summary>
        ///     Returns current <see cref="CultureInfo" />
        ///     TODO: For WPF application it's enough to return null,
        ///     but for server messages for client we need
        ///     to return client's <see cref="CultureInfo" />
        /// </summary>
        private static CultureInfo GetCultureInfo()
        {
            return null;
        }

        /// <summary>
        ///     Get string from resources by key depending on current culture.
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        /// <returns>String from resources.</returns>
        public string GetResourceString(string resourceKey)
        {
            return GetResourceString(resourceKey, null);
        }

        /// <summary>
        ///     Get string from resources by key depending on given culture.
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        /// <param name="cultureInfo"></param>
        /// <returns>String from resources.</returns>
        public string GetResourceString(string resourceKey, CultureInfo cultureInfo)
        {
            cultureInfo = cultureInfo ?? GetCultureInfo();
            var resManager = FindResourceManager(resourceKey);
            return resManager != null ? resManager.GetString(resourceKey, cultureInfo) : string.Empty;
        }

        /// <summary>
        ///     Get object from resources by key depending on current culture.
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        /// <returns>Object from resources.</returns>
        public object GetResourceObject(string resourceKey)
        {
            return GetResourceObject(resourceKey, null);
        }

        /// <summary>
        ///     Get object from resources by key depending on given culture.
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        /// <param name="cultureInfo"></param>
        /// <returns>Object from resources.</returns>
        public object GetResourceObject(string resourceKey, CultureInfo cultureInfo)
        {
            cultureInfo = cultureInfo ?? GetCultureInfo();
            var resManager = FindResourceManager(resourceKey);
            return resManager != null ? resManager.GetObject(resourceKey, cultureInfo) : null;
        }

        #endregion
    }
}