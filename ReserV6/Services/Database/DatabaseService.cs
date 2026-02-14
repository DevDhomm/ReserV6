using Microsoft.Data.Sqlite;

namespace ReserV6.Services.Database
{
    /// <summary>
    /// Service de gestion de la connexion et de l'initialisation de la base de données SQLite
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _dbPath;
        private readonly string _initScriptPath;

        public DatabaseService(string databaseDirectory = "", string databaseFileName = "ReservationSystem.db")
        {
            string dbDir = string.IsNullOrEmpty(databaseDirectory) 
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReserV6")
                : databaseDirectory;

            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            _dbPath = Path.Combine(dbDir, databaseFileName);
            _connectionString = $"Data Source={_dbPath};";

            // Chemin vers le script d'initialisation
            _initScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "initialize_database.sql");
        }

        /// <summary>
        /// Initialise la base de données si elle n'existe pas
        /// </summary>
        public void InitializeDatabase()
        {
            bool dbExists = File.Exists(_dbPath);

            if (!dbExists)
            {
                // Créer la base de données et initialiser le schéma
                CreateDatabase();
            }
            else
            {
                // Exécuter les migrations si la base de données existe
                RunMigrations();
            }
        }

        /// <summary>
        /// Crée la base de données et exécute le script d'initialisation
        /// </summary>
        private void CreateDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                if (File.Exists(_initScriptPath))
                {
                    string initScript = File.ReadAllText(_initScriptPath);
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = initScript;
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }

        /// <summary>
        /// Exécute les migrations de schéma nécessaires
        /// </summary>
        private void RunMigrations()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                // Migration 1 : Ajouter la colonne description à la table Salle si elle n'existe pas
                AddDescriptionColumnToSalleIfNotExists(connection);

                connection.Close();
            }
        }

        /// <summary>
        /// Ajoute la colonne description à la table Salle si elle n'existe pas
        /// </summary>
        private void AddDescriptionColumnToSalleIfNotExists(SqliteConnection connection)
        {
            // Vérifier si la colonne existe
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA table_info(Salle)";
                using (var reader = command.ExecuteReader())
                {
                    bool columnExists = false;
                    while (reader.Read())
                    {
                        string columnName = reader.GetString(1);
                        if (columnName == "description")
                        {
                            columnExists = true;
                            break;
                        }
                    }

                    // Si la colonne n'existe pas, l'ajouter
                    if (!columnExists)
                    {
                        using (var alterCommand = connection.CreateCommand())
                        {
                            alterCommand.CommandText = "ALTER TABLE Salle ADD COLUMN description TEXT";
                            alterCommand.ExecuteNonQuery();
                            System.Diagnostics.Debug.WriteLine("✓ Colonne 'description' ajoutée à la table Salle");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtient une connexion à la base de données
        /// </summary>
        public SqliteConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        /// <summary>
        /// Obtient la chaîne de connexion
        /// </summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// Obtient le chemin de la base de données
        /// </summary>
        public string DatabasePath => _dbPath;

        /// <summary>
        /// Vérifie si la base de données existe
        /// </summary>
        public bool DatabaseExists => File.Exists(_dbPath);

        /// <summary>
        /// Réinitialise la base de données (pour les tests)
        /// </summary>
        public void ResetDatabase()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }

            InitializeDatabase();
        }
    }

    /// <summary>
    /// Classe pour gérer les transactions de base de données
    /// </summary>
    public class DbTransaction : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly SqliteTransaction? _transaction;

        public DbTransaction(string connectionString)
        {
            _connection = new SqliteConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public SqliteConnection Connection => _connection;
        public SqliteTransaction? Transaction => _transaction;

        public void Commit()
        {
            _transaction?.Commit();
        }

        public void Rollback()
        {
            _transaction?.Rollback();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
    }
}
