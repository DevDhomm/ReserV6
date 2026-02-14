# Système de Réservation de Salles - Guide Complet

## 📋 Vue d'ensemble

Le système de réservation de salles est une application complète conçue pour gérer les réservations de salles dans un établissement scolaire. Il permet aux utilisateurs de consulter les disponibilités, de réserver des salles, et aux gestionnaires de valider ou d'annuler les demandes.

## 🏗️ Architecture

### Structure du Projet

```
ReserV6/
├── Assets/
│   └── initialize_database.sql      # Script d'initialisation SQLite
├── Models/
│   └── ReservationSystemModels.cs   # Modèles de données
├── Services/
│   ├── Database/
│   │   ├── DatabaseService.cs       # Gestion de la base de données
│   │   └── Repositories/
│   │       ├── UserRepository.cs          # Gestion des utilisateurs
│   │       ├── SalleRepository.cs         # Gestion des salles
│   │       ├── ReservationRepository.cs   # Gestion des réservations
│   │       ├── EquipementRepository.cs    # Gestion des équipements
│   │       ├── CreneauRepository.cs       # Gestion des créneaux
│   │       ├── HistoriqueRepository.cs    # Gestion de l'historique
│   │       └── RepositoryManager.cs       # Gestionnaire centralisé
│   ├── ReservationService.cs        # Service métier
│   └── ReservationSystemInitializer.cs # Initialisation système
```

### Technologies utilisées

- **Framework**: .NET 10
- **Interface**: WPF (Windows Presentation Foundation)
- **Base de données**: SQLite avec Microsoft.Data.Sqlite
- **MVVM**: CommunityToolkit.Mvvm

## 🗄️ Modèles de Données

### Entités principales

#### User
- `Id` : Identifiant unique
- `Nom` : Nom complet de l'utilisateur
- `Email` : Adresse email unique
- `Role` : "User" ou "Admin"
- `DateCreation` : Date de création

#### Salle
- `Id` : Identifiant unique
- `Nom` : Nom unique de la salle
- `Capacite` : Nombre de places
- `Type` : "Amphithéâtre", "Laboratoire", "Salle de cours", etc.
- `Etage` : Numéro d'étage
- `Disponibilite` : Booléen (true/false)
- `DateCreation` : Date de création

#### Equipement
- `Id` : Identifiant unique
- `Nom` : Nom de l'équipement
- `Description` : Description optionnelle
- `Type` : Type d'équipement
- `EstFonctionnel` : État de fonctionnement (true/false)
- `SalleId` : Référence à la salle
- `DateCreation` : Date de création

#### Creneau
- `Id` : Identifiant unique
- `Debut` : Date/heure de début
- `Fin` : Date/heure de fin
- `DateCreation` : Date de création
- `Duree` : TimeSpan calculé (Fin - Debut)

#### Reservation
- `Id` : Identifiant unique
- `DateReservation` : Date de création de la réservation
- `Motif` : Raison de la réservation
- `Statut` : "EnAttente", "Confirmée", "Annulée", "Terminée"
- `UserId` : Référence à l'utilisateur
- `SalleId` : Référence à la salle
- `CreneauId` : Référence au créneau

#### Historique
- `Id` : Identifiant unique
- `Action` : Description de l'action
- `DateAction` : Date/heure de l'action
- `ReservationId` : Référence à la réservation

## 🚀 Utilisation

### Initialisation du Système

```csharp
// Créer une instance du service d'initialisation
var initializer = new ReservationSystemInitializer();

// Initialiser la base de données
initializer.Initialize();

// Obtenir les services
var repositories = initializer.GetRepositoryManager();
var reservationService = initializer.GetReservationService();

// Récupérer l'utilisateur par défaut
var defaultUser = initializer.GetDefaultUser();
```

### Créer une Réservation

```csharp
var reservationService = new ReservationService(repositories);

var result = reservationService.CreateReservation(
    userId: 1,
    salleId: 2,
    creneauId: 5,
    motif: "Cours de Programmation");

if (result.Success)
{
    Console.WriteLine($"Réservation créée: {result.ReservationId}");
}
else
{
    Console.WriteLine($"Erreur: {result.Message}");
}
```

### Annuler une Réservation

```csharp
var result = reservationService.CancelReservation(reservationId: 1);

if (result.Success)
{
    Console.WriteLine("Réservation annulée");
}
```

### Modifier une Réservation

```csharp
var result = reservationService.ModifyReservation(
    reservationId: 1,
    newSalleId: 3,
    newMotif: "Nouveau motif");

if (result.Success)
{
    Console.WriteLine("Réservation modifiée");
}
```

### Rechercher des Salles Disponibles

```csharp
var availableRooms = reservationService.FindAvailableRooms(
    debut: new DateTime(2026, 2, 12),
    fin: new DateTime(2026, 2, 12, 18, 0, 0),
    minCapacity: 30);

foreach (var room in availableRooms)
{
    Console.WriteLine($"Salle: {room.Nom}, Capacité: {room.Capacite}, Étage: {room.Etage}");
}
```

### Obtenir l'Historique d'une Réservation

```csharp
var history = reservationService.GetReservationHistory(reservationId: 1);

foreach (var (dateAction, action, userNom, salleName) in history)
{
    Console.WriteLine($"{dateAction}: {action} par {userNom} pour {salleName}");
}
```

### Statistiques d'Occupation

```csharp
var stats = reservationService.GetRoomStatistics(salleId: 2);
Console.WriteLine($"Total: {stats.TotalReservations}, Confirmées: {stats.Confirmed}, Taux: {stats.OccupancyRate}%");
```

## 📚 Repositories

Chaque repository fournit des méthodes spécifiques:

### UserRepository
- `GetAllUsers()` - Récupère tous les utilisateurs
- `GetUserById(int id)` - Récupère un utilisateur par ID
- `GetUserByEmail(string email)` - Récupère un utilisateur par email
- `AddUser(User user)` - Ajoute un nouvel utilisateur
- `UpdateUser(User user)` - Met à jour un utilisateur
- `DeleteUser(int id)` - Supprime un utilisateur
- `GetUserStatistics(int userId)` - Récupère les statistiques d'un utilisateur

### SalleRepository
- `GetAllSalles()` - Récupère toutes les salles
- `GetSalleById(int id)` - Récupère une salle par ID
- `GetAvailableSalles()` - Récupère les salles disponibles
- `GetSallesByFloor(int etage)` - Récupère les salles par étage
- `GetSallesByCapacity(int minCapacity)` - Récupère les salles par capacité
- `AddSalle(Salle salle)` - Ajoute une nouvelle salle
- `UpdateSalle(Salle salle)` - Met à jour une salle
- `DeleteSalle(int id)` - Supprime une salle
- `GetSallesWithEquipments()` - Récupère les salles avec leurs équipements
- `GetAvailableSallesForPeriod(DateTime debut, DateTime fin)` - Récupère les salles disponibles pour une période

### ReservationRepository
- `GetAllReservations()` - Récupère toutes les réservations
- `GetReservationById(int id)` - Récupère une réservation par ID
- `GetUserReservations(int userId)` - Récupère les réservations d'un utilisateur
- `GetSalleReservations(int salleId)` - Récupère les réservations d'une salle
- `CreateReservation(Reservation reservation)` - Crée une réservation
- `UpdateReservationStatus(int reservationId, ReservationStatut newStatus)` - Met à jour le statut
- `UpdateReservation(Reservation reservation)` - Met à jour une réservation
- `CancelReservation(int reservationId)` - Annule une réservation
- `DeleteReservation(int id)` - Supprime une réservation
- `GetCompleteReservations(string? statut)` - Récupère les réservations complètes
- `GetUpcomingConfirmedReservations(int days)` - Récupère les réservations futures confirmées
- `HasConflict(int salleId, int creneauId, int? excludeReservationId)` - Vérifie les conflits

### EquipementRepository
- `GetAllEquipements()` - Récupère tous les équipements
- `GetEquipementById(int id)` - Récupère un équipement par ID
- `GetSalleEquipements(int salleId)` - Récupère les équipements d'une salle
- `GetWorkingSalleEquipements(int salleId)` - Récupère les équipements fonctionnels
- `AddEquipement(Equipement equipement)` - Ajoute un équipement
- `UpdateEquipement(Equipement equipement)` - Met à jour un équipement
- `SetEquipementStatus(int id, bool estFonctionnel)` - Change l'état de l'équipement
- `DeleteEquipement(int id)` - Supprime un équipement

### CreneauRepository
- `GetAllCreneaux()` - Récupère tous les créneaux
- `GetCreneauById(int id)` - Récupère un créneau par ID
- `GetCreneauxByPeriod(DateTime debut, DateTime fin)` - Récupère les créneaux d'une période
- `GetAvailableCreneauxForSalle(int salleId)` - Récupère les créneaux disponibles
- `AddCreneau(Creneau creneau)` - Ajoute un créneau
- `UpdateCreneau(Creneau creneau)` - Met à jour un créneau
- `DeleteCreneau(int id)` - Supprime un créneau
- `HasOverlap(DateTime debut, DateTime fin, int? excludeCreneauId)` - Vérifie les chevauchements

### HistoriqueRepository
- `GetAllHistoriques()` - Récupère tous les historiques
- `GetReservationHistory(int reservationId)` - Récupère l'historique d'une réservation
- `AddHistorique(Historique historique)` - Ajoute un enregistrement historique
- `AddAction(int reservationId, string action)` - Ajoute une action à l'historique
- `DeleteReservationHistory(int reservationId)` - Supprime l'historique d'une réservation
- `GetDetailedReservationHistory(int reservationId)` - Récupère l'historique détaillé
- `GetUserHistory(int userId, int limit)` - Récupère l'historique d'un utilisateur

## 📊 Vues SQL

Le système fournit trois vues utiles:

### v_reservations_completes
Affiche toutes les réservations avec les détails des utilisateurs, salles et créneaux.

### v_salles_equipements
Affiche toutes les salles avec le nombre et la liste de leurs équipements.

### v_statistiques_utilisateurs
Affiche les statistiques de chaque utilisateur (nombre de réservations, heures totales, etc.).

## 🔐 Données Initiales

Par défaut, le système crée:

### Utilisateur par défaut
- **Nom**: Jean Dupont
- **Email**: jean.dupont@ecole.fr
- **Rôle**: User

### Salles prédéfinis (8 salles)
- Amphithéâtre A101 (150 places)
- Salles de Cours B201 et B202 (35 places chacune)
- Laboratoires C301 et C302 (25 places chacune)
- Salle de Séminaire D102 (50 places)
- Salle de Réunion E103 (15 places)
- Salle de Travail Collaboratif F104 (20 places)

### Créneaux prédéfinis
Des créneaux de 2 heures sont créés pour les jours de février 2026:
- 8h-10h, 10h-12h, 12h-14h, 14h-16h, 16h-18h

### Équipements
Chaque salle est équipée d'équipements appropriés (vidéoprojecteurs, tableaux interactifs, etc.)

### Réservations de démonstration
3 réservations confirmées par défaut pour tester le système.

## 🎯 Fonctionnalités Principales

1. **Gestion des Utilisateurs**
   - Création, modification, suppression d'utilisateurs
   - Distinction entre utilisateurs simples et administrateurs
   - Statistiques par utilisateur

2. **Gestion des Salles**
   - Création, modification, suppression de salles
   - Gestion de la disponibilité des salles
   - Recherche par étage, capacité ou type
   - Association d'équipements

3. **Gestion des Réservations**
   - Création de réservations (confirmées par défaut)
   - Annulation de réservations
   - Modification de réservations
   - Vérification automatique des conflits
   - Historique complet des modifications

4. **Gestion des Équipements**
   - Attribution d'équipements aux salles
   - Suivi de l'état fonctionnel
   - Association flexible entre équipements et salles

5. **Créneaux Flexibles**
   - Créneaux horaires prédéfinis ou personnalisés
   - Vérification des chevauchements
   - Recherche de créneaux disponibles

6. **Historique et Traçabilité**
   - Enregistrement de toutes les actions
   - Historique complet par réservation et par utilisateur
   - Dates/heures précises des modifications

## 📈 Requêtes SQL Avancées

Le fichier `initialize_database.sql` contient plus de 30 requêtes SQL avancées:

- Recherche de salles disponibles pour une période
- Détection de conflits de réservation
- Statistiques d'occupation par salle/étage
- Réservations multicolonnes
- Requêtes de notification
- Et bien d'autres...

## 💡 Exemples d'Utilisation Complète

```csharp
// Initialiser le système
var initializer = new ReservationSystemInitializer();
initializer.Initialize();
var repositories = initializer.GetRepositoryManager();
var reservationService = initializer.GetReservationService();

// Obtenir l'utilisateur par défaut
var user = repositories.Users.GetUserByEmail("jean.dupont@ecole.fr");

// Obtenir les salles disponibles
var availableSalles = repositories.Salles.GetAvailableSalles();

// Obtenir les créneaux disponibles pour une salle
var availableCreneaux = repositories.Creneaux.GetAvailableCreneauxForSalle(1);

// Créer une réservation
var (success, message, reservationId) = reservationService.CreateReservation(
    user!.Id,
    1,  // Salle
    1,  // Créneau
    "Réunion importante");

if (success)
{
    // Récupérer et afficher les détails
    var reservation = repositories.Reservations.GetReservationById(reservationId);
    var history = reservationService.GetReservationHistory(reservationId);
    
    Console.WriteLine($"Réservation créée: {reservation?.Motif}");
    foreach (var (date, action, userName, roomName) in history)
    {
        Console.WriteLine($"{date}: {action}");
    }
}
```

## 🔄 Cycle de Vie d'une Réservation

1. **Création** → La réservation est créée et immédiatement confirmée
2. **Modification** → Possible tant qu'elle n'est pas annulée ou terminée
3. **Utilisation** → La réservation couvre la période du créneau
4. **Terminaison** → Automatique après la fin du créneau
5. **Annulation** → Possible avant le créneau, enregistrée dans l'historique

## 📝 Notes

- La base de données SQLite est créée automatiquement au premier démarrage
- Tous les statuts de réservation, types de salles, etc., sont enregistrés en texte lisible
- Les transactions ACID assurent la cohérence des données
- Les indices de base de données optimisent les requêtes fréquentes
- Tous les timestamps utilisent le format ISO 8601
