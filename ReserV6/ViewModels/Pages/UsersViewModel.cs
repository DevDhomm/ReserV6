using Wpf.Ui.Abstractions.Controls;

namespace ReserV6.ViewModels.Pages
{
    public partial class UsersViewModel : ObservableObject, INavigationAware
    {
        private static ReservationSystemInitializer? _initializer;
        private RepositoryManager? _repositoryManager;

        [ObservableProperty]
        private IEnumerable<User> _users = [];

        [ObservableProperty]
        private User? _selectedUser;

        public UsersViewModel()
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
                System.Diagnostics.Debug.WriteLine("UsersViewModel: Starting data load...");
                
                if (_initializer == null)
                {
                    System.Diagnostics.Debug.WriteLine("UsersViewModel: Initializing system...");
                    _initializer = new ReservationSystemInitializer();
                    _initializer.Initialize();
                    System.Diagnostics.Debug.WriteLine("UsersViewModel: System initialized");
                }
                
                _repositoryManager = _initializer.GetRepositoryManager();
                
                var users = await Task.Run(() => 
                {
                    System.Diagnostics.Debug.WriteLine("UsersViewModel: Fetching users from database...");
                    var result = _repositoryManager.Users.GetAllUsers();
                    System.Diagnostics.Debug.WriteLine($"UsersViewModel: Retrieved {result?.Count ?? 0} users");
                    return result;
                });
                
                if (users != null && users.Count > 0)
                {
                    Users = users;
                    System.Diagnostics.Debug.WriteLine($"UsersViewModel: {users.Count} users loaded successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("UsersViewModel: No users found in database");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UsersViewModel Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}
