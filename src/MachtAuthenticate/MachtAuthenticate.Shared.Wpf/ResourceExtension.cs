using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml.Linq;
using MachtAuthenticate.Shared.Extensions;

namespace MachtAuthenticate.Localization.Wpf
{
    [MarkupExtensionReturnType(typeof(object))]
    [ContentProperty("Parameters")]
    public class ResourceExtension : ManagedMarkupExtension, IServiceProvider
    {
        public static readonly DependencyProperty ResContextProperty =
            DependencyProperty.RegisterAttached(
                "ResContext",
                typeof(ResourceParametersList),
                typeof(ResourceExtension),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        private readonly ResourceParametersList _parameters = new ResourceParametersList();
        private WeakReference _innerElement;
        private object _innerProperty;
        private string _key;
        private ResourceKeyPart _keyPart1;
        private IResourceKeyProvider _keyProvider;

        public ResourceExtension()
        {
        }

        public ResourceExtension(string key)
            : this()
        {
            Key = key;
        }

        public ResourceExtension(string key, string defaultValue)
            : this(key)
        {
            if (!string.IsNullOrEmpty(defaultValue))
                DefaultValue = defaultValue;
        }

        /// <summary>
        ///     Collection of <see cref="ResourceKeyPart" />s and <see cref="ResourceParameter" />s.
        ///     In one collection for shortening in XAML.
        ///     It's supposed there rarely will be mix of <see cref="ResourceKeyPart" /> and <see cref="ResourceParameter" />
        ///     so we can put it to one collection and use reduced XAML
        /// </summary>
        public ResourceParametersList Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters.Clear();
                foreach (var item in value.OfType<ResourceParameterBase>())
                    _parameters.Add(item);
            }
        }

        public IResourceKeyProvider KeyProvider
        {
            get { return _keyProvider ?? CreateDefaultKeyProvider(); }
            set { _keyProvider = value; }
        }

        public string Key
        {
            get { return KeyProvider.ProvideKey(null); }
            set { _key = value; }
        }

        public string DefaultValue { get; set; }

        /// <summary>
        ///     Synthetic property that makes XAML more short and simple for common cases,
        ///     when Key has only one key part
        ///     Binding from this property will be set as first parameters in <see cref="Parameters" /> property
        /// </summary>
        public ResourceKeyPart KeyPart1
        {
            get { return _keyPart1; }
            set
            {
                if (_keyPart1 == value) return;
                if (value != null && Parameters.Count > 0 && Parameters[0] == value)
                    Parameters.RemoveAt(0);
                _keyPart1 = value;
                Parameters.Insert(0, _keyPart1);
            }
        }

        public object GetService(Type serviceType)
        {
            return serviceType == typeof(IProvideValueTarget) && _innerElement.Target != null && _innerElement.IsAlive
                ? new ProvideValueTarget(_innerElement.Target, _innerProperty)
                : null;
        }

        protected virtual IResourceKeyProvider CreateDefaultKeyProvider()
        {
            return new ResourceKeyProvider(_key);
        }

        /// <summary>
        ///     Returns default value for <paramref name="key" />.
        ///     This method is called when no resources was found for <paramref name="key" />
        ///     For "Some_Key" returns "#Some_Key"
        /// </summary>
        public object GetDefaultValue(string key)
        {
            object result = DefaultValue;
            if (TargetProperty == null) return result;
            var targetType = TargetPropertyType;
            if (DefaultValue == null)
            {
                if (targetType == typeof(string) || targetType == typeof(object))
                    result = $"#{key}";
            }
            else
            {
                if (targetType != typeof(string) && targetType != typeof(object))
                {
                    var converter = TypeDescriptor.GetConverter(targetType);
                    result = converter.ConvertFromInvariantString(DefaultValue);
                }
            }
            return result;
        }

        private static void EnsureAppResourcesLoaded()
        {
            var resourceDescriptionAssembly = Assembly.GetExecutingAssembly();
            var resourcesReferencesStream =
                resourceDescriptionAssembly.GetManifestResourceStream("VI.Common.Wpf.Res.resources.xml");

            if (resourcesReferencesStream == null)
                return;

            XElement rootItem;
            using (TextReader streamReader = new StreamReader(resourcesReferencesStream))
            {
                var doc = XDocument.Load(streamReader);
                rootItem = doc.Nodes().OfType<XElement>().FirstOrDefault();
            }

            if (rootItem == null) return;

            foreach (var resourceElement in rootItem.Elements())
                try
                {
                    var assemblyNameAttribute = resourceElement.Attribute("AssemblyName");
                    var resourceRegistratorNameAttribute = resourceElement.Attribute("ResourceRegistratorName");
                    var resourceRegistratorMethodNameAttribute =
                        resourceElement.Attribute("ResourceRegistratorMethodName");
                    if (assemblyNameAttribute != null && resourceRegistratorNameAttribute != null &&
                        resourceRegistratorMethodNameAttribute != null)
                    {
                        var asmName = assemblyNameAttribute.Value;
                        {
                            var typeName = resourceRegistratorNameAttribute.Value;
                            {
                                var methodName = resourceRegistratorMethodNameAttribute.Value;

                                var assembly =
                                    AppDomain.CurrentDomain.GetAssemblies()
                                        .FirstOrDefault(a => a.FullName.Contains(asmName));

                                if (assembly != null)
                                {
                                    var resourceRegistrator = assembly.GetType(typeName);
                                    var method = resourceRegistrator.GetMethod(methodName);
                                    method.Invoke(null, null);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"error = {ex.Message}");
                }
        }

        /// <summary>
        ///     Our goal dynamically react to current culture and parameters changings.
        ///     So we can't just return localized object. We create binding which
        ///     via ProvideValue() returns BindingExpression.
        ///     This BindingExpression provides dynamic refresh when current culture or parameters are changing.
        ///     In spite of it's name the method returns BindingExpression, not localized value
        /// </summary>
        protected override object GetValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key)) throw new ArgumentException("Key cannot be null");

            var targetHelper = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (targetHelper == null)
                throw new NullReferenceException(nameof(targetHelper));

            if (!(targetHelper.TargetProperty is DependencyProperty))
                return this;

            var targetObject = targetHelper.TargetObject as DependencyObject;
            if (targetObject != null && DesignerProperties.GetIsInDesignMode(targetObject))
                EnsureAppResourcesLoaded();

            ResourceParametersList paramList = null;
            if (targetObject != null && Parameters.Count == 0)
                paramList = GetResContext(targetObject);

            if (paramList == null)
                paramList = Parameters;

            var converter = new ResourceConverter
            {
                KeyProvider = KeyProvider,
                ResourceExtension = this,
                Parameters = paramList
            };

            var binding = new Binding("UICulture")
            {
                Source = CultureManager.Instance,
                Mode = BindingMode.OneWay
            };

            // For simple key just watch for culture changing
            if (paramList.Count == 0)
            {
                binding.Converter = converter;
                return binding.ProvideValue(serviceProvider);
            }

            // For composite key or formatted string watch for culture, key parts or parameters changing
            var multiBinding = new MultiBinding
            {
                Converter = converter,
                Mode = BindingMode.OneWay
            };

            foreach (var param in paramList)
                multiBinding.Bindings.Add(param);

            // Converter waits the culter as last parameter
            multiBinding.Bindings.Add(binding);

            return multiBinding.ProvideValue(serviceProvider);
        }

        public static void BindExtension(DependencyObject element, DependencyProperty property, string key)
        {
            BindExtension(element, property, key, null);
        }

        public static void BindExtension(DependencyObject element, DependencyProperty property, string key,
            IEnumerable<ResourceParameterBase> resourceParametersBase)
        {
            if (element == null || property == null || key == null) return;
            var res = new ResourceExtension(key, string.Empty)
            {
                _innerElement = new WeakReference(element),
                _innerProperty = property
            };

            if (resourceParametersBase != null)
                res.Parameters.AddRange(resourceParametersBase);

            element.SetValue(property, res.ProvideValue(res));
        }

        /// <summary>
        ///     Returns localized string taking parameters from object properties
        /// </summary>
        /// <param name="source">Object which property values will be taken as parameters for formatted string</param>
        /// <param name="key">Resource key</param>
        /// <param name="formatPropertyNames">From these properties method will take values and put them to formatted string </param>
        /// <returns>Localized formatted string</returns>
        public static string GetFormattedStringValue(object source, string key, IEnumerable<string> formatPropertyNames)
        {
            var valueFreezable = new ValueFreezable();

            BindExtension(valueFreezable, ValueFreezable.ValueProperty, key,
                formatPropertyNames.Select(s => new ResourceParameter(s) {Source = source, Mode = BindingMode.OneWay}));

            return valueFreezable.Value as string;
        }

        public static ResourceParametersList GetResContext(DependencyObject obj)
        {
            return (ResourceParametersList) obj.GetValue(ResContextProperty);
        }

        public static void SetResContext(DependencyObject obj, ResourceParametersList value)
        {
            obj.SetValue(ResContextProperty, value);
        }

        public class ProvideValueTarget : IProvideValueTarget
        {
            public ProvideValueTarget(object element, object property)
            {
                TargetObject = element;
                TargetProperty = property;
            }

            public object TargetObject { get; }
            public object TargetProperty { get; }
        }
    }
}