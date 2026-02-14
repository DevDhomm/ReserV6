# 🏫 Système de Réservation de Salles - ReserV6

Un système complet de gestion et de réservation de salles pour les établissements scolaires, construit avec .NET 10 et WPF.

## ✨ Caractéristiques

### 🎯 Fonctionnalités Principales
- **Gestion des Utilisateurs** : Utilisateurs simples (demandeurs) et administrateurs
- **Gestion des Salles** : Création, modification, gestion de disponibilité
- **Gestion des Équipements** : Attribution à des salles avec suivi de l'état fonctionnel
- **Créneaux Horaires** : Prédéfinis ou personnalisés avec vérification des chevauchements
- **Réservations** : Création, modification, annulation avec historique complet
- **Historique** : Traçabilité complète de toutes les actions
- **Statistiques** : Occupation des salles, activité utilisateur, taux de confirmation

### 🏗️ Architecture
- **Framework** : .NET 10 avec WPF
- **Base de Données** : SQLite avec Microsoft.Data.Sqlite
- **MVVM** : CommunityToolkit.Mvvm
- **Pattern** : Repository Pattern avec gestion centralisée

### 💾 Structure de la Base de Données

```
Tables principales:
├── User (Utilisateurs)
├── Salle (Salles réservables)
├── Equipement (Équipements)
├── Creneau (Créneaux horaires)
├── Reservation (Réservations)
└── Historique (Journal des actions)

Vues:
├── v_reservations_completes
├── v_salles_equipements
└── v_statistiques_utilisateurs

Indices pour optimisation des requêtes
```

## 🚀 Démarrage Rapide

### Installation et Initialisation

```csharp
// 1. Créer une instance du système
var initializer = new ReservationSystemInitializer();

// 2. Initialiser la base de données (création automatique)
initializer.Initialize();

// 3. Obtenir les services
var repositories = initializer.GetRepositoryManager();
var reservationService = initializer.GetReservationService();
```

### Créer une Réservation

```csharp
var (success, message, reservationId) = reservationService.CreateReservation(
    userId: 1,
    salleId: 2,
    creneauId: 5,
    motif: "Cours de Programmation");

if (success)
    Console.WriteLine($"Réservation créée: {reservationId}");
```

### Rechercher des Salles Disponibles

```csharp
var availableRooms = reservationService.FindAvailableRooms(
    debut: DateTime.Now,
    fin: DateTime.Now.AddDays(1),
    minCapacity: 30);

foreach (var room in availableRooms)
    Console.WriteLine($"{room.Nom} ({room.Capacite} places)");
```

## 📚 Composants Principaux

### Services
- **ReservationService** : Logique métier pour les réservations
- **DatabaseService** : Gestion de la connexion et initialisation
- **RepositoryManager** : Accès centralisé aux repositories

### Repositories
- **UserRepository** : Gestion des utilisateurs
- **SalleRepository** : Gestion des salles
- **ReservationRepository** : Gestion des réservations
- **EquipementRepository** : Gestion des équipements
- **CreneauRepository** : Gestion des créneaux
- **HistoriqueRepository** : Gestion de l'historique

### Modèles
- **User** : Représentation d'un utilisateur
- **Salle** : Représentation d'une salle
- **Equipement** : Représentation d'un équipement
- **Creneau** : Représentation d'un créneau horaire
- **Reservation** : Représentation d'une réservation
- **Historique** : Représentation d'une action historique
- **DTOs** : ReservationComplete, SalleAvecEquipements, StatistiquesUtilisateur

## 📊 Données Initiales

Lors de la première initialisation, le système crée :

### Utilisateur par défaut
- **Jean Dupont** (jean.dupont@ecole.fr)

### 8 Salles
- Amphithéâtre A101 (150 places)
- Salle de Cours B201 (35 places)
- Salle de Cours B202 (35 places)
- Laboratoire C301 (25 places)
- Laboratoire C302 (25 places)
- Salle de Séminaire D102 (50 places)
- Salle de Réunion E103 (15 places)
- Salle de Travail Collaboratif F104 (20 places)

### 15 Créneaux horaires
- 08:00-10:00, 10:00-12:00, 12:00-14:00, 14:00-16:00, 16:00-18:00
- Pour 3 jours (12, 13, 14 février 2026)

### 11 Équipements
- Vidéoprojecteurs, tableaux interactifs, caméras HD, etc.

### 3 Réservations de démonstration
- Confirmées et prêtes à être utilisées

## 🎯 Cas d'Usage Courants

### Lister toutes les réservations
```csharp
var allReservations = repositories.Reservations.GetCompleteReservations();
```

### Obtenir les réservations d'un utilisateur
```csharp
var userReservations = repositories.Reservations.GetUserReservations(userId);
```

### Vérifier les conflits
```csharp
bool hasConflict = repositories.Reservations.HasConflict(salleId, creneauId);
```

### Obtenir les statistiques
```csharp
var stats = repositories.Users.GetUserStatistics(userId);
var roomStats = reservationService.GetRoomStatistics(salleId);
```

### Ajouter une nouvelle salle
```csharp
var newRoom = new Salle { ... };
int roomId = repositories.Salles.AddSalle(newRoom);
```

## 🔍 Requêtes SQL Avancées

Le fichier `initialize_database.sql` contient plus de 30 requêtes utiles :
- Recherche de disponibilités
- Statistiques d'occupation
- Détection de conflits
- Requêtes de notification
- Analyses multicolonnes

Voir `SYSTEM_GUIDE.md` pour plus de détails.

## 📝 Configuration

### Emplacement de la Base de Données

Par défaut, la base de données SQLite est créée à :
```
%AppData%/ReserV6/ReservationSystem.db
```

Vous pouvez customiser :
```csharp
var dbService = new DatabaseService(
    databaseDirectory: "C:/MyCustomPath",
    databaseFileName: "MyDatabase.db");
```

### Importer le Script d'Initialisation

Le script `Assets/initialize_database.sql` est utilisé automatiquement lors de la création de la base de données.

## 🔐 Sécurité

- Utilisation de paramètres SQL pour éviter les injections
- Transactions ACID pour la cohérence des données
- Validation des données à l'application
- Gestion appropriée des erreurs

## 📈 Performance

- Indices de base de données optimisés
- Lazy loading des repositories
- Utilisation efficace des connexions
- Vues SQL pour les requêtes complexes

## 📖 Documentation Complète

Consultez :
- **SYSTEM_GUIDE.md** : Guide détaillé du système
- **EXAMPLES.cs** : 15 exemples pratiques complets
- Code inline commenté en français

## 🛠️ Technologies

- **.NET 10** - Framework
- **WPF** - Interface utilisateur
- **SQLite** - Base de données
- **Microsoft.Data.Sqlite** - Pilote SQLite
- **CommunityToolkit.Mvvm** - Pattern MVVM
- **WPF-UI** - Composants UI modernes

## 📋 État des Réservations

- **EnAttente** : En attente de confirmation
- **Confirmée** : Confirmée et réservée (défaut pour les nouvelles)
- **Annulée** : Annulée, salle libérée
- **Terminée** : Automatiquement après le créneau

## 🎓 Exemple Complet

```csharp
// Initialiser
var initializer = new ReservationSystemInitializer();
initializer.Initialize();
var repositories = initializer.GetRepositoryManager();
var service = initializer.GetReservationService();

// Obtenir l'utilisateur par défaut
var user = initializer.GetDefaultUser();

// Lister les salles
var rooms = repositories.Salles.GetAvailableSalles();

// Créer une réservation
var (ok, msg, id) = service.CreateReservation(
    user!.Id, rooms[0].Id, 1, "Réunion");

// Voir l'historique
var history = service.GetReservationHistory(id);
foreach (var (date, action, name, room) in history)
    Console.WriteLine($"{date}: {action}");

// Statistiques
var stats = service.GetRoomStatistics(rooms[0].Id);
Console.WriteLine($"Taux: {stats.OccupancyRate}%");
```

## 🐛 Dépannage

### Base de données non trouvée
→ Vérifiez que `initialize_database.sql` existe dans le dossier `Assets`

### Erreurs de connexion
→ Vérifiez les permissions d'accès au dossier `%AppData%`

### Conflits de réservation non détectés
→ Vérifiez le chevauchement exact des créneaux

## 📞 Support

Pour des exemples supplémentaires et des cas d'usage, consultez `EXAMPLES.cs`.

## 📄 Licence

Ce projet est fourni en tant que système de réservation pour établissements scolaires.

---

**Dernière mise à jour** : 2026-02-12
**Version** : 1.0
**Framework** : .NET 10
