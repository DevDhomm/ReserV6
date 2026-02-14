using Microsoft.Data.Sqlite;
using ReserV6.Models;

namespace ReserV6.Services.Database.Repositories
{
    /// <summary>
    /// Repository pour la gestion des équipements
    /// </summary>
    public class EquipementRepository
    {
        private readonly DatabaseService _dbService;

        public EquipementRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Récupère tous les équipements
        /// </summary>
        public List<Equipement> GetAllEquipements()
        {
            var equipements = new List<Equipement>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, type, estFonctionnel, salle_id, dateCreation
                        FROM Equipement
                        ORDER BY salle_id, nom";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            equipements.Add(MapEquipementFromReader(reader));
                        }
                    }
                }
            }

            return equipements;
        }

        /// <summary>
        /// Récupère un équipement par ID
        /// </summary>
        public Equipement? GetEquipementById(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, type, estFonctionnel, salle_id, dateCreation
                        FROM Equipement
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapEquipementFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Récupère les équipements d'une salle
        /// </summary>
        public List<Equipement> GetSalleEquipements(int salleId)
        {
            var equipements = new List<Equipement>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, type, estFonctionnel, salle_id, dateCreation
                        FROM Equipement
                        WHERE salle_id = @salleId
                        ORDER BY nom";
                    command.Parameters.AddWithValue("@salleId", salleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            equipements.Add(MapEquipementFromReader(reader));
                        }
                    }
                }
            }

            return equipements;
        }

        /// <summary>
        /// Récupère les équipements fonctionnels d'une salle
        /// </summary>
        public List<Equipement> GetWorkingSalleEquipements(int salleId)
        {
            var equipements = new List<Equipement>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, type, estFonctionnel, salle_id, dateCreation
                        FROM Equipement
                        WHERE salle_id = @salleId AND estFonctionnel = 1
                        ORDER BY nom";
                    command.Parameters.AddWithValue("@salleId", salleId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            equipements.Add(MapEquipementFromReader(reader));
                        }
                    }
                }
            }

            return equipements;
        }

        /// <summary>
        /// Ajoute un nouvel équipement
        /// </summary>
        public int AddEquipement(Equipement equipement)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Equipement (nom, description, type, estFonctionnel, salle_id, dateCreation)
                        VALUES (@nom, @description, @type, @estFonctionnel, @salle_id, @dateCreation)";
                    command.Parameters.AddWithValue("@nom", equipement.Nom);
                    command.Parameters.AddWithValue("@description", equipement.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@type", equipement.Type);
                    command.Parameters.AddWithValue("@estFonctionnel", equipement.EstFonctionnel ? 1 : 0);
                    command.Parameters.AddWithValue("@salle_id", equipement.SalleId);
                    command.Parameters.AddWithValue("@dateCreation", equipement.DateCreation);

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
        /// Met à jour un équipement existant
        /// </summary>
        public bool UpdateEquipement(Equipement equipement)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Equipement
                        SET nom = @nom, description = @description, type = @type, 
                            estFonctionnel = @estFonctionnel, salle_id = @salle_id
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", equipement.Id);
                    command.Parameters.AddWithValue("@nom", equipement.Nom);
                    command.Parameters.AddWithValue("@description", equipement.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@type", equipement.Type);
                    command.Parameters.AddWithValue("@estFonctionnel", equipement.EstFonctionnel ? 1 : 0);
                    command.Parameters.AddWithValue("@salle_id", equipement.SalleId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Change l'état fonctionnel d'un équipement
        /// </summary>
        public bool SetEquipementStatus(int id, bool estFonctionnel)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Equipement
                        SET estFonctionnel = @estFonctionnel
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);
                    command.Parameters.AddWithValue("@estFonctionnel", estFonctionnel ? 1 : 0);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Supprime un équipement
        /// </summary>
        public bool DeleteEquipement(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Equipement WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private Equipement MapEquipementFromReader(SqliteDataReader reader)
        {
            return new Equipement
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Type = reader.GetString(3),
                EstFonctionnel = reader.GetInt32(4) == 1,
                SalleId = reader.GetInt32(5),
                DateCreation = DateTime.Parse(reader.GetString(6))
            };
        }
    }
}
