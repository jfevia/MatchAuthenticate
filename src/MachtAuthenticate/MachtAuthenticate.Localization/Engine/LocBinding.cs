using System.Windows;
using System.Windows.Data;
using MachtAuthenticate.Localization.Extensions;

namespace MachtAuthenticate.Localization.Engine
{
    /// <summary>
    ///     A binding proxy class that accepts bindings and forwards them to the LocalizationResourceExtension.
    ///     Based on: http://www.codeproject.com/Articles/71348/Binding-on-a-Property-which-is-not-a-DependencyPro
    /// </summary>
    public class LocBinding : FrameworkElement
    {
        #region OnPropertyChanged

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var locBinding = obj as LocBinding;

            if (locBinding != null && args.Property == SourceProperty)
                if (!ReferenceEquals(locBinding.Source, locBinding._target) && locBinding._target != null && locBinding.Source != null)
                    locBinding._target.Key = locBinding.Source.ToString();
        }

        #endregion

        #region Source DP

        /// <summary>
        ///     We don't know what will be the Source/target type so we keep 'object'.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(LocBinding),
                new FrameworkPropertyMetadata(OnPropertyChanged)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        /// <summary>
        ///     The source.
        /// </summary>
        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion

        #region Target LocalizationResourceExtension

        private LocalizationResourceExtension _target;

        /// <summary>
        ///     The target extension.
        /// </summary>
        public LocalizationResourceExtension Target
        {
            get { return _target; }
            set
            {
                _target = value;
                if (_target != null && Source != null)
                    _target.Key = Source.ToString();
            }
        }

        #endregion
    }
}