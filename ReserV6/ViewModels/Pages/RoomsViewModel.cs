using Wpf.Ui.Abstractions.Controls;
using System.Collections.ObjectModel;
using ReserV6.ViewModels.Windows;
using ReserV6.Views.Windows;

namespace ReserV6.ViewModels.Pages
{
    public partial class RoomsViewModel : ObservableObject, INavigationAware
    {
        private static ReservationSystemInitializer? _initializer;
        private RepositoryManager? _repositoryManager;
        private List<Salle> _allRooms = [];

        // Optimization: Debouncing and async filtering
        private CancellationTokenSource? _filterCancellationTokenSource;
        private Task? _filteringTask;
        private const int DEBOUNCE_DELAY_MS = 300;

        [ObservableProperty]
        private IEnumerable<Salle> _rooms = [];

        [ObservableProperty]
        private IEnumerable<Salle> _filteredRooms = [];

        [ObservableProperty]
        private Salle? _selectedRoom;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _minCapacity = 0;

        [ObservableProperty]
        private int? _selectedFloor;

        [ObservableProperty]
        private ObservableCollection<int?> _floors = new() { null };

        [ObservableProperty]
        private ObservableCollection<EquipementSelectionItem> _availableEquipements = new();

        [ObservableProperty]
        private ObservableCollection<Equipement> _selectedEquipements = new();

        // Public command used by UI - keeps debounce behavior
        [RelayCommand]
        public async Task ApplyFilters()
        {
            await ApplyFiltersInternal(skipDebounce: false);
        }

        [RelayCommand]
        public void ClearEquipements()
        {
            foreach (var item in AvailableEquipements)
            {
                item.IsSelected = false;
            }
        }

        [RelayCommand]
        public void ResetAllFilters()
        {
            SearchText = string.Empty;
            MinCapacity = 0;
            SelectedFloor = null;

            // Clear all equipment selections
            foreach (var item in AvailableEquipements)
            {
                item.IsSelected = false;
            }

            System.Diagnostics.Debug.WriteLine("RoomsViewModel: All filters have been reset");
        }

        // Internal implementation allowing to skip the debounce when needed
        private async Task ApplyFiltersInternal(bool skipDebounce)
        {
            // Cancel previous filtering task
            _filterCancellationTokenSource?.Cancel();
            _filterCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _filterCancellationTokenSource.Token;

            try
            {
                // Wait for debounce delay unless caller requested immediate execution
                if (!skipDebounce)
                {
                    await Task.Delay(DEBOUNCE_DELAY_MS, cancellationToken);
                }

                System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Applying filters - SearchText: {SearchText}, MinCapacity: {MinCapacity}, Floor: {SelectedFloor}, Selected Equipements: {SelectedEquipements.Count}, SkipDebounce: {skipDebounce}");

                // Run filtering on thread pool to avoid blocking UI
                _filteringTask = Task.Run(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var filtered = _allRooms.AsEnumerable();

                    // Filter by search text (case-insensitive and optimized)
                    if (!string.IsNullOrWhiteSpace(SearchText))
                    {
                        var search = SearchText.ToLower();
                        filtered = filtered.Where(r =>
                            (r.Nom?.ToLower().Contains(search) ?? false) ||
                            (r.Description?.ToLower().Contains(search) ?? false)
                        );
                    }

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // Filter by minimum capacity
                    if (MinCapacity > 0)
                    {
                        filtered = filtered.Where(r => r.Capacite >= MinCapacity);
                    }

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // Filter by floor
                    if (SelectedFloor.HasValue)
                    {
                        filtered = filtered.Where(r => r.Etage == SelectedFloor.Value);
                    }

                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // Filter by selected equipements - salle must have ALL selected equipment types
                    if (SelectedEquipements.Count > 0)
                    {
                        var selectedEquipementTypes = SelectedEquipements.Select(e => e.Type).ToHashSet();
                        filtered = filtered.Where(r =>
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return false;

                            if (r.Equipements == null || r.Equipements.Count == 0)
                                return false;

                            var salleEquipementTypes = r.Equipements.Select(e => e.Type).ToHashSet();
                            return selectedEquipementTypes.All(type => salleEquipementTypes.Contains(type));
                        }).ToList();
                    }
                    else
                    {
                        filtered = filtered.ToList();
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        FilteredRooms = (List<Salle>)filtered;
                        System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Filtered to {FilteredRooms.Count()} rooms");
                    }
                }, cancellationToken);

                await _filteringTask;
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("RoomsViewModel: Filter operation was cancelled");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Error applying filters - {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task Reserve(Salle? salle)
        {
            if (salle == null || _repositoryManager == null)
            {
                System.Diagnostics.Debug.WriteLine("RoomsViewModel: Reserve command called with null salle or repositoryManager");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Reserve button clicked for salle {salle.Nom}");
            SelectedRoom = salle;

            // Open reservation dialog
            await OpenReservationDialog(salle);
        }

        private async Task OpenReservationDialog(Salle salle)
        {
            try
            {
                // Create the ViewModel
                var dialogViewModel = new ReservationDialogViewModel();
                await dialogViewModel.LoadCreneaux(salle, _repositoryManager!);

                // Create the Window
                var dialogWindow = new ReservationDialogWindow
                {
                    DataContext = new { ViewModel = dialogViewModel },
                    Owner = System.Windows.Application.Current.MainWindow
                };

                // Show as dialog
                var result = dialogWindow.ShowDialog();

                System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Reservation dialog closed with result: {result}");

                // Refresh the data if needed
                if (result == true)
                {
                    await LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RoomsViewModel OpenReservationDialog Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        public RoomsViewModel()
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
                System.Diagnostics.Debug.WriteLine("RoomsViewModel: Starting data load...");

                if (_initializer == null)
                {
                    System.Diagnostics.Debug.WriteLine("RoomsViewModel: Initializing system...");
                    _initializer = new ReservationSystemInitializer();
                    _initializer.Initialize();
                    System.Diagnostics.Debug.WriteLine("RoomsViewModel: System initialized");
                }

                _repositoryManager = _initializer.GetRepositoryManager();

                // Load rooms and equipements in parallel for better performance
                var roomsTask = Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("RoomsViewModel: Fetching rooms from database...");
                    var result = _repositoryManager.Salles.GetAllSalles();
                    System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Retrieved {result?.Count ?? 0} rooms");
                    return result;
                });

                var equipementsTask = Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("RoomsViewModel: Fetching all equipements from database...");
                    var result = _repositoryManager.Equipements.GetAllEquipements();
                    System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Retrieved {result?.Count ?? 0} equipements");
                    return result;
                });

                // Wait for both tasks to complete
                await Task.WhenAll(roomsTask, equipementsTask);

                var rooms = roomsTask.Result;
                var equipements = equipementsTask.Result;

                if (rooms != null && rooms.Count > 0)
                {
                    _allRooms = rooms;
                    Rooms = rooms;
                    FilteredRooms = rooms;

                    // Load available floors - optimize by caching distinct values
                    var floors = new ObservableCollection<int?> { null };
                    var distinctFloors = rooms.Select(r => r.Etage)
                        .Distinct()
                        .OrderBy(f => f)
                        .ToList();

                    foreach (var floor in distinctFloors)
                    {
                        floors.Add(floor);
                    }
                    Floors = floors;

                    if (equipements != null && equipements.Count > 0)
                    {
                        // Optimize: Use dictionary to deduplicate by Type and keep one per type
                        var equipementsByType = new Dictionary<string, Equipement>();

                        foreach (var eq in equipements.OrderBy(e => e.Type).ThenBy(e => e.Nom))
                        {
                            // Keep only the first equipment of each type
                            if (!equipementsByType.ContainsKey(eq.Type))
                            {
                                equipementsByType[eq.Type] = eq;
                            }
                        }

                        var selectionItems = equipementsByType.Values
                            .Select(eq => new EquipementSelectionItem(eq, isSelected: false))
                            .ToList();

                        AvailableEquipements = new ObservableCollection<EquipementSelectionItem>(selectionItems);
                        System.Diagnostics.Debug.WriteLine($"RoomsViewModel: {AvailableEquipements.Count} unique equipment types loaded");
                    }

                    System.Diagnostics.Debug.WriteLine($"RoomsViewModel: {rooms.Count} rooms loaded successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("RoomsViewModel: No rooms found in database");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RoomsViewModel Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Handler automatique pour le changement des équipements disponibles (peut inclure changement de sélection des items)
        /// </summary>
        partial void OnAvailableEquipementsChanged(ObservableCollection<EquipementSelectionItem> oldValue, ObservableCollection<EquipementSelectionItem> newValue)
        {
            // Unsubscribe from old collection
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= AvailableEquipements_CollectionChanged;
                foreach (var item in oldValue)
                {
                    item.PropertyChanged -= EquipementSelectionItem_PropertyChanged;
                }
            }

            // Subscribe to new collection
            if (newValue != null)
            {
                newValue.CollectionChanged += AvailableEquipements_CollectionChanged;
                foreach (var item in newValue)
                {
                    item.PropertyChanged += EquipementSelectionItem_PropertyChanged;
                }
            }

            SyncSelectedEquipements();
        }

        /// <summary>
        /// Handle property changes in equipment selection items (when checkbox is toggled)
        /// </summary>
        private void EquipementSelectionItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EquipementSelectionItem.IsSelected) && sender is EquipementSelectionItem item)
            {
                System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Equipment selection changed - {item.Equipement.Nom}: {item.IsSelected}");
                SyncSelectedEquipements();
            }
        }

        /// <summary>
        /// Handle collection changes in available equipments
        /// </summary>
        private void AvailableEquipements_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EquipementSelectionItem item in e.NewItems)
                {
                    item.PropertyChanged += EquipementSelectionItem_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EquipementSelectionItem item in e.OldItems)
                {
                    item.PropertyChanged -= EquipementSelectionItem_PropertyChanged;
                }
            }

            SyncSelectedEquipements();
        }

        /// <summary>
        /// Synchronize SelectedEquipements based on which items are checked in AvailableEquipements
        /// </summary>
        private void SyncSelectedEquipements()
        {
            var checkedEquipements = AvailableEquipements
                .Where(item => item.IsSelected)
                .Select(item => item.Equipement)
                .ToList();

            // Clear and rebuild selected equipements
            SelectedEquipements.Clear();
            foreach (var eq in checkedEquipements)
            {
                SelectedEquipements.Add(eq);
            }
        }

        /// <summary>
        /// Handler automatique pour le changement des équipements sélectionnés
        /// </summary>
        partial void OnSelectedEquipementsChanged(ObservableCollection<Equipement> oldValue, ObservableCollection<Equipement> newValue)
        {
            System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Selected equipements changed, count: {newValue?.Count ?? 0}");
            // Apply filters asynchronously without debouncing (equipment selection is direct)
            _ = ApplyFiltersInternal(skipDebounce: true);
        }

        /// <summary>
        /// Handler automatique pour le changement du texte de recherche
        /// </summary>
        partial void OnSearchTextChanged(string oldValue, string newValue)
        {
            System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Search text changed to '{newValue}'");
            // Apply filters asynchronously with debouncing
            _ = ApplyFilters();
        }

        /// <summary>
        /// Handler automatique pour le changement de la capacité minimale
        /// </summary>
        partial void OnMinCapacityChanged(int oldValue, int newValue)
        {
            System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Min capacity changed to {newValue}");
            // Apply filters asynchronously with debouncing
            _ = ApplyFilters();
        }

        /// <summary>
        /// Handler automatique pour le changement de l'étage sélectionné
        /// </summary>
        partial void OnSelectedFloorChanged(int? oldValue, int? newValue)
        {
            System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Selected floor changed to {newValue}");
            // Apply filters asynchronously without debouncing (floor selection is direct)
            _ = ApplyFiltersInternal(skipDebounce: true);
        }
    }
}
