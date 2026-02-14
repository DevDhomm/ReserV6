# 🗺️ Guide de Navigation - Système de Réservation

## 📍 Où Trouver Quoi

### 🚀 Pour Démarrer Rapidement
**Fichier à lire** → `README.md`
- Guide d'utilisation rapide
- Démarrage en 5 minutes
- Exemples simples

### 📚 Pour Comprendre le Système
**Fichier à lire** → `SYSTEM_GUIDE.md`
- Architecture complète
- Documentation détaillée
- Toutes les APIs
- SQL avancé

### 💡 Pour Voir des Exemples
**Fichier à lire** → `EXAMPLES.cs`
- 15 cas d'usage pratiques
- Code copy-paste ready
- Du simple au complexe

### 📊 Pour Voir ce qui est Implémenté
**Fichier à lire** → `IMPLEMENTATION_SUMMARY.md`
- Checklist complète
- Statistiques du projet
- Résumé des fichiers

---

## 🧭 Parcours par Rôle

### 👨‍💻 Développeur WPF
```
1. Lisez : README.md (5 min)
2. Regardez : EXAMPLES.cs (20 min)
3. Utilisez : ReservationService + RepositoryManager (dans vos ViewModels)
4. Consultez : SYSTEM_GUIDE.md (si besoin d'API spécifique)
```

### 🏗️ Architecte
```
1. Lisez : SYSTEM_GUIDE.md (architecture complète)
2. Parcourez : Models/ReservationSystemModels.cs
3. Examinez : Services/Database/DatabaseService.cs
4. Analysez : Assets/initialize_database.sql (schema)
```

### 🔍 Code Reviewer
```
1. Vérifiez : BUILD STATUS (succès)
2. Reviewez : Repositories (pattern cohérent)
3. Testez : Conflits de réservation
4. Validez : Transactions et historique
```

### 📖 Documentation
```
1. Consultez : IMPLEMENTATION_SUMMARY.md
2. Copiez/Adaptez : EXAMPLES.cs
3. Présentez : PROJECT_SUMMARY.txt
```

---

## 📁 Structure des Fichiers

```
ReserV6/
│
├── 📖 Documentation (lire en 1er)
│   ├── README.md ............................. Démarrage rapide ⭐
│   ├── SYSTEM_GUIDE.md ....................... Documentation complète ⭐⭐
│   ├── EXAMPLES.cs ........................... 15 Exemples ⭐⭐⭐
│   ├── IMPLEMENTATION_SUMMARY.md ............. Résumé technique
│   └── PROJECT_SUMMARY.txt ................... Navigation
│
├── 💾 Database (initialisation auto)
│   └── Assets/initialize_database.sql ........ Schema SQLite
│
├── 🧬 Models (entités du domaine)
│   └── Models/ReservationSystemModels.cs ..... 10 classes
│
├── 🗄️ Data Access (Repository Pattern)
│   ├── Services/Database/DatabaseService.cs ..... Gestion DB
│   └── Services/Database/Repositories/ .......... 6 Repositories
│       ├── UserRepository.cs
│       ├── SalleRepository.cs
│       ├── ReservationRepository.cs
│       ├── EquipementRepository.cs
│       ├── CreneauRepository.cs
│       ├── HistoriqueRepository.cs
│       └── RepositoryManager.cs ............... Façade
│
├── 🎯 Business Logic (Services)
│   ├── Services/ReservationService.cs ........ Service métier
│   └── Services/ReservationSystemInitializer.cs Initialisation
│
├── ⚙️ Configuration
│   ├── ReserV6.csproj ....................... NuGet packages
│   └── Usings.cs ............................ Global usings
│
└── 🚀 Prêt à l'emploi!
```

---

## 🎯 Cas d'Usage Typiques

### 1️⃣ "Je veux créer une réservation"
```
📄 EXAMPLES.cs → Exemple 3: CreateSimpleReservation
✅ Service : ReservationService.CreateReservation()
📖 Docs : SYSTEM_GUIDE.md → "Créer une Réservation"
```

### 2️⃣ "Je veux chercher les salles disponibles"
```
📄 EXAMPLES.cs → Exemple 7: FindAvailableRooms
✅ Service : ReservationService.FindAvailableRooms()
📖 Docs : SYSTEM_GUIDE.md → "Recherches Avancées"
```

### 3️⃣ "Je veux voir les statistiques"
```
📄 EXAMPLES.cs → Exemples 9-10
✅ Services : GetUserStatistics(), GetRoomStatistics()
📖 Docs : SYSTEM_GUIDE.md → "Statistiques"
```

### 4️⃣ "Je veux lister les réservations"
```
📄 EXAMPLES.cs → Exemple 4: ListAllReservations
✅ Method : repositories.Reservations.GetCompleteReservations()
📖 Docs : SYSTEM_GUIDE.md → "Repositories"
```

### 5️⃣ "Je veux modifier une réservation"
```
📄 EXAMPLES.cs → Exemples 5-6
✅ Service : ReservationService.ModifyReservation()
✅ Service : ReservationService.CancelReservation()
📖 Docs : SYSTEM_GUIDE.md → "Cycle de Vie"
```

---

## 🔧 Points d'Intégration

### Pour Utiliser dans Votre Application

#### 1️⃣ Au Démarrage
```csharp
// App.xaml.cs ou Startup
var initializer = new ReservationSystemInitializer();
initializer.Initialize();
var service = initializer.GetReservationService();
// Stocker dans un ContentResolver ou Singleton
```

#### 2️⃣ Dans Vos ViewModels
```csharp
public class MyViewModel : ObservableObject
{
    private readonly ReservationService _service;
    
    public MyViewModel(ReservationService service)
    {
        _service = service;
    }
    
    [RelayCommand]
    public void CreateReservation()
    {
        var (ok, msg, id) = _service.CreateReservation(
            userId: 1, salleId: 2, creneauId: 5, 
            motif: "Meeting");
    }
}
```

#### 3️⃣ Dans Vos Pages
```xaml
<local:MyPage DataContext="{Binding MyViewModel, Source={StaticResource Locator}}"/>
```

---

## 📊 Flux de Données

```
Application (UI)
    ↓
ViewModel (binding)
    ↓
ReservationService (logique)
    ↓
RepositoryManager (accès)
    ├── UserRepository
    ├── SalleRepository
    ├── ReservationRepository
    ├── EquipementRepository
    ├── CreneauRepository
    └── HistoriqueRepository
    ↓
DatabaseService (connexion)
    ↓
SQLite Database
```

---

## 🆘 Besoin d'Aide?

| Question | Réponse |
|----------|---------|
| **Comment commencer?** | → Lire `README.md` |
| **Comment créer une réservation?** | → Voir `EXAMPLES.cs` #3 |
| **Comment ajouter une salle?** | → Voir `EXAMPLES.cs` #12 |
| **Comment chercher salles dispo?** | → Voir `EXAMPLES.cs` #7 |
| **Comment voir l'historique?** | → Voir `EXAMPLES.cs` #8 |
| **Quelle méthode appeler?** | → Chercher dans `SYSTEM_GUIDE.md` |
| **Où est la classe XYZ?** | → Voir `IMPLEMENTATION_SUMMARY.md` |
| **Quel est le schema DB?** | → Voir `Assets/initialize_database.sql` |

---

## ✨ Points Forts

- ✅ **Tous les packages Microsoft** - Cohérent avec .NET
- ✅ **Initialisation automatique** - Base de données créée seule
- ✅ **Données de démo** - Salles, créneaux, équipements prêts
- ✅ **15 Exemples** - Code copy-paste ready
- ✅ **Documentation complète** - En français, avec explications
- ✅ **Production-ready** - Validations, transactions, erreurs
- ✅ **Extensible** - Pattern Repository facile à adapter

---

## 🚀 Prochaines Étapes

1. **Lire** → `README.md` (5 min)
2. **Copier** → Un exemple de `EXAMPLES.cs`
3. **Tester** → Dans votre application
4. **Adapter** → Selon vos besoins
5. **Consulter** → `SYSTEM_GUIDE.md` si questions

---

## 📞 Résum Technique Rapide

| Aspect | Détail |
|--------|--------|
| **Framework** | .NET 10 Windows |
| **DB** | SQLite (Auto-Init) |
| **Pattern** | Repository + Service |
| **Entities** | 6 tables + 3 vues |
| **Repositories** | 6 (60+ méthodes) |
| **Status Build** | ✅ Succès |
| **Packages** | Microsoft.Data.Sqlite |

---

**Bonne intégration! 🎉**
