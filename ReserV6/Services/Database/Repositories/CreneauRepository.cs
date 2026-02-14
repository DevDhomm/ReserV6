using Microsoft.Data.Sqlite;
using ReserV6.Models;

namespace ReserV6.Services.Database.Repositories
{
    /// <summary>
    /// Repository pour la gestion des créneaux horaires
    /// </summary>
    public class CreneauRepository
    {
        private readonly DatabaseService _dbService;

        public CreneauRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Récupère tous les créneaux
        /// </summary>
        public List<Creneau> GetAllCreneaux()
        {
            var creneaux = new List<Creneau>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, debut, fin, dateCreation
                        FROM Creneau
                        ORDER BY debut";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            creneaux.Add(MapCreneauFromReader(reader));
                        }
                    }
                }
            }

            return creneaux;
        }

        /// <summary>
        /// Récupère un créneau par ID
        /// </summary>
        public Creneau? GetCreneauById(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, debut, fin, dateCreation
                        FROM Creneau
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapCreneauFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Récupère les créneaux pour une période donnée
        /// </summary>
        public List<Creneau> GetCreneauxByPeriod(DateTime debut, DateTime fin)
        {
            var creneaux = new List<Creneau>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, debut, fin, dateCreation
                        FROM Creneau
                        WHERE debut >= @debut AND fin <= @fin
                        ORDER BY debut";
                    command.Parameters.AddWithValue("@debut", debut);
                    command.Parameters.AddWithValue("@fin", fin);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            creneaux.Add(MapCreneauFromReader(reader));
                        }
                    }
                }
            }

            return creneaux;
        }

        /// <summary>
        /// Récupère les créneaux disponibles pour une salle
        /// </summary>
        public List<Creneau> GetAvailableCreneauxForSalle(int salleId)
        {
            var creneaux = new List<Creneau>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT c.id, c.debut, c.fin, c.dateCreation
                        FROM Creneau c
                        WHERE c.id NOT IN (
                            SELECT creneau_id 
                            FROM Reservation 
                            WHERE salle_id = @salleId
                              AND statut IN ('En attente', 'Confirmée')
                        )
                        ORDER BY c.debut";
                    command.Parameters.AddWithValue("@salleId", salleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            creneaux.Add(MapCreneauFromReader(reader));
                        }
                    }
                }
            }

            return creneaux;
        }

        /// <summary>
        /// Ajoute un nouveau créneau
        /// </summary>
        public int AddCreneau(Creneau creneau)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Creneau (debut, fin, dateCreation)
                        VALUES (@debut, @fin, @dateCreation)";
                    command.Parameters.AddWithValue("@debut", creneau.Debut);
                    command.Parameters.AddWithValue("@fin", creneau.Fin);
                    command.Parameters.AddWithValue("@dateCreation", creneau.DateCreation);

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
        /// Met à jour un créneau existant
        /// </summary>
        public bool UpdateCreneau(Creneau creneau)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Creneau
                        SET debut = @debut, fin = @fin
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", creneau.Id);
                    command.Parameters.AddWithValue("@debut", creneau.Debut);
                    command.Parameters.AddWithValue("@fin", creneau.Fin);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Supprime un créneau
        /// </summary>
        public bool DeleteCreneau(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Creneau WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Vérifie si un créneau chevauche un autre
        /// </summary>
        public bool HasOverlap(DateTime debut, DateTime fin, int? excludeCreneauId = null)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT COUNT(*) as count FROM Creneau
                        WHERE debut < @fin AND fin > @debut";
                    
                    if (excludeCreneauId.HasValue)
                    {
                        command.CommandText += " AND id != @excludeId";
                        command.Parameters.AddWithValue("@excludeId", excludeCreneauId.Value);
                    }

                    command.Parameters.AddWithValue("@debut", debut);
                    command.Parameters.AddWithValue("@fin", fin);

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

        private Creneau MapCreneauFromReader(SqliteDataReader reader)
        {
            return new Creneau
            {
                Id = reader.GetInt32(0),
                Debut = DateTime.Parse(reader.GetString(1)),
                Fin = DateTime.Parse(reader.GetString(2)),
                DateCreation = DateTime.Parse(reader.GetString(3))
            };
        }
    }
}
