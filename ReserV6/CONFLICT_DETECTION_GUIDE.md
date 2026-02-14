# Guide de Détection des Conflits de Réservation

## Vue d'ensemble

Le système de réservation inclut une logique complète de détection des conflits pour s'assurer qu'aucune salle n'est réservée deux fois pour la même période.

## Mécanismes de Détection

### 1. Vérification Simple (Créneaux pré-définis)
**Méthode:** `ReservationRepository.HasConflict(int salleId, int creneauId)`

Vérifie si un créneau pré-défini est déjà réservé pour une salle donnée.

```csharp
bool hasConflict = repository.Reservations.HasConflict(salleId, creneauId);
```

**Logique:**
- Cherche toutes les réservations confirmées ou en attente pour cette salle
- Vérifie si le créneau est déjà occupé
- Filtre les statuts: `EnAttente` et `Confirmée`

### 2. Vérification Avancée (Plages horaires personnalisées)
**Méthode:** `ReservationRepository.HasTimeConflict(int salleId, DateTime startTime, DateTime endTime)`

Vérifie s'il y a une chevauchement entre une plage horaire demandée et les réservations existantes.

```csharp
bool hasConflict = repository.Reservations.HasTimeConflict(
    salleId, 
    startTime,    // Date et heure de début souhaitée
    endTime       // Date et heure de fin souhaitée
);
```

**Logique:**
- Récupère tous les créneaux réservés pour la salle avec statut confirmé ou en attente
- Utilise la formule de détection de chevauchement: `NOT (fin_existing <= start_new OR debut_existing >= end_new)`
- Retourne `true` s'il y a au moins un conflit

### Formule de Chevauchement

Deux plages horaires se chevauchent si:
```
NOT (FIN_EXISTANTE <= DEBUT_NOUVELLE OR DEBUT_EXISTANTE >= FIN_NOUVELLE)
```

Ou de manière équivalente, elles se chevauchent si:
```
DEBUT_NOUVELLE < FIN_EXISTANTE AND FIN_NOUVELLE > DEBUT_EXISTANTE
```

## Intégration dans le Workflow de Réservation

### Vue Utilisateur (ReservationDialogWindow)

1. **Sélection de la date**
   - L'utilisateur choisit une date dans le ComboBox
   - `OnDateSelectionChanged` est déclenché
   - Les créneaux sont filtrés pour cette date

2. **Sélection du créneau**
   - L'utilisateur clique sur "Selectionner" pour un créneau
   - `SelectCreneauCommand` est exécuté dans le ViewModel
   - La vérification de conflit est effectuée
   - Un avertissement s'affiche si un conflit est détecté

3. **Création de réservation**
   - L'utilisateur clique sur "Confirmer la réservation"
   - Une double vérification de conflit est effectuée
   - Si un conflit est détecté, un message d'avertissement s'affiche
   - Sinon, la réservation est créée

### Modèle de Données (ViewModel - ReservationDialogViewModel)

**Propriétés:**
- `HasConflictWarning` : Indique si un conflit a été détecté
- `ConflictMessage` : Message descriptif du conflit
- `CanCreateReservation` : Désactive le bouton si un conflit est détecté

**Conditions pour créer une réservation:**
1. Une salle est sélectionnée
2. Un créneau est sélectionné
3. Un motif est fourni
4. **Aucun conflit n'est détecté** ✅

## Scénarios de Test

### Cas 1: Créneau Libre
```
Salle: Salle A
Créneau demandé: 09:00 - 10:00 (aucune réservation existante)
Résultat: ✅ Réservation autorisée
```

### Cas 2: Créneau Occupé
```
Salle: Salle A
Créneau existant: 09:00 - 10:00 (réservé par Utilisateur B)
Créneau demandé: 09:00 - 10:00
Résultat: ❌ Conflit détecté - Réservation refusée
```

### Cas 3: Chevauchement Partiel
```
Salle: Salle A
Créneau existant: 09:00 - 10:00 (réservé)
Créneau demandé: 09:30 - 10:30
Résultat: ❌ Conflit détecté - Chevauchement sur 09:30 - 10:00
```

### Cas 4: Créneaux Adjacents (Pas de conflit)
```
Salle: Salle A
Créneau 1: 09:00 - 10:00 (réservé)
Créneau 2: 10:00 - 11:00 (demandé)
Résultat: ✅ Réservation autorisée (pas de chevauchement)
```

## Statuts Pris en Compte

Les vérifications de conflit concernent uniquement les réservations avec ces statuts:
- `EnAttente` : Réservation en attente de confirmation
- `Confirmée` : Réservation confirmée

Les réservations avec ces statuts sont **ignorées**:
- `Annulée` : Réservation annulée
- `Terminée` : Réservation déjà passée

## Améliorations Futures

1. **Filtrage par statut personnalisé** : Permettre une configuration des statuts à vérifier
2. **Buffers de temps** : Ajouter un délai entre deux réservations pour le nettoyage
3. **Récurrence** : Supporter les réservations récurrentes
4. **Validation en temps réel** : Afficher les créneaux conflictuels directement
5. **Export des plannings** : Permettre une visualisation calendaire des réservations

## Exemple d'Utilisation Complète

```csharp
// Dans ReservationDialogViewModel
[RelayCommand]
public void SelectCreneau(Creneau? creneau)
{
    if (creneau == null) return;

    _selectedCreneau = creneau;
    
    if (_repositoryManager != null && _selectedSalle != null)
    {
        // Vérification du conflit
        bool hasConflict = _repositoryManager.Reservations.HasConflict(
            _selectedSalle.Id, 
            creneau.Id
        );
        
        if (hasConflict)
        {
            HasConflictWarning = true;
            ConflictMessage = $"Conflit: Le créneau {creneau.Debut:HH:mm} - {creneau.Fin:HH:mm} est déjà réservé!";
        }
        else
        {
            HasConflictWarning = false;
            ConflictMessage = string.Empty;
        }
    }
    
    UpdateCanCreateReservation();
}
```

## Dépannage

### Le bouton "Confirmer" est désactivé
- Vérifiez qu'une salle est sélectionnée
- Vérifiez qu'un créneau est sélectionné
- Vérifiez qu'un motif est fourni
- Vérifiez qu'aucun avertissement de conflit n'est affiché

### Le message "Conflit" apparaît mais le créneau semble libre
- Vérifiez le statut des réservations existantes
- Vérifiez les fuseaux horaires
- Vérifiez les dates dans la base de données
