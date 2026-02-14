using Microsoft.Data.Sqlite;
using ReserV6.Models;

namespace ReserV6.Services.Database.Repositories
{
    /// <summary>
    /// Repository pour la gestion des utilisateurs
    /// </summary>
    public class UserRepository
    {
        private readonly DatabaseService _dbService;

        public UserRepository(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, email, role, dateCreation 
                        FROM ""User""
                        ORDER BY nom";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(MapUserFromReader(reader));
                        }
                    }
                }
            }

            return users;
        }

        /// <summary>
        /// Récupère un utilisateur par ID
        /// </summary>
        public User? GetUserById(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, email, role, dateCreation 
                        FROM ""User""
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUserFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Récupère un utilisateur par email
        /// </summary>
        public User? GetUserByEmail(string email)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT id, nom, email, role, dateCreation 
                        FROM ""User""
                        WHERE email = @email";
                    command.Parameters.AddWithValue("@email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapUserFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Ajoute un nouvel utilisateur
        /// </summary>
        public int AddUser(User user)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO ""User"" (nom, email, role, dateCreation)
                        VALUES (@nom, @email, @role, @dateCreation)";
                    command.Parameters.AddWithValue("@nom", user.Nom);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@role", user.Role);
                    command.Parameters.AddWithValue("@dateCreation", user.DateCreation);

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
        /// Met à jour un utilisateur existant
        /// </summary>
        public bool UpdateUser(User user)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE ""User""
                        SET nom = @nom, email = @email, role = @role
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", user.Id);
                    command.Parameters.AddWithValue("@nom", user.Nom);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@role", user.Role);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Supprime un utilisateur
        /// </summary>
        public bool DeleteUser(int id)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM \"User\" WHERE id = @id";
                    command.Parameters.AddWithValue("@id", id);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Récupère les statistiques d'un utilisateur
        /// </summary>
        public StatistiquesUtilisateur? GetUserStatistics(int userId)
        {
            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT * FROM v_statistiques_utilisateurs
                        WHERE id = @id";
                    command.Parameters.AddWithValue("@id", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new StatistiquesUtilisateur
                            {
                                Id = reader.GetInt32(0),
                                Nom = reader.GetString(1),
                                Email = reader.GetString(2),
                                Role = reader.GetString(3),
                                TotalReservations = reader.GetInt32(4),
                                Confirmees = reader.GetInt32(5),
                                EnAttente = reader.GetInt32(6),
                                Annulees = reader.GetInt32(7),
                                HeuresTotales = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Récupère les statistiques de tous les utilisateurs
        /// </summary>
        public List<StatistiquesUtilisateur> GetAllUserStatistics()
        {
            var stats = new List<StatistiquesUtilisateur>();

            using (var connection = _dbService.GetConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT * FROM v_statistiques_utilisateurs
                        WHERE total_reservations > 0
                        ORDER BY total_reservations DESC";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stats.Add(new StatistiquesUtilisateur
                            {
                                Id = reader.GetInt32(0),
                                Nom = reader.GetString(1),
                                Email = reader.GetString(2),
                                Role = reader.GetString(3),
                                TotalReservations = reader.GetInt32(4),
                                Confirmees = reader.GetInt32(5),
                                EnAttente = reader.GetInt32(6),
                                Annulees = reader.GetInt32(7),
                                HeuresTotales = reader.IsDBNull(8) ? 0 : reader.GetInt32(8)
                            });
                        }
                    }
                }
            }

            return stats;
        }

        private User MapUserFromReader(SqliteDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32(0),
                Nom = reader.GetString(1),
                Email = reader.GetString(2),
                Role = reader.GetString(3),
                DateCreation = DateTime.Parse(reader.GetString(4))
            };
        }
    }
}
