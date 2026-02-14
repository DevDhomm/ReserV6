using ReserV6.Models;

namespace ReserV6.Services
{
    /// <summary>
    /// Service utilitaire pour la détection et gestion des conflits de réservation
    /// </summary>
    public class ConflictResolutionService
    {
        private readonly RepositoryManager _repositoryManager;

        public ConflictResolutionService(RepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        /// <summary>
        /// Vérifie s'il y a un conflit pour un créneau pré-défini
        /// </summary>
        public bool CheckCreneauConflict(int salleId, int creneauId)
        {
            return _repositoryManager.Reservations.HasConflict(salleId, creneauId);
        }

        /// <summary>
        /// Vérifie s'il y a un conflit pour une plage horaire personnalisée
        /// </summary>
        public bool CheckTimeConflict(int salleId, DateTime startTime, DateTime endTime)
        {
            // Validation basique
            if (startTime >= endTime)
            {
                throw new ArgumentException("La date/heure de début doit être avant la date/heure de fin");
            }

            return _repositoryManager.Reservations.HasTimeConflict(salleId, startTime, endTime);
        }

        /// <summary>
        /// Récupère les réservations conflictuelles pour une plage horaire donnée
        /// </summary>
        public List<Reservation> GetConflictingReservations(int salleId, DateTime startTime, DateTime endTime)
        {
            var allReservations = _repositoryManager.Reservations.GetSalleReservations(salleId)
                .Where(r => r.Statut == ReservationStatut.Confirmée || r.Statut == ReservationStatut.EnAttente)
                .ToList();

            var conflictingReservations = new List<Reservation>();

            foreach (var reservation in allReservations)
            {
                if (reservation.Creneau == null)
                {
                    var creneau = _repositoryManager.Creneaux.GetCreneauById(reservation.CreneauId);
                    if (creneau != null)
                    {
                        reservation.Creneau = creneau;
                    }
                }

                if (reservation.Creneau != null)
                {
                    // Vérifier le chevauchement
                    if (!(reservation.Creneau.Fin <= startTime || reservation.Creneau.Debut >= endTime))
                    {
                        conflictingReservations.Add(reservation);
                    }
                }
            }

            return conflictingReservations;
        }

        /// <summary>
        /// Vérifie si une salle est disponible pour une période donnée
        /// </summary>
        public bool IsSalleAvailable(int salleId, DateTime startTime, DateTime endTime)
        {
            return !CheckTimeConflict(salleId, startTime, endTime);
        }

        /// <summary>
        /// Génère un message d'erreur détaillé pour un conflit
        /// </summary>
        public string GenerateConflictErrorMessage(int salleId, DateTime startTime, DateTime endTime, string salleName = "")
        {
            var conflicts = GetConflictingReservations(salleId, startTime, endTime);

            if (conflicts.Count == 0)
            {
                return "Aucun conflit détecté";
            }

            var roomDisplay = string.IsNullOrEmpty(salleName) ? $"salle {salleId}" : salleName;
            var message = $"La {roomDisplay} est déjà réservée pendant cette période:\n\n";

            foreach (var conflict in conflicts)
            {
                if (conflict.Creneau != null)
                {
                    message += $"• {conflict.Creneau.Debut:dd/MM/yyyy HH:mm} - {conflict.Creneau.Fin:HH:mm}";
                    if (!string.IsNullOrEmpty(conflict.Motif))
                    {
                        message += $" ({conflict.Motif})";
                    }
                    message += "\n";
                }
            }

            return message;
        }

        /// <summary>
        /// Récupère les créneaux libres pour une salle et une date données
        /// </summary>
        public List<Creneau> GetAvailableCreneaux(int salleId, DateTime date)
        {
            var allCreneaux = _repositoryManager.Creneaux.GetAllCreneaux()
                .Where(c => c.Debut.Date == date.Date)
                .OrderBy(c => c.Debut)
                .ToList();

            var bookedCreneauIds = _repositoryManager.Reservations.GetSalleReservations(salleId)
                .Where(r => r.Statut == ReservationStatut.Confirmée || r.Statut == ReservationStatut.EnAttente)
                .Select(r => r.CreneauId)
                .ToHashSet();

            return allCreneaux.Where(c => !bookedCreneauIds.Contains(c.Id)).ToList();
        }

        /// <summary>
        /// Récupère le statut de disponibilité d'une salle pour une période
        /// </summary>
        public SalleAvailabilityStatus GetSalleAvailabilityStatus(int salleId, DateTime startTime, DateTime endTime)
        {
            var hasConflict = CheckTimeConflict(salleId, startTime, endTime);
            
            if (hasConflict)
            {
                var conflicts = GetConflictingReservations(salleId, startTime, endTime);
                return new SalleAvailabilityStatus
                {
                    IsAvailable = false,
                    ConflictCount = conflicts.Count,
                    ConflictingReservations = conflicts
                };
            }

            return new SalleAvailabilityStatus
            {
                IsAvailable = true,
                ConflictCount = 0,
                ConflictingReservations = new List<Reservation>()
            };
        }
    }

    /// <summary>
    /// Représente le statut de disponibilité d'une salle
    /// </summary>
    public class SalleAvailabilityStatus
    {
        public bool IsAvailable { get; set; }
        public int ConflictCount { get; set; }
        public List<Reservation> ConflictingReservations { get; set; } = new();
    }
}
