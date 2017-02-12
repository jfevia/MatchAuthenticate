using System.Collections.Generic;
using System.Reflection;

namespace MachtAuthenticate.Localization
{
    public abstract class ResourceRegistrator
    {
        public virtual void Initialize()
        {
            // We use singleton ResourceManager.Instance for simplicity
            // In real app its better to do this via DI, IoC containers
            RegisterResources(ResourceManager.Instance);
        }

        public virtual void RegisterResources(ResourceManager resourceManager)
        {
            foreach (var manager in GetResManagers())
                resourceManager.RegisterResManager(manager);
        }

        protected abstract IEnumerable<IResourceManager> GetResManagers();
        protected abstract string GetResourceFileName();
        protected abstract Assembly GetExecutingAssembly();

        protected virtual IResourceManager CreateResourceManager(string resPrefix, string namespaceSuffix)
        {
            var executingAssembly = GetExecutingAssembly();
            return new FileResourceManager(resPrefix + "_", GetResourceFileName() + "." + namespaceSuffix, executingAssembly);
        }

        protected virtual IResourceManager CreateResourceManager(string resPrefix)
        {
            return CreateResourceManager(resPrefix, resPrefix);
        }
    }
}