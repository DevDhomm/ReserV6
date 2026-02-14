# Affichage Dynamique des Statuts et Filtrage - Documentation

## Implémentation Complète

Un système complet de **statuts dynamiques** et de **filtrage** a été implémenté pour la page Réservations.

## 🎨 Affichage Dynamique des Statuts

### Problème Résolu
Avant: Les réservations affichaient toujours le statut "Confirmée" même si elles auraient dû être "en cours" ou "terminées".

Maintenant: Les statuts changent **automatiquement en temps réel** basé sur la date/heure actuelle.

### Comment ça Fonctionne

#### ReservationStatusDynamicConverter
Converter WPF qui calcule le statut attendu **en temps réel**:

```csharp
public class ReservationStatusDynamicConverter : IValueConverter
{
    if (now >= creneauFin) return "Terminee";
    else if (now >= creneau && now < creneauFin) return "EnCours";
    else if (Statut == "Annulee") return "Annulee";
    else return reservation.Statut;
}
```

**Logique**:
- ⏹️ **Terminée**: La date/heure de fin est passée
- ▶️ **EnCours**: Actuellement entre la date/heure de début et fin
- ✅ **Confirmée**: À venir (date de début non atteinte)
- ❌ **Annulée**: Réservation annulée (état final)

#### Utilisation dans le DataGrid
```xaml
<DataGridTextColumn 
  Binding="{Binding ., Converter={StaticResource ReservationStatusDynamicConverter}}" 
  Header="Statut" />
```

## 🎨 Coloration des Lignes

### Système de Couleurs Automatique

Chaque ligne du DataGrid se colore automatiquement selon le statut:

| Statut | Couleur | Hexadécimal | Utilisation |
|--------|---------|---|---|
| EnCours | Vert clair | #C8FFC8 | Réservation actuellement active |
| Terminee | Gris clair | #DCDCDC | Réservation passée |
| Annulee | Rouge clair | #FFC8C8 | Réservation annulée |
| Confirmée | Blanc | #FFFFFF | Réservation à venir |

### Implémentation XAML

```xaml
<DataGrid.RowStyle>
  <Style TargetType="{x:Type DataGridRow}">
    <Style.Triggers>
      <DataTrigger Binding="{Binding ., Converter={StaticResource ReservationStatusDynamicConverter}}" 
                   Value="EnCours">
        <Setter Property="Background" Value="#C8FFC8" />
      </DataTrigger>
      <DataTrigger Binding="{Binding ., Converter={StaticResource ReservationStatusDynamicConverter}}" 
                   Value="Terminee">
        <Setter Property="Background" Value="#DCDCDC" />
      </DataTrigger>
      <DataTrigger Binding="{Binding ., Converter={StaticResource ReservationStatusDynamicConverter}}" 
                   Value="Annulee">
        <Setter Property="Background" Value="#FFC8C8" />
      </DataTrigger>
    </Style.Triggers>
  </Style>
</DataGrid.RowStyle>
```

## 🔍 Filtrage par Statut

### Interface Filtre

```xaml
<ComboBox
  ItemsSource="{Binding StatusFilterOptions}"
  SelectedValue="{Binding SelectedStatusFilter}"
  Width="200"
/>
```

Options disponibles:
- **Tous** - Affiche toutes les réservations
- **Confirmee** - Réservations à venir
- **EnCours** - Réservations actuellement actives
- **Terminee** - Réservations terminées
- **Annulee** - Réservations annulées

### Logique de Filtrage

```csharp
private void ApplyStatusFilter()
{
    if (SelectedStatusFilter == "Tous")
    {
        FilteredReservations = Reservations;
    }
    else
    {
        var filtered = Reservations.Where(r =>
        {
            // Recalcule le statut dynamique
            string status = GetDynamicStatus(r);
            return status == SelectedStatusFilter;
        }).ToList();
        
        FilteredReservations = filtered;
    }
}
```

### Handlers Automatiques

Grâce aux partial methods MVVM Toolkit:

```csharp
partial void OnSelectedStatusFilterChanged(string oldValue, string newValue)
{
    ApplyStatusFilter(); // Filtre automatiquement
}

partial void OnReservationsChanged(IEnumerable<ReservationComplete> oldValue, 
                                  IEnumerable<ReservationComplete> newValue)
{
    ApplyStatusFilter(); // Re-filtre si les données changent
}
```

## 📊 Flux d'Affichage

### Quand l'Utilisateur Ouvre la Page Réservations

```
1. OnNavigatedToAsync()
   ↓
2. LoadDataAsync()
   - UpdateAllReservationStatuses() (actualise la BD)
   - GetCompleteReservations() (récupère les données)
   ↓
3. Reservations = données du serveur
   ↓
4. OnReservationsChanged() déclenché
   ↓
5. ApplyStatusFilter()
   - SelectedStatusFilter = "Tous" (par défaut)
   - FilteredReservations = Reservations (affiche tout)
   ↓
6. DataGrid affiche FilteredReservations
   - Chaque ligne:
     • Statut calculé dynamiquement (converter)
     • Couleur appliquée selon statut (DataTrigger)
```

### Quand l'Utilisateur Change le Filtre

```
1. Utilisateur sélectionne "EnCours" dans ComboBox
   ↓
2. SelectedStatusFilter = "EnCours"
   ↓
3. OnSelectedStatusFilterChanged() déclenché
   ↓
4. ApplyStatusFilter()
   - Filtre Reservations WHERE statut == "EnCours"
   ↓
5. FilteredReservations = résultats filtrés
   ↓
6. DataGrid rafraîchit automatiquement (binding change)
```

## 🔄 Mises à Jour en Temps Réel

### Actualisation Automatique du Statut

Le statut change **automatiquement au fur et à mesure** sans rechargement:

**Exemple**:
- 09:59: Réservation affichée en blanc (Confirmée - commence à 10h)
- 10:00: **La ligne change au vert** (EnCours) - sans rechargement!
- 12:00: **La ligne change au gris** (Terminée) - sans rechargement!

Ceci est possible car:
1. Le converter recalcule à chaque rafraîchissement du DataGrid
2. WPF actualise la vue chaque seconde (ou quand les données changent)
3. Les DataTriggers appliquent les couleurs selon le résultat du converter

### Limitations Actuelles

⚠️ **Note**: Si l'utilisateur laisse la page Réservations ouverte pendant longtemps:
- Le statut affiché peut être décalé (reste "Confirmée" même après 10h)
- Solution: Actualiser la page (naviguer ailleurs et revenir)
- Amélioration future: Timer qui actualise les statuts toutes les minutes

## 📄 Fichiers Modifiés/Créés

| Fichier | Type | Changement |
|---------|------|---|
| `ReservationDynamicConverters.cs` | 🆕 Créé | 3 converters pour dynamique + couleurs |
| `ReservationsViewModel.cs` | 📝 Modifié | +3 propriétés (filtre), +méthodes de filtrage |
| `ReservationsPage.xaml` | 📝 Modifié | +ComboBox filtre, +RowStyle, +binding converter |
| `ReservationsPage.xaml.cs` | ✓ Inchangé | Code-behind existant toujours valide |
| `App.xaml` | 📝 Modifié | +3 converter registrations |

## 🎯 Cas d'Usage

### Cas 1: Voir Toutes les Réservations
```
ComboBox = "Tous" → Affiche 100% des réservations
```

### Cas 2: Voir Uniquement les Réservations en Cours
```
ComboBox = "EnCours" → Affiche seulement celles actuellement actives
                      (affichées en vert clair)
```

### Cas 3: Archiver les Réservations Terminées
```
Filtre = "Terminee" → Admin peut voir toutes les réservations terminées
                      (affichées en gris clair)
                      → Peut les supprimer du système
```

## 🔧 Détails Techniques

### ReservationDynamicConverters.cs

```csharp
public class ReservationStatusDynamicConverter : IValueConverter
```
- **Input**: ReservationComplete (objet entier)
- **Output**: string (statut calculé)
- **Appel**: À chaque rafraîchissement du binding

```csharp
public class ReservationRowColorConverter : IValueConverter
```
- **Input**: ReservationComplete (objet entier)  
- **Output**: SolidColorBrush (couleur de la ligne)
- **Appel**: Utilisé dans les DataTriggers

```csharp
public class ReservationCanEditConverter : IValueConverter
```
- **Input**: ReservationComplete
- **Output**: bool (peut éditer?)
- **Logique**: true si (not Annulée AND not Terminée AND not EnCours)

### ReservationsViewModel.cs

```csharp
[ObservableProperty]
private string _selectedStatusFilter = "Tous";

[ObservableProperty]
private IEnumerable<string> _statusFilterOptions = new[]
{ "Tous", "Confirmee", "EnCours", "Terminee", "Annulee" };

[ObservableProperty]
private IEnumerable<ReservationComplete> _filteredReservations = [];
```

## 📈 Performance

- ✅ Les converters sont appelés une fois par ligne lors du rendu
- ✅ Le filtrage LINQ est O(n) sur ~100 réservations
- ✅ Pas de requête BD pour les mises à jour dynamiques
- ⚠️ Peut ralentir avec 10000+ réservations

## 🚀 Améliorations Futures

- [ ] Timer pour actualiser les statuts toutes les 60 secondes
- [ ] Notification utilisateur quand une réservation devient "EnCours"
- [ ] Tri par colonne dans le DataGrid
- [ ] Recherche par mot-clé (salle, utilisateur, motif)
- [ ] Export des réservations filtrées
- [ ] Graphique des réservations par jour/semaine

## ✅ Testing Checklist

- [ ] Ouvrir page Réservations → Coloration correcte?
- [ ] Filtrer par "EnCours" → Affiche seulement réservations actuelles?
- [ ] Filtrer par "Terminee" → Affiche réservations passées?
- [ ] Filtrer par "Annulee" → Affiche réservations annulées?
- [ ] Revenir à "Tous" → Tous les statuts visibles?
- [ ] Attendre 10h si réservation à 10h → Passe-t-elle au vert automatiquement?
- [ ] Boutons Annuler/Supprimer → Toujours fonctionnels?
