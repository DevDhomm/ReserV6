# Gestion des Statuts de Réservation - Documentation

## Vue d'ensemble

Un système complet de gestion automatique des statuts de réservation a été implémenté. Les réservations changent automatiquement de statut en fonction de la date actuelle et de leurs dates de début/fin.

## Statuts Disponibles

L'énumération `ReservationStatut` contient maintenant 5 états:

```csharp
public enum ReservationStatut
{
    EnAttente,      // En attente de confirmation
    Confirmée,      // Confirmée et à venir
    EnCours,        // Actuellement en cours
    Terminée,       // Terminée (date de fin dépassée)
    Annulée         // Annulée
}
```

## Logique de Transition Automatique

### ReservationStatusService

Service qui gère l'actualisation automatique des statuts (situé dans `Services/ReservationStatusService.cs`):

**Méthode principale: `UpdateAllReservationStatuses()`**
- Parcourt toutes les réservations
- Compares la date/heure actuelle avec les dates de début/fin des créneaux
- Met à jour automatiquement les statuts selon ces règles:

| Condition | Nouveau Statut | Description |
|-----------|---|---|
| `now >= Creneau.Fin` | ⏹️ **Terminée** | La réservation est passée |
| `Creneau.Debut <= now < Creneau.Fin` | ▶️ **EnCours** | La réservation est actuellement active |
| `now < Creneau.Debut` | ✅ **Confirmée** | La réservation est à venir (état initial) |
| `Statut == Annulée` | ❌ **Annulée** | Les réservations annulées ne changent pas |

### Quand la Mise à Jour Automatique Se Déclenche?

1. **Au chargement de la page Réservations** - Actualise tous les statuts avant d'afficher les données
2. **À chaque navigation vers la page** - L'event `OnNavigatedToAsync()` appelle `UpdateAllReservationStatuses()`

## Actions sur les Réservations

### 1. Annuler une Réservation
- **Bouton**: "Annuler" dans la colonne Actions
- **Condition**: Actif si le statut n'est pas "Annulée" ou "Terminée"
- **Effet**: Change le statut à "Annulée"
- **Confirmation**: Demande la confirmation à l'utilisateur
- **Code**: 
```csharp
[RelayCommand]
public async Task CancelReservation(ReservationComplete? reservation)
```

### 2. Supprimer une Réservation
- **Bouton**: "Supprimer" dans la colonne Actions
- **Effet**: Supprime la réservation définitivement de la base de données
- **Confirmation**: Demande la confirmation à l'utilisateur
- **Code**:
```csharp
[RelayCommand]
public async Task DeleteReservation(ReservationComplete? reservation)
```

### 3. Actualiser la Pagination
- **Automatique**: Après chaque action (annulation/suppression), les données se rechargent

## Structure du Fichier ReservationsPage.xaml

### DataGrid Amélioré

La page Réservations utilise un DataGrid avec 7 colonnes:

```xaml
<DataGrid ItemsSource="{Binding ViewModel.Reservations}">
  <DataGrid.Columns>
    <DataGridTextColumn Binding="{Binding SalleNom}" Header="Salle" />
    <DataGridTextColumn Binding="{Binding UserNom}" Header="Utilisateur" />
    <DataGridTextColumn Binding="{Binding Motif}" Header="Motif" />
    <DataGridTextColumn Binding="{Binding CreneauDebut, StringFormat=dd/MM/yyyy HH:mm}" Header="Debut" />
    <DataGridTextColumn Binding="{Binding CreneauFin, StringFormat=dd/MM/yyyy HH:mm}" Header="Fin" />
    <DataGridTextColumn Binding="{Binding Statut}" Header="Statut" />
    <DataGridTemplateColumn Header="Actions">
      <!-- Boutons Annuler et Supprimer -->
    </DataGridTemplateColumn>
  </DataGrid.Columns>
</DataGrid>
```

### Propriété IsNotFinalStatus

Ajoutée au modèle `ReservationComplete`:

```csharp
public bool IsNotFinalStatus
{
    get
    {
        return Statut != "Annulée" && Statut != "Terminée";
    }
}
```

- **Utilisée pour**: Désactiver le bouton "Annuler" si la réservation est déjà annulée ou terminée
- **Logic**: Le bouton reste actif seulement si on peut encore annuler la réservation

## Flux Complet d'Utilisation

### Scénario 1: Réservation à Venir
```
1. Créer une réservation pour demain 14h-16h
2. Statut initial: "Confirmée"
3. En chargeant la page Réservations: reste "Confirmée"
4. Utilisateur peut: Annuler ✓ ou Supprimer ✓
```

### Scénario 2: Réservation en Cours
```
1. Réservation aujourd'hui 14h-16h (et on est 15h)
2. Charger la page Réservations
3. UpdateAllReservationStatuses() détecte que now > debut et now < fin
4. Statut change automatiquement: "Confirmée" → "EnCours"
5. Utilisateur peut: Annuler ✓ ou Supprimer ✓
```

### Scénario 3: Réservation Terminée
```
1. Réservation hier 14h-16h
2. Charger la page Réservations
3. UpdateAllReservationStatuses() détecte que now > fin
4. Statut change automatiquement: "Confirmée" → "Terminée"
5. Utilisateur peut: Supprimer ✓ (Annuler ✗ désactivé)
6. Message d'historique: "Réservation archivée"
```

### Scénario 4: Réservation Annulée Manuellement
```
1. Réservation demain 14h-16h, statut "Confirmée"
2. Utilisateur clique sur "Annuler"
3. Confirmation demandée
4. Statut change: "Confirmée" → "Annulée"
5. Bouton "Annuler" devient ✗ désactivé
6. Utilisateur peut: Supprimer ✓
```

## Messages de Confirmation

### Annuler une Réservation
```
Title: "Confirmation d'annulation"
Message: "Êtes-vous sûr de vouloir annuler cette réservation?
          Salle: {nom}
          Date: {date HH:mm}"
Options: Oui / Non
```

### Supprimer une Réservation
```
Title: "Confirmation de suppression"
Message: "Êtes-vous sûr de vouloir supprimer la réservation?
          Salle: {nom}
          Date: {date HH:mm}"
Options: Oui / Non
```

## Appels de Code

### Depuis ReservationsViewModel

```csharp
// Charge les données et actualise les statuts
private async Task LoadDataAsync()
{
    // ...
    _statusService = new ReservationStatusService(_repositoryManager);
    
    // Actualise automatiquement avant d'afficher
    await Task.Run(() => _statusService.UpdateAllReservationStatuses());
    
    // Charge et affiche les réservations
    var reservations = _repositoryManager.Reservations.GetCompleteReservations();
    Reservations = reservations;
}

// Annule une réservation
[RelayCommand]
public async Task CancelReservation(ReservationComplete? reservation)
{
    // Confirmation utilisateur
    // Appel: _statusService.UpdateReservationStatus(id, ReservationStatut.Annulée)
    // Rechargement des données
}

// Supprime une réservation
[RelayCommand]
public async Task DeleteReservation(ReservationComplete? reservation)
{
    // Confirmation utilisateur
    // Appel: _repositoryManager.Reservations.DeleteReservation(id)
    // Rechargement des données
}
```

## Cas Particuliers

### Réservation Multi-Jour
Une réservation du lundi 9h au mardi 17h:
- Lundi 9h-23h: Statut = "EnCours"
- Mardi 0h-17h: Statut = "EnCours" (toujours dans la plage)
- Mardi 17h+: Statut = "Terminée"

### Réservation Passée Non Finalisée
Si une réservation très ancienne (2023) existe encore:
- La première fois qu'on charge la page: statut → "Terminée"
- Elle apparaît en grisé/archived dans le système
- Peut être supprimée mais pas annulée

## Modifications Apportées

| Fichier | Modifications |
|---------|---|
| `ReservationSystemModels.cs` | Ajout statut "EnCours"; propriété `IsNotFinalStatus` |
| `ReservationStatusService.cs` | Nouveau service de gestion des statuts |
| `ReservationsViewModel.cs` | Ajout `CancelReservationCommand` et `DeleteReservationCommand` |
| `ReservationsPage.xaml` | Nouvelle colonne "Actions" avec 2 boutons |

## Dépendances

- ✅ `MVVM Community Toolkit` - pour `[RelayCommand]`
- ✅ `System.Diagnostics` - pour logs debug
- ✅ `System.Windows` - pour MessageBox
- ✅ Repositories existants - `DeleteReservation()`, `UpdateReservationStatus()`

## À Améliorer (Futur)

- [ ] Colorer les lignes selon le statut (vert=en cours, rouge=terminée)
- [ ] Ajouter un filtre par statut
- [ ] Ajouter l'historique des changements de statut
- [ ] Notifier l'utilisateur quand une réservation bascule à "EnCours"
- [ ] Archiver automatiquement les réservations très anciennes
- [ ] Exporter les réservations en PDF/Excel
- [ ] Ajouter la modification (édition) de réservation avant qu'elle commence

## Exemples de Sortie Debug

```
ReservationStatusService: Updating reservation statuses...
ReservationStatusService: Reservation 1 status updated to EnCours
ReservationStatusService: Reservation 2 status updated to Terminee
ReservationStatusService: 2 reservations status updated
ReservationsViewModel: 5 reservations loaded successfully
```
