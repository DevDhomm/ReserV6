using System.Collections.ObjectModel;
using Wpf.Ui.Controls;

namespace ReserV6.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "WPF UI - ReserV6";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Reservations",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Calendar24 },
                TargetPageType = typeof(Views.Pages.ReservationsPage)
            },
            new NavigationViewItem()
            {
                Content = "Rooms",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Building24 },
                TargetPageType = typeof(Views.Pages.RoomsPage)
            },
            new NavigationViewItem()
            {
                Content = "Gestion Salles",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                TargetPageType = typeof(Views.Pages.SallesGestionPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
