using ReserV6.Models;
using ReserV6.Services.Database.Repositories;

namespace ReserV6.Services
{
    /// <summary>
    /// Service métier pour la gestion des réservations
    /// </summary>
    public class ReservationService
    {
        private readonly RepositoryManager _repositories;

        public ReservationService(RepositoryManager repositories)
        {
            _repositories = repositories;
        }

        /// <summary>
        /// Crée une nouvelle réservation
        /// </summary>
        public (bool Success, string Message, int ReservationId) CreateReservation(
            int userId, int salleId, int creneauId, string motif)
        {
            // Vérifier que l'utilisateur existe
            var user = _repositories.Users.GetUserById(userId);
            if (user == null)
                return (false, "Utilisateur non trouvé", 0);

            // Vérifier que la salle existe et est disponible
            var salle = _repositories.Salles.GetSalleById(salleId);
            if (salle == null)
                return (false, "Salle non trouvée", 0);

            if (!salle.Disponibilite)
                return (false, "La salle n'est pas disponible", 0);

            // Vérifier que le créneau existe
            var creneau = _repositories.Creneaux.GetCreneauById(creneauId);
            if (creneau == null)
                return (false, "Créneau non trouvé", 0);

            // Vérifier qu'il n'y a pas de conflit
            if (_repositories.Reservations.HasConflict(salleId, creneauId))
                return (false, "Cette salle est déjà réservée pour ce créneau", 0);

            // Créer la réservation (par défaut confirmée)
            var reservation = new Reservation
            {
                DateReservation = DateTime.Now,
                Motif = motif,
                Statut = ReservationStatut.Confirmée,
                UserId = userId,
                SalleId = salleId,
                CreneauId = creneauId
            };

            int reservationId = _repositories.Reservations.CreateReservation(reservation);

            // Ajouter à l'historique
            _repositories.Historiques.AddAction(reservationId, "Réservation créée et confirmée");

            return (true, "Réservation créée avec succès", reservationId);
        }

        /// <summary>
        /// Annule une réservation
        /// </summary>
        public (bool Success, string Message) CancelReservation(int reservationId)
        {
            var reservation = _repositories.Reservations.GetReservationById(reservationId);
            if (reservation == null)
                return (false, "Réservation non trouvée");

            if (reservation.Statut == ReservationStatut.Annulée)
                return (false, "Cette réservation est déjà annulée");

            if (reservation.Statut == ReservationStatut.Terminée)
                return (false, "Impossible d'annuler une réservation terminée");

            // Annuler la réservation
            if (_repositories.Reservations.CancelReservation(reservationId))
            {
                _repositories.Historiques.AddAction(reservationId, "Réservation annulée");
                return (true, "Réservation annulée avec succès");
            }

            return (false, "Erreur lors de l'annulation de la réservation");
        }

        /// <summary>
        /// Modifie une réservation existante
        /// </summary>
        public (bool Success, string Message) ModifyReservation(
            int reservationId, int? newSalleId = null, int? newCreneauId = null, string? newMotif = null)
        {
            var reservation = _repositories.Reservations.GetReservationById(reservationId);
            if (reservation == null)
                return (false, "Réservation non trouvée");

            if (reservation.Statut == ReservationStatut.Annulée)
                return (false, "Impossible de modifier une réservation annulée");

            if (reservation.Statut == ReservationStatut.Terminée)
                return (false, "Impossible de modifier une réservation terminée");

            // Appliquer les modifications
            int salleId = newSalleId ?? reservation.SalleId;
            int creneauId = newCreneauId ?? reservation.CreneauId;

            // Vérifier les conflits si la salle ou le créneau change
            if ((newSalleId.HasValue || newCreneauId.HasValue) &&
                _repositories.Reservations.HasConflict(salleId, creneauId, reservationId))
            {
                return (false, "Cette salle est déjà réservée pour ce créneau");
            }

            reservation.SalleId = salleId;
            reservation.CreneauId = creneauId;
            if (!string.IsNullOrEmpty(newMotif))
                reservation.Motif = newMotif;

            if (_repositories.Reservations.UpdateReservation(reservation))
            {
                _repositories.Historiques.AddAction(reservationId, "Réservation modifiée");
                return (true, "Réservation modifiée avec succès");
            }

            return (false, "Erreur lors de la modification de la réservation");
        }

        /// <summary>
        /// Récupère les réservations disponibles pour une période et une capacité minimum
        /// </summary>
        public List<SalleAvecEquipements> FindAvailableRooms(DateTime debut, DateTime fin, int minCapacity)
        {
            var availableSalles = _repositories.Salles.GetAvailableSallesForPeriod(debut, fin);
            var withEquipments = _repositories.Salles.GetSallesWithEquipments();

            return withEquipments
                .Where(s => availableSalles.Any(a => a.Id == s.Id) && s.Capacite >= minCapacity)
                .OrderBy(s => s.Etage)
                .ThenBy(s => s.Capacite)
                .ToList();
        }

        /// <summary>
        /// Récupère l'historique complet d'une réservation
        /// </summary>
        public List<(DateTime DateAction, string Action, string UserNom, string SalleName)> GetReservationHistory(int reservationId)
        {
            return _repositories.Historiques.GetDetailedReservationHistory(reservationId);
        }

        /// <summary>
        /// Récupère les statistiques d'occupation d'une salle
        /// </summary>
        public (int TotalReservations, int Confirmed, decimal OccupancyRate) GetRoomStatistics(int salleId)
        {
            var reservations = _repositories.Reservations.GetSalleReservations(salleId);
            int total = reservations.Count;
            int confirmed = reservations.Count(r => r.Statut == ReservationStatut.Confirmée);
            decimal rate = total > 0 ? (confirmed / (decimal)total) * 100 : 0;

            return (total, confirmed, rate);
        }
    }
}
