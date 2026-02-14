# 🔧 Amélioration du Filtrage par Équipement - RoomsPage

## 📋 Vue d'ensemble des modifications

Le système de recherche par équipement dans **RoomsPage** a été amélioré pour permettre la sélection **multiple** d'équipements et charger les équipements directement depuis la base de données.

## ✨ Nouvelles Fonctionnalités

### 1. **Sélection Multiple d'Équipements**
- Les utilisateurs peuvent sélectionner **plusieurs équipements** via des CheckBox
- Les équipements sont chargés automatiquement depuis la base de données
- Un compteur affiche le nombre d'équipements sélectionnés

### 2. **Interface Utilisateur Améliorée**
```
┌─────────────────────────────────┐
│ Filtrer par equipement          │
├─────────────────────────────────┤
│ ☑ Vidéoprojecteur              │
│ ☐ Tableau Interactif            │
│ ☑ Système Sonore                │
│ ☐ Climatisation                 │
│ ☐ Connexion Internet            │
└─────────────────────────────────┘
📋 Sélectionné: 2   [Réinitialiser]
```

### 3. **Logique de Filtrage**
- Les salles affichées doivent avoir **TOUS** les équipements sélectionnés
- Si 2 équipements sont sélectionnés, seules les salles ayant ces 2 équipements apparaissent
- Si aucun équipement n'est sélectionné, le filtre d'équipement ne s'applique pas

## 🔄 Flux de Travail Utilisateur

### Scénario 1 : Recherche simple par équipement
```
1. User accède à RoomsPage
2. La liste des équipements disponibles se charge automatiquement
3. User sélectionne "Vidéoprojecteur" dans la liste
4. Les filtres s'appliquent automatiquement
5. Seules les salles avec Vidéoprojecteur sont affichées
```

### Scénario 2 : Recherche multi-équipements
```
1. User sélectionne "Vidéoprojecteur"
2. User sélectionne "Tableau Interactif"
3. Les filtres s'appliquent automatiquement
4. Seules les salles avec BOTH équipements sont affichées
5. User clique "Réinitialiser"
6. Tous les équipements sont désélectionnés
7. Tous les salles réapparaissent
```

## 🏗️ Architecture Technique

### Modifications dans RoomsViewModel

#### Nouvelles Propriétés
```csharp
[ObservableProperty]
private ObservableCollection<Equipement> _availableEquipements = new();
// Contient tous les équipements disponibles dans la base de données

[ObservableProperty]
private ObservableCollection<Equipement> _selectedEquipements = new();
// Contient les équipements sélectionnés par l'utilisateur
```

#### Logique de Filtrage Mise à Jour
```csharp
// Filter by selected equipements - salle must have ALL selected equipements
if (_selectedEquipements.Count > 0)
{
    var selectedEquipementIds = _selectedEquipements.Select(e => e.Id).ToHashSet();
    filtered = filtered.Where(r =>
    {
        if (r.Equipements == null || r.Equipements.Count == 0)
            return false;

        var salleEquipementIds = r.Equipements.Select(e => e.Id).ToHashSet();
        // Check if room has ALL selected equipements
        return selectedEquipementIds.All(id => salleEquipementIds.Contains(id));
    });
}
```

#### Chargement des Équipements
```csharp
// Load all available equipements from database
var equipements = await Task.Run(() =>
{
    var result = _repositoryManager.Equipements.GetAllEquipements();
    return result;
});

// Grouper par ID pour éviter les doublons et trier
AvailableEquipements = new ObservableCollection<Equipement>(
    equipements.GroupBy(e => e.Id)
              .Select(g => g.First())
              .OrderBy(e => e.Type)
              .ThenBy(e => e.Nom)
              .ToList()
);
```

### Modifications dans RoomsPage.xaml

#### UI de Sélection Multiple
```xaml
<Border Height="80" Width="250">
  <ScrollViewer>
    <ItemsControl ItemsSource="{Binding ViewModel.AvailableEquipements}">
      <ItemsControl.ItemTemplate>
        <DataTemplate>
          <CheckBox 
            Content="{Binding Nom}"
            ToolTip="{Binding Type}"
            Tag="{Binding}"
            PreviewMouseUp="OnEquipementCheckBoxToggled"
            />
        </DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>
  </ScrollViewer>
</Border>
```

#### Compteur et Bouton Réinitialiser
```xaml
<TextBlock 
  Text="{Binding ViewModel.SelectedEquipements.Count, StringFormat='Sélectionné: {0}'}"
  />
<ui:Button 
  Content="Réinitialiser"
  Click="OnClearEquipementsClick"
  />
```

### Modifications dans RoomsPage.xaml.cs

#### Gestion des Événements CheckBox
```csharp
private void OnEquipementCheckBoxToggled(object sender, MouseButtonEventArgs e)
{
    if (sender is CheckBox checkBox && checkBox.Tag is Equipement equipement)
    {
        if (checkBox.IsChecked == true)
        {
            if (!ViewModel.SelectedEquipements.Contains(equipement))
            {
                ViewModel.SelectedEquipements.Add(equipement);
            }
        }
        else
        {
            ViewModel.SelectedEquipements.Remove(equipement);
        }
    }
}
```

#### Réinitialisation
```csharp
private void OnClearEquipementsClick(object sender, RoutedEventArgs e)
{
    ViewModel.SelectedEquipements.Clear();
}
```

## 🔗 Intégration avec les Autres Filtres

### Combinaisons de Filtres Supportées
```
┌─────────────────────────────────────────────────────┐
│ Recherche par nom        ✓ Combinable               │
│ Capacité minimale        ✓ Combinable               │
│ Filtre par étage         ✓ Combinable               │
│ Sélection d'équipements  ✓ Combinable               │
└─────────────────────────────────────────────────────┘
```

### Exemple : Filtrage Avancé
```
Utilisateur cherche une salle qui:
- Contient "Meeting" dans le nom
- Capacité ≥ 10 personnes
- Étage 2
- Avec Vidéoprojecteur ET Tableau Interactif

Résultat: Seules les salles matchant TOUS ces critères sont affichées
```

## 📊 Flux de Données

```
Database (Equipement Table)
        ↓
EquipementRepository.GetAllEquipements()
        ↓
RoomsViewModel.LoadDataAsync()
        ↓
AvailableEquipements ObservableCollection
        ↓
RoomsPage CheckBox ItemsControl
        ↓
User Selection → SelectedEquipements ObservableCollection
        ↓
OnSelectedEquipementsChanged Handler
        ↓
ApplyFilters()
        ↓
FilteredRooms
        ↓
RoomsPage ItemsControl Display
```

## 🎯 Avantages

✅ **Sélection Multiple**: Filtrer par plusieurs équipements à la fois
✅ **Données Persistantes**: Les équipements viennent de la base de données
✅ **Feedback Immédiat**: Les filtres s'appliquent en temps réel
✅ **UX Intuitive**: Interface CheckBox familière et facile à utiliser
✅ **Combinaisons Flexibles**: Marche avec tous les autres filtres
✅ **Performance**: Groupage et déduplication des équipements

## ⚙️ Dépendances

- `ObservableCollection<Equipement>` - Collection observable
- `EquipementRepository.GetAllEquipements()` - Charge tous les équipements
- `Wpf.Ui.Abstractions.Controls` - Composants UI
- Event handlers dans code-behind pour la gestion des CheckBox

## 🐛 Cas Limites Gérés

1. **Aucun équipement disponible** → UI vide, pas d'erreur
2. **Équipements supprimés de la base** → Pas de régression, filtrage stable
3. **Salles sans équipement** → Excluées correctement du filtrage
4. **Sélection puis réinitialisation** → Tous les équipements se désélectionnent
5. **Multiple équipements sélectionnés** → Les 3 doivent être présents

## 📝 Notes

- Les équipements sont regroupés par ID pour éviter les doublons
- Tri : d'abord par Type, puis par Nom pour meilleure lisibilité
- Le compteur affiche en temps réel le nombre d'équipements sélectionnés
- Le bouton "Réinitialiser" ne s'affiche que si au moins 1 équipement est sélectionné (optionnel)
