using Wpf.Ui.Abstractions.Controls;

namespace ReserV6.ViewModels.Pages
{
    public partial class ReservationsViewModel : ObservableObject, INavigationAware
    {
        private static ReservationSystemInitializer? _initializer;
        private RepositoryManager? _repositoryManager;
        private ReservationStatusService? _statusService;

        [ObservableProperty]
        private IEnumerable<ReservationComplete> _reservations = [];

        [ObservableProperty]
        private ReservationComplete? _selectedReservation;

        // 🆕 Filtrage par statut
        [ObservableProperty]
        private string _selectedStatusFilter = "Tous";

        [ObservableProperty]
        private IEnumerable<string> _statusFilterOptions = new[]
        {
            "Tous",
            "Confirmee",
            "EnCours",
            "Terminee",
            "Annulee"
        };

        [ObservableProperty]
        private IEnumerable<ReservationComplete> _filteredReservations = [];

        public ReservationsViewModel()
        {
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadDataAsync();
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private async Task LoadDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Starting data load...");

                if (_initializer == null)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Initializing system...");
                    _initializer = new ReservationSystemInitializer();
                    _initializer.Initialize();
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: System initialized");
                }

                _repositoryManager = _initializer.GetRepositoryManager();
                _statusService = new ReservationStatusService(_repositoryManager);

                // 🆕 Mettre à jour les statuts avant de charger
                await Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Updating reservation statuses...");
                    _statusService.UpdateAllReservationStatuses();
                });

                var reservations = await Task.Run(() => 
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Fetching reservations from database...");
                    var result = _repositoryManager.Reservations.GetCompleteReservations();
                    System.Diagnostics.Debug.WriteLine($"ReservationsViewModel: Retrieved {result?.Count ?? 0} reservations");
                    return result;
                });

                if (reservations != null && reservations.Count > 0)
                {
                    Reservations = reservations;
                    System.Diagnostics.Debug.WriteLine($"ReservationsViewModel: {reservations.Count} reservations loaded successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: No reservations found in database");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReservationsViewModel Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Supprime une réservation
        /// </summary>
        [RelayCommand]
        public async Task DeleteReservation(ReservationComplete? reservation)
        {
            if (reservation == null)
            {
                System.Diagnostics.Debug.WriteLine("ReservationsViewModel: DeleteReservation called with null reservation");
                return;
            }

            try
            {
                var result = System.Windows.MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer la réservation?\n\nSalle: {reservation.SalleNom}\nDate: {reservation.CreneauDebut:dd/MM/yyyy HH:mm}",
                    "Confirmation de suppression",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning
                );

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Deletion cancelled by user");
                    return;
                }

                if (_repositoryManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Repository manager is null");
                    return;
                }

                var deleted = await Task.Run(() => 
                    _repositoryManager.Reservations.DeleteReservation(reservation.Id)
                );

                if (deleted)
                {
                    System.Windows.MessageBox.Show(
                        "Réservation supprimée avec succès!",
                        "Succès",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );

                    // Recharger les données
                    await LoadDataAsync();
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Erreur lors de la suppression de la réservation",
                        "Erreur",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReservationsViewModel DeleteReservation Error: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur: {ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Annule une réservation (change son statut à Annulée)
        /// </summary>
        [RelayCommand]
        public async Task CancelReservation(ReservationComplete? reservation)
        {
            if (reservation == null)
            {
                System.Diagnostics.Debug.WriteLine("ReservationsViewModel: CancelReservation called with null reservation");
                return;
            }

            try
            {
                var result = System.Windows.MessageBox.Show(
                    $"Êtes-vous sûr de vouloir annuler cette réservation?\n\nSalle: {reservation.SalleNom}\nDate: {reservation.CreneauDebut:dd/MM/yyyy HH:mm}",
                    "Confirmation d'annulation",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning
                );

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Cancellation cancelled by user");
                    return;
                }

                if (_repositoryManager == null || _statusService == null)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationsViewModel: Service is null");
                    return;
                }

                var updated = await Task.Run(() => 
                    _statusService.UpdateReservationStatus(reservation.Id, ReservationStatut.Annulée)
                );

                if (updated)
                {
                    System.Windows.MessageBox.Show(
                        "Réservation annulée avec succès!",
                        "Succès",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );

                    // Recharger les données
                    await LoadDataAsync();
                }
                else
                {
                    System.Windows.MessageBox.Show(
                        "Erreur lors de l'annulation de la réservation",
                        "Erreur",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReservationsViewModel CancelReservation Error: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur: {ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Applique le filtrage par statut et équipement à la liste des réservations
        /// </summary>
        private void ApplyStatusFilter()
        {
            var filtered = Reservations;

            // Filtrer par statut
            if (SelectedStatusFilter != "Tous")
            {
                filtered = filtered
                    .Where(r =>
                    {
                        // Récalculer le statut dynamique pour chaque réservation
                        var now = DateTime.Now;
                        string status;

                        if (now >= r.CreneauFin)
                            status = "Terminee";
                        else if (now >= r.CreneauDebut && now < r.CreneauFin)
                            status = "EnCours";
                        else if (r.Statut == "Annulee")
                            status = "Annulee";
                        else
                            status = r.Statut;

                        return status == SelectedStatusFilter;
                    })
                    .ToList();
            }

            FilteredReservations = filtered;

            System.Diagnostics.Debug.WriteLine(
                $"ReservationsViewModel: Filter applied, showing {FilteredReservations.Count()} of {Reservations.Count()} reservations"
            );
        }

        /// <summary>
        /// Handler automatique pour le changement du filtre de statut
        /// </summary>
        partial void OnSelectedStatusFilterChanged(string oldValue, string newValue)
        {
            System.Diagnostics.Debug.WriteLine($"ReservationsViewModel: Status filter changed to {newValue}");
            ApplyStatusFilter();
        }

        /// <summary>
        /// Handler automatique pour le changement des réservations
        /// </summary>
        partial void OnReservationsChanged(IEnumerable<ReservationComplete> oldValue, IEnumerable<ReservationComplete> newValue)
        {
            System.Diagnostics.Debug.WriteLine($"ReservationsViewModel: Reservations changed, applying filter");
            ApplyStatusFilter();
        }
    }
}
