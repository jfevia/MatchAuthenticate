using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MachtAuthenticate.Localization.Engine;
using MachtAuthenticate.Localization.TypeConverters;
using XAMLMarkupExtensions.Base;

namespace MachtAuthenticate.Localization.Extensions
{
    /// <summary>
    ///     A localization utility based on <see cref="FrameworkElement" />.
    /// </summary>
    public class FeLoc : FrameworkElement, IDictionaryEventListener, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        ///     This method is called when the resource somehow changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        public void ResourceChanged(DependencyObject sender, DictionaryEventArgs e)
        {
            UpdateNewValue();
        }

        /// <summary>
        ///     Indicates, that the key changed.
        /// </summary>
        /// <param name="obj">The FELoc object.</param>
        /// <param name="args">The event argument.</param>
        private static void DependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var loc = obj as FeLoc;

            loc?.UpdateNewValue();
        }

        #region Forced culture handling

        /// <summary>
        ///     If Culture property defines a valid <see cref="CultureInfo" />, a <see cref="CultureInfo" /> instance will get
        ///     created and returned, otherwise <see cref="LocalizeDictionary" />.Culture will get returned.
        /// </summary>
        /// <returns>The <see cref="CultureInfo" /></returns>
        /// <exception cref="System.ArgumentException">
        ///     thrown if the parameter Culture don't defines a valid <see cref="CultureInfo" />
        /// </exception>
        protected CultureInfo GetForcedCultureOrDefault()
        {
            // define a culture info
            CultureInfo cultureInfo;

            // check if the forced culture is not null or empty
            if (!string.IsNullOrEmpty(ForceCulture))
                try
                {
                    // try to create a specific culture from the forced one
                    // cultureInfo = CultureInfo.CreateSpecificCulture(this.ForceCulture);
                    cultureInfo = new CultureInfo(ForceCulture);
                }
                catch (ArgumentException ex)
                {
                    // on error, check if designmode is on
                    if (LocalizeDictionary.Instance.GetIsInDesignMode())
                        cultureInfo = LocalizeDictionary.Instance.SpecificCulture;
                    else
                        throw new ArgumentException("Cannot create a CultureInfo with '" + ForceCulture + "'", ex);
                }
            else
                cultureInfo = LocalizeDictionary.Instance.SpecificCulture;

            // return the evaluated culture info
            return cultureInfo;
        }

        #endregion

        private void UpdateNewValue()
        {
            Content = FormatOutput();
        }

        #region Resource loopkup

        /// <summary>
        ///     This function returns the properly prepared output of the markup extension.
        /// </summary>
        public object FormatOutput()
        {
            object result = null;

            if (_targetInfo == null)
                return null;

            var targetObject = _targetInfo.TargetObject as DependencyObject;

            // Get target type. Change ImageSource to BitmapSource in order to use our own converter.
            var targetType = _targetInfo.TargetPropertyType;

            if (targetType == typeof(ImageSource))
                targetType = typeof(BitmapSource);

            // Try to get the localized input from the resource.
            string resourceKey = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(Key, targetObject);

            var ci = GetForcedCultureOrDefault();

            // Extract the names of the endpoint object and property
            var epName = "";
            var epProp = "";

            var o = targetObject as FrameworkElement;
            if (o != null)
                epName = o.GetValueSync<string>(NameProperty);
            else if (targetObject is FrameworkContentElement)
                epName = ((FrameworkContentElement) targetObject).GetValueSync<string>(FrameworkContentElement.NameProperty);

            var info = _targetInfo.TargetProperty as PropertyInfo;
            if (info != null)
                epProp = info.Name;
            else if (_targetInfo.TargetProperty is DependencyProperty)
                epProp = ((DependencyProperty) _targetInfo.TargetProperty).Name;

            // What are these names during design time good for? Any suggestions?
            if (epProp.Contains("FrameworkElementWidth5"))
                epProp = "Height";
            else if (epProp.Contains("FrameworkElementWidth6"))
                epProp = "Width";
            else if (epProp.Contains("FrameworkElementMargin12"))
                epProp = "Margin";

            var resKeyBase = ci.Name + ":" + targetType.Name + ":";
            string resKeyNameProp = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName + LocalizeDictionary.GetSeparation(targetObject) + epProp, targetObject);
            string resKeyName = LocalizeDictionary.Instance.GetFullyQualifiedResourceKey(epName, targetObject);

            // Check, if the key is already in our resource buffer.
            object input = null;

            if (!string.IsNullOrEmpty(resourceKey))
            {
                // We've got a resource key. Try to look it up or get it from the dictionary.
                lock (ResourceBufferLock)
                {
                    if (_resourceBuffer.ContainsKey(resKeyBase + resourceKey))
                    {
                        result = _resourceBuffer[resKeyBase + resourceKey];
                    }
                    else
                    {
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resourceKey, targetObject, ci);
                        resKeyBase += resourceKey;
                    }
                }
            }
            else
            {
                // Try the automatic lookup function.
                // First, look for a resource entry named: [FrameworkElement name][Separator][Property name]
                lock (ResourceBufferLock)
                {
                    if (_resourceBuffer.ContainsKey(resKeyBase + resKeyNameProp))
                    {
                        result = _resourceBuffer[resKeyBase + resKeyNameProp];
                    }
                    else
                    {
                        // It was not stored in the buffer - try to retrieve it from the dictionary.
                        input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyNameProp, targetObject, ci);

                        if (input == null)
                            if (_resourceBuffer.ContainsKey(resKeyBase + resKeyName))
                            {
                                result = _resourceBuffer[resKeyBase + resKeyName];
                            }
                            else
                            {
                                input = LocalizeDictionary.Instance.GetLocalizedObject(resKeyName, targetObject, ci);
                                resKeyBase += resKeyName;
                            }
                        else
                            resKeyBase += resKeyNameProp;
                    }
                }
            }

            // If no result was found, convert the input and add it to the buffer.
            if (result == null && input != null)
            {
                result = Converter.Convert(input, targetType, ConverterParameter, ci);
                SafeAddItemToResourceBuffer(resKeyBase, result);
            }

            return result;
        }

        #endregion

        #region PropertyChanged Logic

        /// <summary>
        ///     Informiert über sich ändernde Eigenschaften.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Notify that a property has changed
        /// </summary>
        /// <param name="property">
        ///     The property that changed
        /// </param>
        internal void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #region Private variables

        private static readonly object ResourceBufferLock = new object();
        private static Dictionary<string, object> _resourceBuffer = new Dictionary<string, object>();

        private ParentChangedNotifier _parentChangedNotifier;
        private TargetInfo _targetInfo;

        #endregion

        #region Resource buffer handling.

        /// <summary>
        ///     Clears the common resource buffer.
        /// </summary>
        public static void ClearResourceBuffer()
        {
            lock (ResourceBufferLock)
            {
                _resourceBuffer?.Clear();
            }

            lock (ResourceBufferLock)
            {
                _resourceBuffer = null;
            }
        }


        /// <summary>
        ///     Adds an item to the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        internal static void SafeAddItemToResourceBuffer(string key, object item)
        {
            lock (ResourceBufferLock)
            {
                if (!LocalizeDictionary.Instance.DisableCache && !_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Add(key, item);
            }
        }

        /// <summary>
        ///     Removes an item from the resource buffer (threadsafe).
        /// </summary>
        /// <param name="key">The key.</param>
        internal static void SafeRemoveItemFromResourceBuffer(string key)
        {
            lock (ResourceBufferLock)
            {
                if (_resourceBuffer.ContainsKey(key))
                    _resourceBuffer.Remove(key);
            }
        }

        #endregion

        #region DependencyProperty: Key

        /// <summary>
        ///     <see cref="DependencyProperty" /> Key to set the resource key.
        /// </summary>
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register(
                "Key",
                typeof(string),
                typeof(FeLoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        ///     The resource key.
        /// </summary>
        public string Key
        {
            get { return this.GetValueSync<string>(KeyProperty); }
            set { this.SetValueSync(KeyProperty, value); }
        }

        #endregion

        #region DependencyProperty: Converter

        /// <summary>
        ///     <see cref="DependencyProperty" /> Converter to set the <see cref="IValueConverter" /> used to adapt to the target.
        /// </summary>
        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register(
                "Converter",
                typeof(IValueConverter),
                typeof(FeLoc),
                new PropertyMetadata(new DefaultConverter(), DependencyPropertyChanged));

        /// <summary>
        ///     Gets or sets the custom value converter.
        /// </summary>
        public IValueConverter Converter
        {
            get { return this.GetValueSync<IValueConverter>(ConverterProperty); }
            set { this.SetValueSync(ConverterProperty, value); }
        }

        #endregion

        #region DependencyProperty: ConverterParameter

        /// <summary>
        ///     <see cref="DependencyProperty" /> ConverterParameter.
        /// </summary>
        public static readonly DependencyProperty ConverterParameterProperty =
            DependencyProperty.Register(
                "ConverterParameter",
                typeof(object),
                typeof(FeLoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        ///     Gets or sets the converter parameter.
        /// </summary>
        public object ConverterParameter
        {
            get { return this.GetValueSync<object>(ConverterParameterProperty); }
            set { this.SetValueSync(ConverterParameterProperty, value); }
        }

        #endregion

        #region DependencyProperty: ForceCulture

        /// <summary>
        ///     <see cref="DependencyProperty" /> ForceCulture.
        /// </summary>
        public static readonly DependencyProperty ForceCultureProperty =
            DependencyProperty.Register(
                "ForceCulture",
                typeof(string),
                typeof(FeLoc),
                new PropertyMetadata(null, DependencyPropertyChanged));

        /// <summary>
        ///     Gets or sets the forced culture.
        /// </summary>
        public string ForceCulture
        {
            get { return this.GetValueSync<string>(ForceCultureProperty); }
            set { this.SetValueSync(ForceCultureProperty, value); }
        }

        #endregion

        #region DependencyProperty: Content - used for value transfer only!

        ///// <summary>
        ///// <see cref="DependencyProperty"/> ForceCulture.
        ///// </summary>
        //public static readonly DependencyProperty ContentProperty =
        //        DependencyProperty.Register(
        //        "Content",
        //        typeof(object),
        //        typeof(FELoc));

        ///// <summary>
        ///// Gets or sets the content.
        ///// </summary>
        //public object Content
        //{
        //    get { return GetValue(ContentProperty); }
        //    set { SetValue(ContentProperty, value); }
        //}

        private object _content;

        /// <summary>
        ///     Gets or sets the content.
        /// </summary>
        public object Content
        {
            get { return _content; }
            set
            {
                _content = value;
                RaisePropertyChanged("Content");
            }
        }

        #endregion

        #region Parent changed event

        /// <summary>
        ///     Based on http://social.msdn.microsoft.com/Forums/en/wpf/thread/580234cb-e870-4af1-9a91-3e3ba118c89c
        /// </summary>
        /// <param name="element">The target object.</param>
        /// <returns>The list of DependencyProperties of the object.</returns>
        private List<DependencyProperty> GetDependencyProperties(object element)
        {
            var properties = new List<DependencyProperty>();
            var markupObject = MarkupWriter.GetMarkupObjectFor(element);

            foreach (var mp in markupObject.Properties)
                if (mp.DependencyProperty != null)
                    properties.Add(mp.DependencyProperty);

            return properties;
        }

        private void RegisterParentNotifier()
        {
            _parentChangedNotifier = new ParentChangedNotifier(this, () =>
            {
                _parentChangedNotifier.Dispose();
                _parentChangedNotifier = null;
                var targetObject = Parent;
                if (targetObject != null)
                {
                    var properties = GetDependencyProperties(targetObject);
                    foreach (var p in properties)
                        if (Equals(targetObject.GetValue(p), this))
                        {
                            _targetInfo = new TargetInfo(targetObject, p, p.PropertyType, -1);

                            var binding = new Binding("Content");
                            binding.Source = this;
                            binding.Converter = Converter;
                            binding.ConverterParameter = ConverterParameter;
                            binding.Mode = BindingMode.OneWay;
                            BindingOperations.SetBinding(targetObject, p, binding);
                            UpdateNewValue();
                        }
                }
            });
        }

        #endregion

        #region Constructors & Dispose

        /// <summary>
        ///     Initializes a new instance of the <see cref="BLoc" /> class.
        /// </summary>
        public FeLoc()
        {
            LocalizeDictionary.DictionaryEvent.AddListener(this);
            RegisterParentNotifier();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BLoc" /> class.
        /// </summary>
        /// <param name="key">The resource identifier.</param>
        public FeLoc(string key)
            : this()
        {
            Key = key;
        }

        /// <summary>
        ///     Removes the listener from the dictionary.
        /// </summary>
        public void Dispose()
        {
            LocalizeDictionary.DictionaryEvent.RemoveListener(this);
        }

        /// <summary>
        ///     The finalizer.
        /// </summary>
        ~FeLoc()
        {
            Dispose();
        }

        #endregion
    }
}