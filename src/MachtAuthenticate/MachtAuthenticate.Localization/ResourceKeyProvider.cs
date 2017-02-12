using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MachtAuthenticate.Localization
{
    public class ResourceKeyProvider : IResourceKeyProvider
    {
        private const string NoKey = "<NoKey>";
        public const string EnumPrefix = "Entity";

        public ResourceKeyProvider()
        {
            BaseKey = NoKey;
        }

        public ResourceKeyProvider(string baseKey)
        {
            BaseKey = baseKey ?? NoKey;
        }

        public string BaseKey { get; set; }

        public string ProvideKey(ICollection<object> parameters)
        {
            return GetKey(BaseKey, parameters);
        }

        public static string GetKey(string prefix, ICollection<object> parameters)
        {
            if (parameters == null || !parameters.Any()) return prefix;
            var stringBuilder = new StringBuilder(prefix);
            foreach (var parameter in parameters)
            {
                if (parameter is CultureInfo) continue;
                if (parameter != null)
                    stringBuilder.AppendFormat("_{0}_{1}", parameter.GetType()
                        .Name, parameter);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        ///     Get resource key for enum value
        ///     Makes up a key as: "Enum_EnumType_EnumValue"
        /// </summary>
        public static string GetEnumKey(Enum enumValue)
        {
            return GetKey(EnumPrefix, new object[] {enumValue});
        }
    }
}