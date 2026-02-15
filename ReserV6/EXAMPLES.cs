// Exemples pratiques d'utilisation du système de réservation
// À utiliser dans votre ViewModel ou Code-Behind

using ReserV6.Models;
using ReserV6.Services;

public class ReservationExamples
{
    private readonly ReservationSystemInitializer _initializer;
    private readonly RepositoryManager _repositories;
    private readonly ReservationService _reservationService;

    public ReservationExamples()
    {
        _initializer = new ReservationSystemInitializer();
        _initializer.Initialize();
        _repositories = _initializer.GetRepositoryManager();
        _reservationService = _initializer.GetReservationService();
    }

    // ============================================
    // EXEMPLE 1: Lister toutes les salles disponibles
    // ============================================
    public void Example_ListAvailableRooms()
    {
        var availableRooms = _repositories.Salles.GetAvailableSalles();
        
        foreach (var room in availableRooms)
        {
            Console.WriteLine($"Salle: {room.Nom}");
            Console.WriteLine($"  Capacité: {room.Capacite} places");
            Console.WriteLine($"  Type: {room.Type}");
            Console.WriteLine($"  Étage: {room.Etage}");
            Console.WriteLine($"  ---");
        }
    }

    // ============================================
    // EXEMPLE 2: Voir les équipements d'une salle
    // ============================================
    public void Example_GetRoomEquipments(int roomId)
    {
        var equipments = _repositories.Equipements.GetSalleEquipements(roomId);
        var room = _repositories.Salles.GetSalleById(roomId);

        Console.WriteLine($"Équipements de {room?.Nom}:");
        foreach (var equipment in equipments)
        {
            string status = equipment.EstFonctionnel ? "✓ Fonctionnel" : "✗ Non fonctionnel";
            Console.WriteLine($"  - {equipment.Nom} ({equipment.Type}) [{status}]");
        }
    }

    // ============================================
    // EXEMPLE 3: Créer une réservation simple
    // ============================================
    public void Example_CreateSimpleReservation()
    {
        var defaultUser = _initializer.GetDefaultUser();
        
        if (defaultUser == null)
        {
            Console.WriteLine("Utilisateur par défaut non trouvé!");
            return;
        }

        // Créer une réservation
        var (success, message, reservationId) = _reservationService.CreateReservation(
            userId: defaultUser.Id,
            salleId: 1,  // Amphithéâtre A101
            creneauId: 1, // 08:00-10:00
            motif: "Cours de Programmation C#");

        if (success)
        {
            Console.WriteLine($"✓ Réservation créée avec succès! (ID: {reservationId})");
        }
        else
        {
            Console.WriteLine($"✗ Erreur: {message}");
        }
    }

    // ============================================
    // EXEMPLE 4: Afficher toutes les réservations
    // ============================================
    public void Example_ListAllReservations()
    {
        var reservations = _repositories.Reservations.GetCompleteReservations();

        Console.WriteLine("Toutes les réservations:");
        Console.WriteLine("═══════════════════════════════════════════════════════════");
        
        foreach (var res in reservations)
        {
            Console.WriteLine($"ID: {res.Id}");
            Console.WriteLine($"Utilisateur: {res.UserNom} ({res.UserEmail})");
            Console.WriteLine($"Salle: {res.SalleNom} (Étage {res.Etage}, Capacité: {res.Capacite})");
            Console.WriteLine($"Période: {res.CreneauDebut:dd/MM/yyyy HH:mm} - {res.CreneauFin:dd/MM/yyyy HH:mm}");
            Console.WriteLine($"Durée: {res.DureeHeures} heures");
            Console.WriteLine($"Motif: {res.Motif}");
            Console.WriteLine($"Statut: {res.Statut}");
            Console.WriteLine("───────────────────────────────────────────────────────────");
        }
    }

    // ============================================
    // EXEMPLE 5: Annuler une réservation
    // ============================================
    public void Example_CancelReservation(int reservationId)
    {
        var reservation = _repositories.Reservations.GetReservationById(reservationId);
        if (reservation == null)
        {
            Console.WriteLine("Réservation non trouvée!");
            return;
        }

        Console.WriteLine($"Annulation de la réservation ID {reservationId}");
        Console.WriteLine($"Salle: {_repositories.Salles.GetSalleById(reservation.SalleId)?.Nom}");

        var (success, message) = _reservationService.CancelReservation(reservationId);

        if (success)
        {
            Console.WriteLine($"✓ {message}");
        }
        else
        {
            Console.WriteLine($"✗ {message}");
        }
    }

    // ============================================
    // EXEMPLE 6: Modifier une réservation
    // ============================================
    public void Example_ModifyReservation(int reservationId, int newRoomId)
    {
        var reservation = _repositories.Reservations.GetReservationById(reservationId);
        if (reservation == null)
        {
            Console.WriteLine("Réservation non trouvée!");
            return;
        }

        var oldRoom = _repositories.Salles.GetSalleById(reservation.SalleId);
        var newRoom = _repositories.Salles.GetSalleById(newRoomId);

        Console.WriteLine($"Modification de la réservation ID {reservationId}");
        Console.WriteLine($"Ancienne salle: {oldRoom?.Nom}");
        Console.WriteLine($"Nouvelle salle: {newRoom?.Nom}");

        var (success, message) = _reservationService.ModifyReservation(
            reservationId: reservationId,
            newSalleId: newRoomId,
            newMotif: $"Modifiée vers {newRoom?.Nom}");

        if (success)
        {
            Console.WriteLine($"✓ {message}");
        }
        else
        {
            Console.WriteLine($"✗ {message}");
        }
    }

    // ============================================
    // EXEMPLE 7: Rechercher des salles disponibles
    // ============================================
    public void Example_FindAvailableRooms()
    {
        DateTime from = new DateTime(2026, 2, 12, 10, 0, 0);
        DateTime to = new DateTime(2026, 2, 12, 18, 0, 0);
        int minCapacity = 30;

        Console.WriteLine($"Recherche de salles disponibles:");
        Console.WriteLine($"  Du: {from:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"  Au: {to:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"  Capacité min: {minCapacity} places");
        Console.WriteLine("───────────────────────────────────────────────────────────");

        var availableRooms = _reservationService.FindAvailableRooms(from, to, minCapacity);

        if (availableRooms.Count == 0)
        {
            Console.WriteLine("Aucune salle disponible pour ces critères.");
        }
        else
        {
            foreach (var room in availableRooms)
            {
                Console.WriteLine($"✓ {room.Nom}");
                Console.WriteLine($"  Capacité: {room.Capacite} places");
                Console.WriteLine($"  Type: {room.Type}");
                Console.WriteLine($"  Étage: {room.Etage}");
                Console.WriteLine($"  Équipements: {(string.IsNullOrEmpty(room.Equipements) ? "Aucun" : room.Equipements)}");
                Console.WriteLine();
            }
        }
    }

    // ============================================
    // EXEMPLE 8: Afficher l'historique d'une réservation
    // ============================================
    public void Example_ShowReservationHistory(int reservationId)
    {
        var reservation = _repositories.Reservations.GetReservationById(reservationId);
        if (reservation == null)
        {
            Console.WriteLine("Réservation non trouvée!");
            return;
        }

        var room = _repositories.Salles.GetSalleById(reservation.SalleId);
        Console.WriteLine($"Historique de la réservation {reservationId}");
        Console.WriteLine($"Salle: {room?.Nom}");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        var history = _reservationService.GetReservationHistory(reservationId);

        if (history.Count == 0)
        {
            Console.WriteLine("Aucun historique.");
        }
        else
        {
            foreach (var (dateAction, action, userNom, salleName) in history)
            {
                Console.WriteLine($"[{dateAction:dd/MM/yyyy HH:mm:ss}] {action}");
                Console.WriteLine($"  Utilisateur: {userNom}");
                Console.WriteLine($"  Salle: {salleName}");
            }
        }
    }

    // ============================================
    // EXEMPLE 9: Statistiques utilisateur
    // ============================================
    public void Example_UserStatistics(int userId)
    {
        var user = _repositories.Users.GetUserById(userId);
        if (user == null)
        {
            Console.WriteLine("Utilisateur non trouvé!");
            return;
        }

        var stats = _repositories.Users.GetUserStatistics(userId);

        Console.WriteLine($"Statistiques de {stats?.Nom}");
        Console.WriteLine($"Email: {stats?.Email}");
        Console.WriteLine($"Rôle: {stats?.Role}");
        Console.WriteLine("───────────────────────────────────────────────────────────");
        Console.WriteLine($"Total des réservations: {stats?.TotalReservations}");
        Console.WriteLine($"  - Confirmées: {stats?.Confirmees}");
        Console.WriteLine($"  - En attente: {stats?.EnAttente}");
        Console.WriteLine($"  - Annulées: {stats?.Annulees}");
        Console.WriteLine($"Heures totales réservées: {stats?.HeuresTotales}h");
    }

    // ============================================
    // EXEMPLE 10: Statistiques d'occupation d'une salle
    // ============================================
    public void Example_RoomStatistics(int roomId)
    {
        var room = _repositories.Salles.GetSalleById(roomId);
        if (room == null)
        {
            Console.WriteLine("Salle non trouvée!");
            return;
        }

        var stats = _reservationService.GetRoomStatistics(roomId);

        Console.WriteLine($"Statistiques d'occupation: {room.Nom}");
        Console.WriteLine("───────────────────────────────────────────────────────────");
        Console.WriteLine($"Total des réservations: {stats.TotalReservations}");
        Console.WriteLine($"Réservations confirmées: {stats.Confirmed}");
        Console.WriteLine($"Taux de confirmation: {stats.OccupancyRate:F2}%");
    }

    // ============================================
    // EXEMPLE 11: Lister les réservations par étage
    // ============================================
    public void Example_ReservationsByFloor(int floor)
    {
        var rooms = _repositories.Salles.GetSallesByFloor(floor);
        
        Console.WriteLine($"Salles à l'étage {floor}:");
        Console.WriteLine("═══════════════════════════════════════════════════════════");

        foreach (var room in rooms)
        {
            var reservations = _repositories.Reservations.GetSalleReservations(room.Id);
            Console.WriteLine($"\n{room.Nom} ({room.Capacite} places):");

            if (reservations.Count == 0)
            {
                Console.WriteLine("  Aucune réservation");
            }
            else
            {
                foreach (var res in reservations)
                {
                    var user = _repositories.Users.GetUserById(res.UserId);
                    var startTime = res.DateTimeDebut;
                    Console.WriteLine($"  [{res.Statut}] {startTime:dd/MM HH:mm} - {user?.Nom}");
                }
            }
        }
    }

    // ============================================
    // EXEMPLE 12: Ajouter une nouvelle salle
    // ============================================
    public void Example_AddNewRoom()
    {
        var newRoom = new Salle
        {
            Nom = "Salle de Réunion VIP",
            Capacite = 20,
            Type = "Salle de réunion",
            Etage = 2,
            Disponibilite = true,
            DateCreation = DateTime.Now
        };

        int roomId = _repositories.Salles.AddSalle(newRoom);
        
        Console.WriteLine($"✓ Nouvelle salle créée!");
        Console.WriteLine($"  ID: {roomId}");
        Console.WriteLine($"  Nom: {newRoom.Nom}");
        Console.WriteLine($"  Capacité: {newRoom.Capacite} places");
        Console.WriteLine($"  Étage: {newRoom.Etage}");
    }

    // ============================================
    // EXEMPLE 13: Ajouter un équipement à une salle
    // ============================================
    public void Example_AddEquipmentToRoom(int roomId)
    {
        var newEquipment = new Equipement
        {
            Nom = "Écran tactile interactif",
            Description = "Écran 75 pouces avec système tactile",
            Type = "Affichage interactif",
            EstFonctionnel = true,
            SalleId = roomId,
            DateCreation = DateTime.Now
        };

        int equipmentId = _repositories.Equipements.AddEquipement(newEquipment);
        
        Console.WriteLine($"✓ Équipement ajouté!");
        Console.WriteLine($"  ID: {equipmentId}");
        Console.WriteLine($"  Nom: {newEquipment.Nom}");
        Console.WriteLine($"  Salle ID: {roomId}");
    }

    // ============================================
    // EXEMPLE 14: Ajouter un nouveau créneau
    // ============================================
    public void Example_AddNewTimeSlot()
    {
        var newCreneau = new Creneau
        {
            Debut = new DateTime(2026, 2, 16, 18, 0, 0),
            Fin = new DateTime(2026, 2, 16, 20, 0, 0),
            DateCreation = DateTime.Now
        };

        int creneauId = _repositories.Creneaux.AddCreneau(newCreneau);
        
        Console.WriteLine($"✓ Nouveau créneau créé!");
        Console.WriteLine($"  ID: {creneauId}");
        Console.WriteLine($"  De: {newCreneau.Debut:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"  À: {newCreneau.Fin:dd/MM/yyyy HH:mm}");
    }

    // ============================================
    // EXEMPLE 15: Créer un nouvel utilisateur
    // ============================================
    public void Example_AddNewUser()
    {
        var newUser = new User
        {
            Nom = "Marie Martin",
            Email = "marie.martin@ecole.fr",
            Role = "User",
            DateCreation = DateTime.Now
        };

        int userId = _repositories.Users.AddUser(newUser);
        
        Console.WriteLine($"✓ Nouvel utilisateur créé!");
        Console.WriteLine($"  ID: {userId}");
        Console.WriteLine($"  Nom: {newUser.Nom}");
        Console.WriteLine($"  Email: {newUser.Email}");
        Console.WriteLine($"  Rôle: {newUser.Role}");
    }
}
