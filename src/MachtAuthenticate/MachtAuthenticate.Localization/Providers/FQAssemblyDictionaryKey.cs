using System.Linq;

namespace MachtAuthenticate.Localization.Providers
{
    /// <summary>
    ///     A class that bundles the key, assembly and dictionary information.
    /// </summary>
    public class FqAssemblyDictionaryKey : FullyQualifiedResourceKeyBase
    {
        private readonly string _assembly;

        private readonly string _dictionary;
        private readonly string _key;

        /// <summary>
        ///     Creates a new instance of <see cref="FullyQualifiedResourceKeyBase" />.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="assembly">The assembly of the dictionary.</param>
        /// <param name="dictionary">The resource dictionary.</param>
        public FqAssemblyDictionaryKey(string key, string assembly = null, string dictionary = null)
        {
            this._key = key;
            this._assembly = assembly;
            this._dictionary = dictionary;
        }

        /// <summary>
        ///     The key.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }

        /// <summary>
        ///     The assembly of the dictionary.
        /// </summary>
        public string Assembly
        {
            get { return _assembly; }
        }

        /// <summary>
        ///     The resource dictionary.
        /// </summary>
        public string Dictionary
        {
            get { return _dictionary; }
        }

        /// <summary>
        ///     Converts the object to a string.
        /// </summary>
        /// <returns>The joined version of the assembly, dictionary and key.</returns>
        public override string ToString()
        {
            return string.Join(":", new[] {Assembly, Dictionary, Key}.Where(x => !string.IsNullOrEmpty(x))
                .ToArray());
        }
    }
}