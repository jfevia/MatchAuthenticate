using System;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace MachtAuthenticate.Shared.Wpf
{
    public abstract class ManagedMarkupExtension : MarkupExtension
    {
        private object _targetProperty;

        protected object TargetProperty
        {
            get { return _targetProperty as DependencyProperty; }
        }

        protected Type TargetPropertyType
        {
            get
            {
                Type propertyType = null;
                var property = _targetProperty as DependencyProperty;
                if (property != null)
                    propertyType = property.PropertyType;
                else if (_targetProperty is PropertyInfo)
                    propertyType = ((PropertyInfo) _targetProperty).PropertyType;
                return propertyType;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var targetHelper = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (targetHelper?.TargetObject == null) return null;
            _targetProperty = targetHelper.TargetProperty;
            if (targetHelper.TargetObject is DependencyObject || !(_targetProperty is DependencyProperty))
                return GetValue(serviceProvider);
            // the extension is being used in a template
            return this;
        }

        protected abstract object GetValue(IServiceProvider serviceProvider);
    }
}