using ReserV6.Services.Database;
using ReserV6.Services.Database.Repositories;

namespace ReserV6.Services
{
    /// <summary>
    /// Service pour initialiser le système de réservation
    /// </summary>
    public class ReservationSystemInitializer
    {
        private readonly DatabaseService _databaseService;
        private readonly RepositoryManager _repositories;

        public ReservationSystemInitializer()
        {
            _databaseService = new DatabaseService();
            _repositories = new RepositoryManager(_databaseService);
        }

        /// <summary>
        /// Initialise le système complet
        /// </summary>
        public void Initialize()
        {
            // Initialiser la base de données
            _databaseService.InitializeDatabase();
        }

        /// <summary>
        /// Obtient le gestionnaire de repositories
        /// </summary>
        public RepositoryManager GetRepositoryManager()
        {
            return _repositories;
        }

        /// <summary>
        /// Obtient le service de réservation
        /// </summary>
        public ReservationService GetReservationService()
        {
            return new ReservationService(_repositories);
        }

        /// <summary>
        /// Obtient l'utilisateur par défaut
        /// </summary>
        public User? GetDefaultUser()
        {
            return _repositories.Users.GetUserByEmail("jean.dupont@ecole.fr");
        }

        /// <summary>
        /// Obtient toutes les salles disponibles
        /// </summary>
        public List<Salle> GetAvailableRooms()
        {
            return _repositories.Salles.GetAvailableSalles();
        }

        /// <summary>
        /// Obtient toutes les réservations complètes
        /// </summary>
        public List<ReservationComplete> GetAllReservations()
        {
            return _repositories.Reservations.GetCompleteReservations();
        }

        /// <summary>
        /// Obtient les salles avec leurs équipements
        /// </summary>
        public List<SalleAvecEquipements> GetRoomsWithEquipments()
        {
            return _repositories.Salles.GetSallesWithEquipments();
        }
    }
}
