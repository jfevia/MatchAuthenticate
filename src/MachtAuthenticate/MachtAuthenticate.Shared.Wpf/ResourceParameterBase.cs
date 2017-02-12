using System.Windows.Data;

namespace MachtAuthenticate.Localization.Wpf
{
    public abstract class ResourceParameterBase : Binding
    {
        protected ResourceParameterBase()
            : this(null)
        {
        }

        protected ResourceParameterBase(string path)
            : base(path)
        {
            Mode = BindingMode.OneWay;
            FallbackValue = string.Empty;
        }
    }
}
