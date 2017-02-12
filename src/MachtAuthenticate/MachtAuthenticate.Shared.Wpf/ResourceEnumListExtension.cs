using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace MachtAuthenticate.Localization.Wpf
{
    public class ResourceEnumListExtension : MarkupExtension
    {
        public ResourceEnumListExtension(Type enumType)
        {
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            EnumType = enumType;
        }

        private Type _enumType;
        public Type EnumType
        {
            get { return _enumType; }
            private set
            {
                if (_enumType == value) return;
                var type = Nullable.GetUnderlyingType(value) ?? value;
                if (type.IsEnum == false) throw new ArgumentException("Type must be an Enum.");
                _enumType = value;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var converter = new ResEnumConverter();
            var binding = new Binding("UICulture")
            {
                Source = CultureManager.Instance,
                Mode = BindingMode.OneWay,
                Converter = converter,
                ConverterParameter = EnumType
            };
            return binding.ProvideValue(serviceProvider);
        }

        #region ResEnumConverter

        private class ResEnumConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                Type enumType = parameter as Type;
                if (enumType == null || !enumType.IsEnum) return null;
                IEnumerable<object> enumValues = Enum
                    .GetValues(enumType)
                    .Cast<Enum>()
                    .Select(enumValue => ResourceManager.Instance.GetEnumResource(enumValue))
                    .ToArray();
                return enumValues;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return Binding.DoNothing;
            }
        }

        #endregion
    }
}
