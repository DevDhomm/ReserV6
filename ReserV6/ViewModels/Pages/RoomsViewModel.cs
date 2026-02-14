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

        [RelayCommand]
        public void ApplyFilters()
        {
            System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Applying filters - SearchText: {_searchText}, MinCapacity: {_minCapacity}, Floor: {_selectedFloor}");

            var filtered = _allRooms.AsEnumerable();

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var search = _searchText.ToLower();
                filtered = filtered.Where(r => 
                    r.Nom.ToLower().Contains(search) || 
                    r.Description.ToLower().Contains(search)
                );
            }

            // Filter by minimum capacity
            if (_minCapacity > 0)
            {
                filtered = filtered.Where(r => r.Capacite >= _minCapacity);
            }

            // Filter by floor
            if (_selectedFloor.HasValue)
            {
                filtered = filtered.Where(r => r.Etage == _selectedFloor.Value);
            }

            FilteredRooms = filtered.ToList();
            System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Filtered to {FilteredRooms.Count()} rooms");
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

                var rooms = await Task.Run(() => 
                {
                    System.Diagnostics.Debug.WriteLine("RoomsViewModel: Fetching rooms from database...");
                    var result = _repositoryManager.Salles.GetAllSalles();
                    System.Diagnostics.Debug.WriteLine($"RoomsViewModel: Retrieved {result?.Count ?? 0} rooms");
                    return result;
                });

                if (rooms != null && rooms.Count > 0)
                {
                    _allRooms = rooms;
                    Rooms = rooms;
                    FilteredRooms = rooms;

                    // Load available floors
                    var floors = new ObservableCollection<int?> { null };
                    foreach (var floor in rooms.Select(r => r.Etage).Distinct().OrderBy(f => f))
                    {
                        floors.Add(floor);
                    }
                    Floors = floors;

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
    }
}
