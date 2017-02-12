using System.Windows;
using System.Windows.Controls.Primitives;

namespace MachtAuthenticate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleButtonAddEntryClick(object sender, RoutedEventArgs e)
        {
            PopupAddEntry.IsOpen = true;
            PopupAddEntry.Closed += (senderClosed, eClosed) => { ToggleButtonAddEntry.IsChecked = false; };
        }

        private void PopupDropDownButtonClick(object sender, RoutedEventArgs e)
        {
            var popup = sender as Popup;
            if (popup != null)
                popup.IsOpen = false;
        }
    }
}