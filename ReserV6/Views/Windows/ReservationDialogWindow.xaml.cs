using ReserV6.ViewModels.Windows;
using System.Windows.Controls;

namespace ReserV6.Views.Windows
{
    public partial class ReservationDialogWindow : Window
    {
        private ReservationDialogViewModel? _viewModel;

        public ReservationDialogWindow()
        {
            InitializeComponent();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ReservationDialogViewModel viewModel)
            {
                _viewModel = viewModel;
            }
        }

        // Handler pour changement de date (pré-défini)
        private void DatePicker_SelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (this.DataContext is ReservationDialogViewModel viewModel && !viewModel.UseCustomTime)
            {
                System.Diagnostics.Debug.WriteLine("Date changed - filtering creneaux");
                viewModel.OnDateSelected();
            }
        }

        // 🆕 Handler pour changement de dates/heures personnalisées
        private void CustomTimeChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine("CustomTimeChanged event triggered");
                _viewModel.UpdateCanCreateReservationPublic();
            }
        }

        // Handler pour changement du motif
        private void MotifTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                System.Diagnostics.Debug.WriteLine("Motif changed - updating button state");
                _viewModel.UpdateCanCreateReservationPublic();
            }
        }
    }
}
