using ReserV6.Models;

namespace ReserV6.Services
{
    /// <summary>
    /// Service pour gérer la mise à jour automatique des statuts de réservation
    /// </summary>
    public class ReservationStatusService
    {
        private readonly RepositoryManager _repositoryManager;

        public ReservationStatusService(RepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        /// <summary>
        /// Met à jour les statuts de toutes les réservations basé sur la date actuelle
        /// - "Confirmée" → "EnCours" si la date de début est passée et la fin est future
        /// - "Confirmée" → "Terminée" si la date de fin est passée
        /// - "EnCours" → "Terminée" si la date de fin est passée
        /// </summary>
        public void UpdateAllReservationStatuses()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ReservationStatusService: Updating reservation statuses...");

                var allReservations = _repositoryManager.Reservations.GetAllReservations();

                if (allReservations == null || allReservations.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("ReservationStatusService: No reservations to update");
                    return;
                }

                var now = DateTime.Now;
                int updatedCount = 0;

                foreach (var reservation in allReservations)
                {
                    var reservationStart = reservation.DateTimeDebut;
                    var reservationEnd = reservation.DateTimeFin;
                    var newStatus = reservation.Statut;
                    bool statusChanged = false;

                    // Logique de mise à jour des statuts
                    if (reservation.Statut == ReservationStatut.Annulée)
                    {
                        // Les réservations annulées ne changent pas
                        continue;
                    }

                    if (now >= reservationEnd)
                    {
                        // La réservation est terminée
                        if (reservation.Statut != ReservationStatut.Terminée)
                        {
                            newStatus = ReservationStatut.Terminée;
                            statusChanged = true;
                        }
                    }
                    else if (now >= reservationStart && now < reservationEnd)
                    {
                        // La réservation est en cours
                        if (reservation.Statut != ReservationStatut.EnCours)
                        {
                            newStatus = ReservationStatut.EnCours;
                            statusChanged = true;
                        }
                    }
                    else if (now < reservationStart && reservation.Statut == ReservationStatut.EnCours)
                    {
                        // La réservation était en cours mais n'a plus commencé?? (cas rare)
                        // Ne pas changer le statut
                    }

                    if (statusChanged)
                    {
                        reservation.Statut = newStatus;
                        var updated = _repositoryManager.Reservations.UpdateReservationStatus(
                            reservation.Id, 
                            newStatus
                        );

                        if (updated)
                        {
                            updatedCount++;
                            System.Diagnostics.Debug.WriteLine(
                                $"ReservationStatusService: Reservation {reservation.Id} status updated to {newStatus}"
                            );
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine(
                    $"ReservationStatusService: {updatedCount} reservations status updated"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReservationStatusService Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Met à jour le statut d'une réservation spécifique
        /// </summary>
        public bool UpdateReservationStatus(int reservationId, ReservationStatut newStatus)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ReservationStatusService: Updating reservation {reservationId} to {newStatus}"
                );

                return _repositoryManager.Reservations.UpdateReservationStatus(reservationId, newStatus);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ReservationStatusService Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtient le statut attendu pour une réservation basé sur les dates
        /// </summary>
        public ReservationStatut GetExpectedStatus(Creneau? creneau)
        {
            if (creneau == null)
                return ReservationStatut.EnAttente;

            var now = DateTime.Now;

            if (now >= creneau.Fin)
                return ReservationStatut.Terminée;

            if (now >= creneau.Debut && now < creneau.Fin)
                return ReservationStatut.EnCours;

            return ReservationStatut.Confirmée;
        }
    }
}
