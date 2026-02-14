using Microsoft.Data.Sqlite;
using ReserV6.Models;

namespace ReserV6.Services.Database.Repositories
{
    /// <summary>
    /// Repository pour la gestion de l'historique des réservations
    /// </summary>
    public class HistoriqueRepository
    {
        private readonly DatabaseService _dbService;

        public HistoriqueRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Récupère tous les historiques
        /// </summary>
        public List<Historique> GetAllHistoriques()
        {
            var historiques = new List<Historique>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, action, dateAction, reservation_id
                        FROM Historique
                        ORDER BY dateAction DESC";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            historiques.Add(MapHistoriqueFromReader(reader));
                        }
                    }
                }
            }

            return historiques;
        }

        /// <summary>
        /// Récupère l'historique d'une réservation
        /// </summary>
        public List<Historique> GetReservationHistory(int reservationId)
        {
            var historiques = new List<Historique>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, action, dateAction, reservation_id
                        FROM Historique
                        WHERE reservation_id = @reservationId
                        ORDER BY dateAction DESC";
                    command.Parameters.AddWithValue("@reservationId", reservationId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            historiques.Add(MapHistoriqueFromReader(reader));
                        }
                    }
                }
            }

            return historiques;
        }

        /// <summary>
        /// Ajoute un nouvel enregistrement historique
        /// </summary>
        public int AddHistorique(Historique historique)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Historique (action, dateAction, reservation_id)
                        VALUES (@action, @dateAction, @reservation_id)";
                    command.Parameters.AddWithValue("@action", historique.Action);
                    command.Parameters.AddWithValue("@dateAction", historique.DateAction);
                    command.Parameters.AddWithValue("@reservation_id", historique.ReservationId);

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
        /// Ajoute une action à l'historique d'une réservation
        /// </summary>
        public int AddAction(int reservationId, string action)
        {
            var historique = new Historique
            {
                Action = action,
                DateAction = DateTime.Now,
                ReservationId = reservationId
            };

            return AddHistorique(historique);
        }

        /// <summary>
        /// Supprime l'historique d'une réservation
        /// </summary>
        public bool DeleteReservationHistory(int reservationId)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Historique WHERE reservation_id = @reservationId";
                    command.Parameters.AddWithValue("@reservationId", reservationId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Récupère l'historique complet d'une réservation avec détails
        /// </summary>
        public List<(DateTime DateAction, string Action, string UserNom, string SalleName)> GetDetailedReservationHistory(int reservationId)
        {
            var history = new List<(DateTime DateAction, string Action, string UserNom, string SalleName)>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            h.dateAction,
                            h.action,
                            u.nom,
                            s.nom
                        FROM Historique h
                        JOIN Reservation r ON h.reservation_id = r.id
                        JOIN ""User"" u ON r.user_id = u.id
                        JOIN Salle s ON r.salle_id = s.id
                        WHERE r.id = @reservationId
                        ORDER BY h.dateAction DESC";
                    command.Parameters.AddWithValue("@reservationId", reservationId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add((
                                DateTime.Parse(reader.GetString(0)),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3)
                            ));
                        }
                    }
                }
            }

            return history;
        }

        /// <summary>
        /// Récupère l'historique d'un utilisateur
        /// </summary>
        public List<(DateTime DateAction, string Action, string SalleName, string Motif)> GetUserHistory(int userId, int limit = 20)
        {
            var history = new List<(DateTime DateAction, string Action, string SalleName, string Motif)>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT 
                            h.dateAction,
                            h.action,
                            s.nom,
                            r.motif
                        FROM Historique h
                        JOIN Reservation r ON h.reservation_id = r.id
                        JOIN Salle s ON r.salle_id = s.id
                        WHERE r.user_id = @userId
                        ORDER BY h.dateAction DESC
                        LIMIT @limit";
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@limit", limit);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            history.Add((
                                DateTime.Parse(reader.GetString(0)),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                            ));
                        }
                    }
                }
            }

            return history;
        }

        private Historique MapHistoriqueFromReader(SqliteDataReader reader)
        {
            return new Historique
            {
                Id = reader.GetInt32(0),
                Action = reader.GetString(1),
                DateAction = DateTime.Parse(reader.GetString(2)),
                ReservationId = reader.GetInt32(3)
            };
        }
    }
}
