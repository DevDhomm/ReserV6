using ReserV6.ViewModels.Pages;
using ReserV6.Models;
using Wpf.Ui.Abstractions.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ReserV6.Views.Pages
{
    public partial class ReservationsPage : INavigableView<ReservationsViewModel>
    {
        public ReservationsViewModel ViewModel { get; }

        public ReservationsPage(ReservationsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        /// <summary>
        /// Event handler pour le bouton Annuler dans le DataGrid
        /// </summary>
        private async void OnCancelReservationClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ReservationComplete reservation)
            {
                System.Diagnostics.Debug.WriteLine($"OnCancelReservationClick: Reservation {reservation.Id}");
                await ViewModel.CancelReservationCommand.ExecuteAsync(reservation);
            }
        }

        /// <summary>
        /// Event handler pour le bouton Supprimer dans le DataGrid
        /// </summary>
        private async void OnDeleteReservationClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ReservationComplete reservation)
            {
                System.Diagnostics.Debug.WriteLine($"OnDeleteReservationClick: Reservation {reservation.Id}");
                await ViewModel.DeleteReservationCommand.ExecuteAsync(reservation);
            }
        }
    }
}
