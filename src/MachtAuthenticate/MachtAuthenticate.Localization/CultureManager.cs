using System.Globalization;
using System.Threading;
using GalaSoft.MvvmLight;

namespace MachtAuthenticate.Localization
{
    public class CultureManager : ObservableObject
    {
        public static readonly CultureManager Instance = new CultureManager();
        private bool _synchronizeThreadCulture = true;
        private CultureInfo _uiCulture = Thread.CurrentThread.CurrentUICulture;

        static CultureManager()
        {
        }

        private CultureManager()
        {
        }

        public CultureInfo UiCulture
        {
            get { return _uiCulture; }
            set
            {
                Thread.CurrentThread.CurrentUICulture = value;

                if (SynchronizeThreadCulture)
                    SetThreadCulture(value);

                Set(nameof(UiCulture), ref _uiCulture, value);
            }
        }

        public bool SynchronizeThreadCulture
        {
            get { return _synchronizeThreadCulture; }
            set
            {
                if (value)
                    SetThreadCulture(UiCulture);

                Set(nameof(SynchronizeThreadCulture), ref _synchronizeThreadCulture, value);
            }
        }

        private static void SetThreadCulture(CultureInfo value)
        {
            Thread.CurrentThread.CurrentCulture = value.IsNeutralCulture ? CultureInfo.CreateSpecificCulture(value.Name) : value;
        }
    }
}