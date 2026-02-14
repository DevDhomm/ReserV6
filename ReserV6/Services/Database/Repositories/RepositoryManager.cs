namespace ReserV6.Services.Database.Repositories
{
    /// <summary>
    /// Gestionnaire centralisé de tous les repositories
    /// </summary>
    public class RepositoryManager
    {
        private readonly DatabaseService _dbService;
        private UserRepository? _userRepository;
        private SalleRepository? _salleRepository;
        private ReservationRepository? _reservationRepository;
        private EquipementRepository? _equipementRepository;
        private CreneauRepository? _creneauRepository;
        private HistoriqueRepository? _historiqueRepository;

        public RepositoryManager(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public UserRepository Users => _userRepository ??= new UserRepository(_dbService);
        public SalleRepository Salles => _salleRepository ??= new SalleRepository(_dbService);
        public ReservationRepository Reservations => _reservationRepository ??= new ReservationRepository(_dbService);
        public EquipementRepository Equipements => _equipementRepository ??= new EquipementRepository(_dbService);
        public CreneauRepository Creneaux => _creneauRepository ??= new CreneauRepository(_dbService);
        public HistoriqueRepository Historiques => _historiqueRepository ??= new HistoriqueRepository(_dbService);
    }
}
