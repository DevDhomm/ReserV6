# 📅 Nouvelle Fonctionnalité: Sélection de Date Libre et Horaires Personnalisés

## 🎯 Objectif

Permettre à l'utilisateur de réserver une salle en:
1. **Sélectionnant n'importe quelle date future** (pas limitée aux créneaux existants)
2. **Choisissant ses propres horaires** (heure de début et de fin)
3. **Vérifiant automatiquement les conflits** avec les réservations existantes

## ✨ Changements Implémentés

### 1. Interface Utilisateur Améliorée

**Avant:**
- ComboBox limité aux dates avec créneaux pré-définis
- Impossible de sélectionner d'autres dates

**Après:**
- ✅ **DatePicker** permettant de sélectionner n'importe quelle date >= aujourd'hui
- ✅ **CheckBox** "Utiliser des horaires personnalisés"
- ✅ **Champs d'heure** pour saisir l'heure de début et fin (HH:mm)
- ✅ **Affichage conditionnel** des créneaux pré-définis selon le mode

### 2. Vue Utilisateur

```
┌─────────────────────────────────────────┐
│ Sélectionnez une date: [  DatePicker  ] │
│                        (Toutes les dates│
│                         futures sont    │
│                         disponibles)    │
├─────────────────────────────────────────┤
│ ☑ Utiliser des horaires personnalisés   │
│                                         │
│ Heure de début:                         │
│ De [09:00] (HH:mm)                      │
│                                         │
│ Heure de fin:                           │
│ À [10:00] (HH:mm)                       │
└─────────────────────────────────────────┘
```

### 3. Logique de Réservation

**Mode 1: Créneaux Pré-définis (défaut)**
```
1. User choisit date
2. Système affiche créneaux disponibles
3. User sélectionne un créneau
4. Système vérifie les conflits
5. Création de réservation avec CreneauId
```

**Mode 2: Horaires Personnalisés (nouveau)**
```
1. User active "Utiliser des horaires personnalisés"
2. User saisit heure début et fin
3. Système vérifie les conflits avec HasTimeConflict()
4. Système crée un Creneau dynamiquement
5. Création de réservation avec le nouveau CreneauId
```

## 🔄 Changements de Code

### ViewModels/Windows/ReservationDialogViewModel.cs

**Nouvelles propriétés:**
```csharp
// Horaires personnalisés
[ObservableProperty]
private TimeSpan _customStartTime = new TimeSpan(9, 0, 0);

[ObservableProperty]
private TimeSpan _customEndTime = new TimeSpan(10, 0, 0);

[ObservableProperty]
private bool _useCustomTime = false;

[ObservableProperty]
private DateTime? _minimumDate = DateTime.Today;
```

**Améliorations:**
- `LoadCreneaux()` : Génère 365 jours futurs au lieu de dépendre des créneaux
- `CreateReservation()` : Supporte deux modes (pré-défini et personnalisé)
- Support de création dynamique de Creneau

### Views/Windows/ReservationDialogWindow.xaml

**Nouvelles composantes:**
- `DatePicker` : Sélection libre de la date
- `CheckBox` : Toggle mode horaires personnalisés
- `TextBox` x2 : Saisie heures début/fin
- Visibilité conditionnelle avec `InverseBoolToVisibilityConverter`

### Converters/ReservationConverters.cs (Nouveau)

**Deux nouveaux convertisseurs:**
1. `InverseBoolToVisibilityConverter` : Affiche/cache selon booléen inversé
2. `TimeSpanToStringConverter` : Convertit TimeSpan ↔ string (HH:mm)

## 🚀 Comment Ça Marche

### Flux pour Mode Personnalisé

```
User clique DatePicker
    ↓
Choisit date future (ex: 25/01/2025)
    ↓
Coche "Utiliser des horaires personnalisés"
    ↓
Champs de temps s'affichent
    ↓
Saisit heure début: 14:00
    ↓
Saisit heure fin: 15:30
    ↓
Système calcule:
  - DateTime début = 25/01/2025 14:00
  - DateTime fin = 25/01/2025 15:30
    ↓
Clique "Confirmer"
    ↓
Système vérifie HasTimeConflict():
  - Cherche réservations chevauchantes
  - Si conflit → Affiche erreur
  - Si OK → Crée Creneau + Reservation
```

## 📊 Données

### Création Dynamique de Créneau

```csharp
// Créé automatiquement si horaires personnalisés
var creneau = new Creneau
{
    Debut = 2025-01-25 14:00:00,
    Fin = 2025-01-25 15:30:00,
    DateCreation = DateTime.Now
};

// Inséré dans DB
int creneauId = repository.Creneaux.AddCreneau(creneau);

// Utilisé pour la réservation
var reservation = new Reservation
{
    ...
    CreneauId = creneauId  // ID du nouveau créneau
};
```

## ✅ Validations

### Date
- ✅ Minimum = Aujourd'hui
- ✅ Aucune limite maximum
- ✅ Sélectable dans le DatePicker

### Heures
- ✅ Format HH:mm requis
- ✅ Heure début < Heure fin (validé avant création)
- ✅ Champ texte avec placeholder "(HH:mm)"

### Conflits
- ✅ Vérification avec `HasTimeConflict()`
- ✅ Détecte chevauchements partiels
- ✅ Affiche message d'erreur détaillé

## 🎨 Interface Visuelle

### Quand "Utiliser des horaires personnalisés" est DÉCOCHÉ:
```
✓ DatePicker visible
✗ Champs de temps cachés
✓ Créneaux pré-définis affichés
✓ Bouton "Sélectionner" sur chaque créneau
```

### Quand "Utiliser des horaires personnalisés" est COCHÉ:
```
✓ DatePicker visible
✓ Champs de temps visibles
✗ Créneaux pré-définis cachés
✗ Bouton "Sélectionner" invisible
```

## 📝 Exemples

### Exemple 1: Réservation Pré-définie
```
Date: 15/01/2025 (selected in DatePicker)
Checkbox: Non coché
Créneau pré-défini: 10:00-11:00
Motif: Réunion équipe
→ Réservation créée avec CreneauId existant
```

### Exemple 2: Réservation Personnalisée
```
Date: 25/01/2025 (selected in DatePicker)
Checkbox: Coché ✓
Heure début: 14:00
Heure fin: 15:30
Motif: Formation
→ Créneau créé dynamiquement
→ Réservation créée avec CreneauId nouveau
```

### Exemple 3: Conflit Détecté
```
Date: 15/01/2025
Heure début: 09:30
Heure fin: 10:30
Réservation existante: 09:00-10:00 (chevauchement!)
→ Message: "Conflit detecté! La salle est déjà reservée..."
→ Réservation refusée
```

## 🔒 Sécurité et Robustesse

1. **Double-vérification** : Conflit vérifié avant création
2. **Validation d'heures** : Heure début < heure fin
3. **Vérification de date** : Minimum = aujourd'hui
4. **Fallback visibilité** : Si convertisseur échoue, affiche par défaut

## 📈 Performance

- **DatePicker** : Léger, pas de requête DB
- **Génération dates** : 365 dates (initialisé une fois)
- **Vérification conflits** : O(n) acceptable
- **Création Créneau** : Rapide (une insertion DB)

## 🐛 Dépannage

### Le DatePicker ne montre pas toutes les dates
✓ C'est normal! Il montre 365 jours à partir d'aujourd'hui

### Les champs de temps ne s'affichent pas
✓ Vérifiez que le CheckBox est coché
✓ Vérifiez que `InverseBoolToVisibilityConverter` est enregistré

### Le format d'heure est invalide
✓ Utilisez HH:mm (ex: 09:30, 14:45)
✓ Vérifiez le séparateur `:` (deux-points)

### Erreur "Conflit détecté" inattendue
✓ Vérifiez les réservations existantes
✓ Vérifiez qu'il n'y a pas de chevauchement

## 🎓 Points Clés

1. **Flexibilité totale:** L'user peut choisir n'importe quelle date future
2. **Horaires personnalisés:** Pas limité aux créneaux pré-définis
3. **Sécurité:** Double-vérification des conflits
4. **Backward compatible:** Mode pré-défini encore fonctionnel
5. **Dynamic creneaux:** Créés automatiquement si nécessaire

## 📚 Fichiers Modifiés

| Fichier | Changement |
|---------|-----------|
| ReservationDialogViewModel.cs | +4 propriétés, logique personnalisée |
| ReservationDialogWindow.xaml | DatePicker, CheckBox, Time controls |
| ReservationDialogWindow.xaml.cs | Event handlers |
| ReservationConverters.cs | Nouveaux convertisseurs |
| App.xaml | Enregistrement convertisseurs |

## 🎯 Résultat Final

✅ **L'utilisateur peut maintenant:**
- Choisir n'importe quelle date future
- Saisir des horaires personnalisés
- Réserver en toute sécurité sans conflits
- Voir un message d'erreur clair en cas de conflit

✅ **Le système:**
- Crée automatiquement les créneaux manquants
- Vérifie les conflits robustement
- Offre deux modes (pré-défini et personnalisé)
- Reste backward compatible
