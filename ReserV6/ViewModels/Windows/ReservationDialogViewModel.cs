using System.Collections.ObjectModel;

namespace ReserV6.ViewModels.Windows
{
    public partial class ReservationDialogViewModel : ObservableObject
    {
        private RepositoryManager? _repositoryManager;
        private ConflictResolutionService? _conflictService;
        private List<Creneau> _allCreneaux = [];
        private HashSet<int> _bookedCreneauIds = [];

        [ObservableProperty]
        private Salle? _selectedSalle;

        [ObservableProperty]
        private string _motif = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Creneau> _allAvailableCreneaux = [];

        [ObservableProperty]
        private ObservableCollection<Creneau> _creneauxForSelectedDate = [];

        [ObservableProperty]
        private Creneau? _selectedCreneau;

        [ObservableProperty]
        private bool _canCreateReservation;

        [ObservableProperty]
        private DateTime _selectedDate = DateTime.Today;

        [ObservableProperty]
        private ObservableCollection<DateTime> _availableDates = [];

        [ObservableProperty]
        private string _conflictMessage = string.Empty;

        [ObservableProperty]
        private bool _hasConflictWarning;

        // 🆕 Horaires personnalisés
        [ObservableProperty]
        private DateTime _customStartDate = DateTime.Today;  // 🆕 Date de début personnalisée

        [ObservableProperty]
        private TimeSpan _customStartTime = new TimeSpan(9, 0, 0);

        [ObservableProperty]
        private TimeSpan _customEndTime = new TimeSpan(10, 0, 0);

        [ObservableProperty]
        private bool _useCustomTime = false;

        [ObservableProperty]
        private DateTime? _minimumDate = DateTime.Today;

        [ObservableProperty]
        private DateTime _customEndDate = DateTime.Today;

        [RelayCommand]
        public void SelectCreneau(Creneau? creneau)
        {
            if (creneau == null)
            {
                System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: SelectCreneau called with null creneau");
                return;
            }

            _selectedCreneau = creneau;

            // Check for conflict
            if (_repositoryManager != null && _selectedSalle != null)
            {
                bool hasConflict = _repositoryManager.Reservations.HasConflict(_selectedSalle.Id, creneau.Id);

                if (hasConflict)
                {
                    HasConflictWarning = true;
                    ConflictMessage = $"Conflit: Le créneau {creneau.Debut:HH:mm} - {creneau.Fin:HH:mm} est déjà réservé!";
                    System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: Conflict detected for creneau {creneau.Id}");
                }
                else
                {
                    HasConflictWarning = false;
                    ConflictMessage = string.Empty;
                    System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: Creneau selected - {creneau.Debut:dd/MM/yyyy HH:mm} to {creneau.Fin:HH:mm}");
                }
            }

            UpdateCanCreateReservation();
        }

        [RelayCommand]
        public void OnDateSelected()
        {
            System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: Date selected - {_selectedDate:dd/MM/yyyy}");
            FilterCreneauxByDate();
            _selectedCreneau = null;
            UpdateCanCreateReservation();
        }

        [RelayCommand]
        public async Task CreateReservation()
        {
            try
            {
                if (_selectedSalle == null || _repositoryManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: Cannot create reservation - missing salle or repository");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_motif))
                {
                    System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: Motif is empty");
                    return;
                }

                DateTime startDateTime;
                DateTime endDateTime;

                if (_useCustomTime)
                {
                    // Mode horaire personnalisé
                    startDateTime = _customStartDate.Date.Add(_customStartTime);
                    endDateTime = _customEndDate.Date.Add(_customEndTime);

                    // Validation
                    if (startDateTime >= endDateTime)
                    {
                        System.Windows.MessageBox.Show(
                            "La date/heure de début doit être avant la date/heure de fin!",
                            "Erreur",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error
                        );
                        return;
                    }

                    if (_customEndDate < _customStartDate)
                    {
                        System.Windows.MessageBox.Show(
                            "La date de fin doit être après la date de début!",
                            "Erreur",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error
                        );
                        return;
                    }

                    // Vérifier les conflits avec HasTimeConflict
                    bool hasConflict = await Task.Run(() =>
                        _repositoryManager.Reservations.HasTimeConflict(_selectedSalle.Id, startDateTime, endDateTime));

                    if (hasConflict)
                    {
                        System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: Time conflict detected");
                        System.Windows.MessageBox.Show(
                            $"Conflit detecté! La salle est déjà reservée entre {startDateTime:dd/MM/yyyy HH:mm} et {endDateTime:dd/MM/yyyy HH:mm}.",
                            "Conflit de reservation",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning
                        );
                        return;
                    }
                }
                else
                {
                    // Mode créneau pré-défini
                    if (_selectedCreneau == null)
                    {
                        System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: No creneau selected");
                        return;
                    }

                    startDateTime = _selectedCreneau.Debut;
                    endDateTime = _selectedCreneau.Fin;

                    // Double-check pour conflits
                    bool hasConflict = await Task.Run(() =>
                        _repositoryManager.Reservations.HasConflict(_selectedSalle.Id, _selectedCreneau.Id));

                    if (hasConflict)
                    {
                        System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: Time conflict detected");
                        System.Windows.MessageBox.Show(
                            $"Conflit detecté! Le creneau {_selectedCreneau.Debut:HH:mm} - {_selectedCreneau.Fin:HH:mm} est déjà reservé pour cette salle.",
                            "Conflit de reservation",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Warning
                        );
                        return;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: Creating reservation for salle {_selectedSalle.Nom}");

                // Get current user
                var user = await Task.Run(() => _repositoryManager.Users.GetUserById(1));

                if (user == null)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: User not found");
                    return;
                }

                // Create the reservation with separated date and time fields
                var reservation = new Reservation
                {
                    DateReservation = DateTime.Now,
                    Motif = _motif,
                    Statut = ReservationStatut.Confirmée,
                    UserId = user.Id,
                    SalleId = _selectedSalle.Id,
                    CreneauId = _useCustomTime ? null : _selectedCreneau?.Id,
                    DateDebut = startDateTime.Date,
                    DateFin = endDateTime.Date,
                    HeureDebut = startDateTime.TimeOfDay,
                    HeureFin = endDateTime.TimeOfDay
                };

                var reservationId = await Task.Run(() => _repositoryManager.Reservations.CreateReservation(reservation));

                System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: Reservation created with ID {reservationId}");

                if (reservationId > 0)
                {
                    string timeRange = startDateTime.Date == endDateTime.Date
                        ? $"Le {startDateTime:dd/MM/yyyy} de {startDateTime:HH:mm} à {endDateTime:HH:mm}"
                        : $"Du {startDateTime:dd/MM/yyyy HH:mm} au {endDateTime:dd/MM/yyyy HH:mm}";

                    System.Windows.MessageBox.Show(
                        $"Reservation confirmee pour la salle {_selectedSalle.Nom}\n{timeRange}",
                        "Succes",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        public async Task LoadCreneaux(Salle salle, RepositoryManager repositoryManager)
        {
            try
            {
                _selectedSalle = salle;
                _repositoryManager = repositoryManager;
                _conflictService = new ConflictResolutionService(repositoryManager);
                _motif = string.Empty;
                _selectedCreneau = null;
                _selectedDate = DateTime.Today;
                _customEndDate = DateTime.Today;
                _useCustomTime = false;
                _minimumDate = DateTime.Today;

                System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: Loading creneaux for salle {salle.Nom}");

                var creneaux = await Task.Run(() => repositoryManager.Creneaux.GetAllCreneaux());

                if (creneaux != null && creneaux.Count > 0)
                {
                    _allCreneaux = creneaux;

                    // Filter out booked creneaux for this salle
                    _bookedCreneauIds = await Task.Run(() => 
                    {
                        var reservations = repositoryManager.Reservations.GetSalleReservations(salle.Id);
                        return reservations.Where(r => r.CreneauId.HasValue).Select(r => r.CreneauId.Value).ToHashSet();
                    });

                    var available = creneaux
                        .Where(c => !_bookedCreneauIds.Contains(c.Id))
                        .OrderBy(c => c.Debut)
                        .ToList();

                    _allCreneaux = available; // Store filtered list

                    // 🆕 Générer toutes les dates futures (365 jours)
                    var futureDate = new List<DateTime>();
                    var today = DateTime.Today;
                    for (int i = 0; i < 365; i++)
                    {
                        futureDate.Add(today.AddDays(i));
                    }

                    AvailableDates = new ObservableCollection<DateTime>(futureDate);

                    System.Diagnostics.Debug.WriteLine(
                        $"ReservationDialogViewModel: Loaded {available.Count} predefined creneaux"
                    );

                    // Filter creneaux for today
                    FilterCreneauxByDate();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ReservationDialogViewModel: No creneaux found");
                }

                UpdateCanCreateReservation();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel LoadCreneaux Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        private void FilterCreneauxByDate()
        {
            var filtered = _allCreneaux
                .Where(c => c.Debut.Date == _selectedDate.Date)
                .OrderBy(c => c.Debut)
                .ToList();

            CreneauxForSelectedDate = new ObservableCollection<Creneau>(filtered);

            System.Diagnostics.Debug.WriteLine(
                $"ReservationDialogViewModel: Filtered {filtered.Count} creneaux for {_selectedDate:dd/MM/yyyy}"
            );
        }

        private void UpdateCanCreateReservation()
        {
            bool hasRequiredData = _selectedSalle != null && !string.IsNullOrWhiteSpace(_motif) && !HasConflictWarning;

            if (_useCustomTime)
            {
                // Mode personnalisé: doit avoir dates et heures valides
                CanCreateReservation = hasRequiredData && 
                    _customEndDate >= _customStartDate &&
                    (_customEndDate > _customStartDate || _customEndTime > _customStartTime);
            }
            else
            {
                // Mode créneaux pré-définis: doit avoir un créneau sélectionné
                CanCreateReservation = hasRequiredData && _selectedCreneau != null;
            }

            System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: CanCreateReservation = {CanCreateReservation}");
        }

        // 🆕 Public pour pouvoir l'appeler depuis le code-behind
        public void UpdateCanCreateReservationPublic()
        {
            UpdateCanCreateReservation();
        }

        // 🆕 Handler automatique quand le motif change (partial method for MVVM Toolkit)
        partial void OnMotifChanged(string oldValue, string newValue)
        {
            System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: Motif changed from '{oldValue}' to '{newValue}'");
            UpdateCanCreateReservation();
        }

        // 🆕 Handler pour CustomStartDate
        partial void OnCustomStartDateChanged(DateTime oldValue, DateTime newValue)
        {
            System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: CustomStartDate changed to {newValue:dd/MM/yyyy}");
            UpdateCanCreateReservation();
        }

        // 🆕 Handler pour CustomEndDate
        partial void OnCustomEndDateChanged(DateTime oldValue, DateTime newValue)
        {
            System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: CustomEndDate changed to {newValue:dd/MM/yyyy}");
            UpdateCanCreateReservation();
        }

        // 🆕 Handler pour CustomStartTime
        partial void OnCustomStartTimeChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: CustomStartTime changed to {newValue:hh\\:mm}");
            UpdateCanCreateReservation();
        }

        // 🆕 Handler pour CustomEndTime
        partial void OnCustomEndTimeChanged(TimeSpan oldValue, TimeSpan newValue)
        {
            System.Diagnostics.Debug.WriteLine($"ReservationDialogViewModel: CustomEndTime changed to {newValue:hh\\:mm}");
            UpdateCanCreateReservation();
        }
    }
}
