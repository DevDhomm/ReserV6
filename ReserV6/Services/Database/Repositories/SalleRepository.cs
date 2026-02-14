using Microsoft.Data.Sqlite;
using ReserV6.Models;

namespace ReserV6.Services.Database.Repositories
{
    /// <summary>
    /// Repository pour la gestion des salles
    /// </summary>
    public class SalleRepository
    {
        private readonly DatabaseService _dbService;

        public SalleRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Récupère toutes les salles avec leurs équipements
        /// </summary>
        public List<Salle> GetAllSalles()
        {
            var salles = new List<Salle>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, capacite, type, etage, disponibilite, dateCreation
                        FROM Salle
                        ORDER BY etage, nom";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salles.Add(MapSalleFromReader(reader));
                        }
                    }
                }

                // Load equipements for each salle
                var equipementRepository = new EquipementRepository(_dbService);
                foreach (var salle in salles)
                {
                    var equipements = equipementRepository.GetSalleEquipements(salle.Id);
                    salle.Equipements = equipements;
                }
            }

            return salles;
        }

        /// <summary>
        /// Récupère une salle par ID avec ses équipements
        /// </summary>
        public Salle? GetSalleById(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, capacite, type, etage, disponibilite, dateCreation
                        FROM Salle
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var salle = MapSalleFromReader(reader);

                            // Load equipements for this salle
                            var equipementRepository = new EquipementRepository(_dbService);
                            var equipements = equipementRepository.GetSalleEquipements(salle.Id);
                            salle.Equipements = equipements;

                            return salle;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Récupère les salles disponibles avec leurs équipements
        /// </summary>
        public List<Salle> GetAvailableSalles()
        {
            var salles = new List<Salle>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, capacite, type, etage, disponibilite, dateCreation
                        FROM Salle
                        WHERE disponibilite = 1
                        ORDER BY etage, nom";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salles.Add(MapSalleFromReader(reader));
                        }
                    }
                }

                // Load equipements for each salle
                var equipementRepository = new EquipementRepository(_dbService);
                foreach (var salle in salles)
                {
                    var equipements = equipementRepository.GetSalleEquipements(salle.Id);
                    salle.Equipements = equipements;
                }
            }

            return salles;
        }

        /// <summary>
        /// Récupère les salles par étage avec leurs équipements
        /// </summary>
        public List<Salle> GetSallesByFloor(int etage)
        {
            var salles = new List<Salle>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, capacite, type, etage, disponibilite, dateCreation
                        FROM Salle
                        WHERE etage = @etage
                        ORDER BY nom";
                    command.Parameters.AddWithValue("@etage", etage);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salles.Add(MapSalleFromReader(reader));
                        }
                    }
                }

                // Load equipements for each salle
                var equipementRepository = new EquipementRepository(_dbService);
                foreach (var salle in salles)
                {
                    var equipements = equipementRepository.GetSalleEquipements(salle.Id);
                    salle.Equipements = equipements;
                }
            }

            return salles;
        }

        /// <summary>
        /// Récupère les salles avec capacité minimale avec leurs équipements
        /// </summary>
        public List<Salle> GetSallesByCapacity(int minCapacity)
        {
            var salles = new List<Salle>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, description, capacite, type, etage, disponibilite, dateCreation
                        FROM Salle
                        WHERE disponibilite = 1 AND capacite >= @minCapacity
                        ORDER BY etage, capacite";
                    command.Parameters.AddWithValue("@minCapacity", minCapacity);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salles.Add(MapSalleFromReader(reader));
                        }
                    }
                }

                // Load equipements for each salle
                var equipementRepository = new EquipementRepository(_dbService);
                foreach (var salle in salles)
                {
                    var equipements = equipementRepository.GetSalleEquipements(salle.Id);
                    salle.Equipements = equipements;
                }
            }

            return salles;
        }

        /// <summary>
        /// Ajoute une nouvelle salle
        /// </summary>
        public int AddSalle(Salle salle)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Salle (nom, capacite, type, etage, disponibilite, dateCreation)
                        VALUES (@nom, @capacite, @type, @etage, @disponibilite, @dateCreation)";
                    command.Parameters.AddWithValue("@nom", salle.Nom);
                    command.Parameters.AddWithValue("@capacite", salle.Capacite);
                    command.Parameters.AddWithValue("@type", salle.Type);
                    command.Parameters.AddWithValue("@etage", salle.Etage);
                    command.Parameters.AddWithValue("@disponibilite", salle.Disponibilite ? 1 : 0);
                    command.Parameters.AddWithValue("@dateCreation", salle.DateCreation);

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
        /// Met à jour une salle existante
        /// </summary>
        public bool UpdateSalle(Salle salle)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Salle
                        SET nom = @nom, description = @description, capacite = @capacite, type = @type, etage = @etage, disponibilite = @disponibilite
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", salle.Id);
                    command.Parameters.AddWithValue("@nom", salle.Nom);
                    command.Parameters.AddWithValue("@description", salle.Description ?? string.Empty);
                    command.Parameters.AddWithValue("@capacite", salle.Capacite);
                    command.Parameters.AddWithValue("@type", salle.Type);
                    command.Parameters.AddWithValue("@etage", salle.Etage);
                    command.Parameters.AddWithValue("@disponibilite", salle.Disponibilite ? 1 : 0);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Supprime une salle
        /// </summary>
        public bool DeleteSalle(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Salle WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Récupère les salles avec leurs équipements
        /// </summary>
        public List<SalleAvecEquipements> GetSallesWithEquipments()
        {
            var salles = new List<SalleAvecEquipements>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT * FROM v_salles_equipements
                        ORDER BY etage, nom";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salles.Add(new SalleAvecEquipements
                            {
                                Id = reader.GetInt32(0),
                                Nom = reader.GetString(1),
                                Capacite = reader.GetInt32(2),
                                Type = reader.GetString(3),
                                Etage = reader.GetInt32(4),
                                Disponibilite = reader.GetInt32(5) == 1,
                                NbEquipementsFonctionnels = reader.GetInt32(6),
                                NbEquipementsTotal = reader.GetInt32(7),
                                Equipements = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
                            });
                        }
                    }
                }
            }

            return salles;
        }

        /// <summary>
        /// Récupère les salles disponibles pour une période donnée avec leurs équipements
        /// </summary>
        public List<Salle> GetAvailableSallesForPeriod(DateTime debut, DateTime fin)
        {
            var salles = new List<Salle>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT DISTINCT s.id, s.nom, s.description, s.capacite, s.type, s.etage, s.disponibilite, s.dateCreation
                        FROM Salle s
                        WHERE s.disponibilite = 1
                          AND s.id NOT IN (
                            SELECT r.salle_id 
                            FROM Reservation r
                            JOIN Creneau c ON r.creneau_id = c.id
                            WHERE r.statut IN ('En attente', 'Confirmée')
                              AND c.debut < @fin
                              AND c.fin > @debut
                          )
                        ORDER BY s.etage, s.nom";
                    command.Parameters.AddWithValue("@debut", debut);
                    command.Parameters.AddWithValue("@fin", fin);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            salles.Add(MapSalleFromReader(reader));
                        }
                    }
                }

                // Load equipements for each salle
                var equipementRepository = new EquipementRepository(_dbService);
                foreach (var salle in salles)
                {
                    var equipements = equipementRepository.GetSalleEquipements(salle.Id);
                    salle.Equipements = equipements;
                }
            }

            return salles;
        }

        private Salle MapSalleFromReader(SqliteDataReader reader)
        {
            return new Salle
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nom = reader.GetString(reader.GetOrdinal("nom")),
                Description = reader.IsDBNull(reader.GetOrdinal("description")) ? string.Empty : reader.GetString(reader.GetOrdinal("description")),
                Capacite = reader.GetInt32(reader.GetOrdinal("capacite")),
                Type = reader.GetString(reader.GetOrdinal("type")),
                Etage = reader.GetInt32(reader.GetOrdinal("etage")),
                Disponibilite = reader.GetInt32(reader.GetOrdinal("disponibilite")) == 1,
                DateCreation = DateTime.Parse(reader.GetString(reader.GetOrdinal("dateCreation")))
            };
        }
    }
}
