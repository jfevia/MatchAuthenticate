using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MachtAuthenticate.Localization
{
    public class ResourceManager : IResourceManager
    {
        private readonly List<IResourceManager> _defaultResManagers = new List<IResourceManager>();

        private readonly List<IResourceManager> _resManagers = new List<IResourceManager>();

        /// <summary>
        ///     Register resource manager.
        /// </summary>
        /// <param name="resourceManager">Resource manager.</param>
        public void RegisterResManager(IResourceManager resourceManager)
        {
            if (!_resManagers.Contains(resourceManager))
                _resManagers.Add(resourceManager);
        }

        /// <summary>
        ///     Register default resource manager.
        /// </summary>
        /// <param name="resourceManager">Resource manager that will be default.</param>
        public void RegisterDefaultResManager(IResourceManager resourceManager)
        {
            if (!_defaultResManagers.Contains(resourceManager))
                _defaultResManagers.Add(resourceManager);
        }

        /// <summary>
        ///     Get resource string by <paramref name="enumValue" />
        /// </summary>
        public string GetEnumResource(Enum enumValue)
        {
            return GetResourceString(ResourceKeyProvider.GetEnumKey(enumValue));
        }

        #region Singleton implementation

        // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        static ResourceManager()
        {
        }

        private ResourceManager()
        {
        }

        public static readonly ResourceManager Instance = new ResourceManager();

        #endregion

        #region IResourceManager Members

        /// <summary>
        ///     Get resource managers that can find resource by <paramref name="resourceKey" />
        /// </summary>
        private IEnumerable<IResourceManager> GetResManagers(string resourceKey)
        {
            return _resManagers
                .Where(rm => rm.Match(resourceKey))
                .Union(_defaultResManagers);
        }

        /// <summary>
        ///     Get resource string by <paramref name="resourceKey" />
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        /// <returns>
        ///     Returns null if resource was not found
        /// </returns>
        public string GetResourceString(string resourceKey)
        {
            return GetResourceString(resourceKey, null);
        }

        /// <summary>
        ///     Get resource string by <paramref name="resourceKey" /> and <paramref name="cultureInfo" />
        /// </summary>
        /// <param name="resourceKey">Resource key.</param>
        /// <param name="cultureInfo"></param>
        /// <returns>
        ///     Returns null if resource was not found
        /// </returns>
        public string GetResourceString(string resourceKey, CultureInfo cultureInfo)
        {
            return GetResManagers(resourceKey)
                .Select(rm => rm.GetResourceString(resourceKey, cultureInfo))
                .FirstOrDefault(res => res != null);
        }

        /// <summary>
        ///     Returns resource string for composite key
        /// </summary>
        /// <param name="prefix">Composite key prefix</param>
        /// <param name="parameters">Composite key parts</param>
        public static string GetResourceStringForCompositeKey(string prefix, params object[] parameters)
        {
            var resultKey = ResourceKeyProvider.GetKey(prefix, parameters);
            return Instance.GetResourceString(resultKey);
        }

        /// <summary>
        ///     Ïîëó÷èòü ðåñóðñ ïî åãî êëþ÷ó.
        /// </summary>
        /// <param name="resourceKey">Êëþ÷ ðåñóðñà.</param>
        /// <returns>Ðåñóðñ.</returns>
        public object GetResourceObject(string resourceKey)
        {
            return GetResourceObject(resourceKey, null);
        }

        /// <summary>
        ///     Get resource by <paramref name="resourceKey" /> and <paramref name="cultureInfo" />.
        /// </summary>
        public object GetResourceObject(string resourceKey, CultureInfo cultureInfo)
        {
            return GetResManagers(resourceKey)
                .Select(rm => rm.GetResourceObject(resourceKey, cultureInfo))
                .FirstOrDefault(res => res != null);
        }

        public bool Match(string resourceKey)
        {
            var managers = GetResManagers(resourceKey);
            return managers != null && managers.Any();
        }

        #endregion
    }
}