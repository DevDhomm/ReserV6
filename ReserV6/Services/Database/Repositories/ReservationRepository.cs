using Microsoft.Data.Sqlite;
using ReserV6.Models;

namespace ReserV6.Services.Database.Repositories
{
    /// <summary>
    /// Repository pour la gestion des réservations
    /// </summary>
    public class ReservationRepository
    {
        private readonly DatabaseService _dbService;

        public ReservationRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Récupère toutes les réservations
        /// </summary>
        public List<Reservation> GetAllReservations()
        {
            var reservations = new List<Reservation>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, dateReservation, motif, statut, user_id, salle_id, creneau_id, dateDebut, dateFin, heureDebut, heureFin
                        FROM Reservation
                        ORDER BY dateDebut DESC";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservations.Add(MapReservationFromReader(reader));
                        }
                    }
                }
            }

            return reservations;
        }

        /// <summary>
        /// Récupère une réservation par ID
        /// </summary>
        public Reservation? GetReservationById(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, dateReservation, motif, statut, user_id, salle_id, creneau_id, dateDebut, dateFin, heureDebut, heureFin
                        FROM Reservation
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReservationFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Récupère les réservations d'un utilisateur
        /// </summary>
        public List<Reservation> GetUserReservations(int userId)
        {
            var reservations = new List<Reservation>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, dateReservation, motif, statut, user_id, salle_id, creneau_id, dateDebut, dateFin, heureDebut, heureFin
                        FROM Reservation
                        WHERE user_id = @userId
                        ORDER BY dateDebut DESC";
                    command.Parameters.AddWithValue("@userId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservations.Add(MapReservationFromReader(reader));
                        }
                    }
                }
            }

            return reservations;
        }

        /// <summary>
        /// Récupère les réservations d'une salle
        /// </summary>
        public List<Reservation> GetSalleReservations(int salleId)
        {
            var reservations = new List<Reservation>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, dateReservation, motif, statut, user_id, salle_id, creneau_id, dateDebut, dateFin, heureDebut, heureFin
                        FROM Reservation
                        WHERE salle_id = @salleId
                        ORDER BY dateDebut DESC";
                    command.Parameters.AddWithValue("@salleId", salleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservations.Add(MapReservationFromReader(reader));
                        }
                    }
                }
            }

            return reservations;
        }

        /// <summary>
        /// Crée une nouvelle réservation
        /// </summary>
        public int CreateReservation(Reservation reservation)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Reservation (dateReservation, motif, statut, user_id, salle_id, creneau_id, dateDebut, dateFin, heureDebut, heureFin)
                        VALUES (@dateReservation, @motif, @statut, @user_id, @salle_id, @creneau_id, @dateDebut, @dateFin, @heureDebut, @heureFin)";
                    command.Parameters.AddWithValue("@dateReservation", reservation.DateReservation);
                    command.Parameters.AddWithValue("@motif", reservation.Motif ?? string.Empty);
                    command.Parameters.AddWithValue("@statut", reservation.Statut.ToString());
                    command.Parameters.AddWithValue("@user_id", reservation.UserId);
                    command.Parameters.AddWithValue("@salle_id", reservation.SalleId);
                    command.Parameters.AddWithValue("@creneau_id", reservation.CreneauId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@dateDebut", reservation.DateDebut);
                    command.Parameters.AddWithValue("@dateFin", reservation.DateFin);
                    command.Parameters.AddWithValue("@heureDebut", reservation.HeureDebut.ToString(@"hh\:mm\:ss"));
                    command.Parameters.AddWithValue("@heureFin", reservation.HeureFin.ToString(@"hh\:mm\:ss"));

                    command.ExecuteNonQuery();
                    using (var lastIdCommand = connection.CreateCommand())
                    {
                        lastIdCommand.CommandText = "SELECT last_insert_rowid()";
                        return (int)(long)lastIdCommand.ExecuteScalar()!;
                    }
                }
            }
        }

        /// <summary>
        /// Met à jour le statut d'une réservation
        /// </summary>
        public bool UpdateReservationStatus(int reservationId, ReservationStatut newStatus)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Reservation
                        SET statut = @statut
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", reservationId);
                    command.Parameters.AddWithValue("@statut", newStatus.ToString());

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Met à jour une réservation complète
        /// </summary>
        public bool UpdateReservation(Reservation reservation)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Reservation
                        SET motif = @motif, statut = @statut, salle_id = @salle_id, creneau_id = @creneau_id, dateDebut = @dateDebut, dateFin = @dateFin, heureDebut = @heureDebut, heureFin = @heureFin
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", reservation.Id);
                    command.Parameters.AddWithValue("@motif", reservation.Motif ?? string.Empty);
                    command.Parameters.AddWithValue("@statut", reservation.Statut.ToString());
                    command.Parameters.AddWithValue("@salle_id", reservation.SalleId);
                    command.Parameters.AddWithValue("@creneau_id", reservation.CreneauId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@dateDebut", reservation.DateDebut);
                    command.Parameters.AddWithValue("@dateFin", reservation.DateFin);
                    command.Parameters.AddWithValue("@heureDebut", reservation.HeureDebut.ToString(@"hh\:mm\:ss"));
                    command.Parameters.AddWithValue("@heureFin", reservation.HeureFin.ToString(@"hh\:mm\:ss"));

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Annule une réservation
        /// </summary>
        public bool CancelReservation(int reservationId)
        {
            return UpdateReservationStatus(reservationId, ReservationStatut.Annulée);
        }

        /// <summary>
        /// Supprime une réservation
        /// </summary>
        public bool DeleteReservation(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Reservation WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Récupère les réservations complètes (avec tous les détails)
        /// </summary>
        public List<ReservationComplete> GetCompleteReservations(string? statut = null)
        {
            var reservations = new List<ReservationComplete>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    string query = "SELECT * FROM v_reservations_completes";
                    if (!string.IsNullOrEmpty(statut))
                    {
                        query += " WHERE statut = @statut";
                    }
                    query += " ORDER BY creneau_debut DESC";

                    command.CommandText = query;
                    if (!string.IsNullOrEmpty(statut))
                    {
                        command.Parameters.AddWithValue("@statut", statut);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservations.Add(MapReservationCompleteFromReader(reader));
                        }
                    }
                }
            }

            return reservations;
        }

        /// <summary>
        /// Récupère les réservations futures confirmées
        /// </summary>
        public List<ReservationComplete> GetUpcomingConfirmedReservations(int days = 1)
        {
            var reservations = new List<ReservationComplete>();
            var futureDate = DateTime.Now.AddDays(days);

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT * FROM v_reservations_completes
                        WHERE statut = 'Confirmée'
                          AND creneau_debut BETWEEN DATETIME('now') AND @futureDate
                        ORDER BY creneau_debut";
                    command.Parameters.AddWithValue("@futureDate", futureDate);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservations.Add(MapReservationCompleteFromReader(reader));
                        }
                    }
                }
            }

            return reservations;
        }

        /// <summary>
        /// Vérifie s'il y a un conflit de réservation
        /// </summary>
        public bool HasConflict(int salleId, int creneauId, int? excludeReservationId = null)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) as count FROM Reservation
                        WHERE salle_id = @salleId
                          AND creneau_id = @creneauId
                          AND statut IN ('EnAttente', 'Confirmée')";

                    if (excludeReservationId.HasValue)
                    {
                        command.CommandText += " AND id != @excludeId";
                        command.Parameters.AddWithValue("@excludeId", excludeReservationId.Value);
                    }

                    command.Parameters.AddWithValue("@salleId", salleId);
                    command.Parameters.AddWithValue("@creneauId", creneauId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0) > 0;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Vérifie s'il y a un conflit de réservation sur une plage horaire personnalisée
        /// </summary>
        public bool HasTimeConflict(int salleId, DateTime startTime, DateTime endTime, int? excludeReservationId = null)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) as count 
                        FROM Reservation r
                        WHERE r.salle_id = @salleId
                          AND r.statut IN ('EnAttente', 'Confirmée')
                          AND NOT (
                            (r.dateFin < @startDate) OR
                            (r.dateDebut > @endDate) OR
                            (r.dateDebut = @startDate AND TIME(r.heureFin) <= TIME(@startTime)) OR
                            (r.dateFin = @endDate AND TIME(r.heureDebut) >= TIME(@endTime)) OR
                            (r.dateDebut = @startDate AND r.dateFin = @startDate AND (TIME(r.heureFin) <= TIME(@startTime) OR TIME(r.heureDebut) >= TIME(@endTime)))
                          )";

                    if (excludeReservationId.HasValue)
                    {
                        command.CommandText += " AND r.id != @excludeId";
                        command.Parameters.AddWithValue("@excludeId", excludeReservationId.Value);
                    }

                    command.Parameters.AddWithValue("@salleId", salleId);
                    command.Parameters.AddWithValue("@startDate", startTime.Date);
                    command.Parameters.AddWithValue("@endDate", endTime.Date);
                    command.Parameters.AddWithValue("@startTime", startTime.TimeOfDay.ToString(@"hh\:mm\:ss"));
                    command.Parameters.AddWithValue("@endTime", endTime.TimeOfDay.ToString(@"hh\:mm\:ss"));

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetInt32(0) > 0;
                        }
                    }
                }
            }

            return false;
        }

        private Reservation MapReservationFromReader(SqliteDataReader reader)
        {
            return new Reservation
            {
                Id = reader.GetInt32(0),
                DateReservation = DateTime.Parse(reader.GetString(1)),
                Motif = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Statut = ParseReservationStatut(reader.GetString(3)),
                UserId = reader.GetInt32(4),
                SalleId = reader.GetInt32(5),
                CreneauId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                DateDebut = DateTime.Parse(reader.GetString(7)),
                DateFin = DateTime.Parse(reader.GetString(8)),
                HeureDebut = TimeSpan.Parse(reader.GetString(9)),
                HeureFin = TimeSpan.Parse(reader.GetString(10))
            };
        }

        private ReservationComplete MapReservationCompleteFromReader(SqliteDataReader reader)
        {
            return new ReservationComplete
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                DateReservation = DateTime.Parse(reader.GetString(reader.GetOrdinal("dateReservation"))),
                Motif = reader.IsDBNull(reader.GetOrdinal("motif")) ? string.Empty : reader.GetString(reader.GetOrdinal("motif")),
                Statut = reader.GetString(reader.GetOrdinal("statut")),
                UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                UserNom = reader.GetString(reader.GetOrdinal("user_nom")),
                UserEmail = reader.GetString(reader.GetOrdinal("user_email")),
                SalleId = reader.GetInt32(reader.GetOrdinal("salle_id")),
                SalleNom = reader.GetString(reader.GetOrdinal("salle_nom")),
                Capacite = reader.GetInt32(reader.GetOrdinal("capacite")),
                SalleType = reader.GetString(reader.GetOrdinal("salle_type")),
                Etage = reader.GetInt32(reader.GetOrdinal("etage")),
                DateDebut = DateTime.Parse(reader.GetString(reader.GetOrdinal("dateDebut"))),
                DateFin = DateTime.Parse(reader.GetString(reader.GetOrdinal("dateFin"))),
                HeureDebut = TimeSpan.Parse(reader.GetString(reader.GetOrdinal("heureDebut"))),
                HeureFin = TimeSpan.Parse(reader.GetString(reader.GetOrdinal("heureFin")))
            };
        }

        private ReservationStatut ParseReservationStatut(string statut)
        {
            return statut switch
            {
                "En attente" => ReservationStatut.EnAttente,
                "Confirmée" => ReservationStatut.Confirmée,
                "Annulée" => ReservationStatut.Annulée,
                "Terminée" => ReservationStatut.Terminée,
                _ => ReservationStatut.EnAttente
            };
        }
    }
}
