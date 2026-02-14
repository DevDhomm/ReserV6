# 🔍 Guide de Debugging : Gestion des Équipements

## Si les équipements ne s'affichent pas

### Vérification 1 : Chargement des données
```
1. Vérifier dans le Debug Output :
   - "SallesGestionViewModel: Starting data load..."
   - "SallesGestionViewModel: Fetching salles from database..."
   - "SallesGestionViewModel: Retrieved X salles"

2. Chaque salle doit avoir ses équipements chargés par :
   SalleRepository.GetAllSalles() 
   → EquipementRepository.GetSalleEquipements(salleId)
```

### Vérification 2 : Édition d'une salle
```
1. Cliquer "Éditer" sur une salle
2. Debug Output doit afficher :
   "SallesGestionViewModel: EditSalle for salle [NOM]"
3. Vérifier que EquipementsDeSalleSelectionnee est remplie
4. Le DataGrid devrait afficher les équipements
```

### Vérification 3 : DataGrid binding
```
Si le DataGrid est vide malgré les équipements :
- Vérifier : ItemsSource="{Binding ViewModel.EquipementsDeSalleSelectionnee}"
- Vérifier que c'est bien une ObservableCollection<Equipement>
- Vérifier que la hauteur n'est pas 0 (Height="150")
```

## Si les équipements ne s'ajoutent pas

### Vérification 1 : Bouton "+ Ajouter"
```
1. Vérifier que le bouton existe dans le formulaire modal de salle
2. Cliquer sur le bouton
3. Vérifier que SelectedSalle n'est pas null
4. Debug Output doit afficher :
   "SallesGestionViewModel: AddNewEquipement for salle [NOM]"
```

### Vérification 2 : Formulaire modal équipement
```
1. Après cliquer "+ Ajouter", un formulaire modal doit s'ouvrir
   Visibility={Binding ViewModel.IsEquipementFormVisible}
2. Le ZIndex doit être 101 pour être au-dessus du formulaire salle (ZIndex 100)
3. Remplir les champs
```

### Vérification 3 : Enregistrement
```
1. Cliquer "Enregistrer"
2. Vérifier validation :
   - Nom vide → Message d'erreur
   - Type vide → Message d'erreur
3. Si OK, Debug Output :
   "SallesGestionViewModel Error SaveEquipement: [erreur]" (si erreur)
   OU message de succès
4. Vérifier que l'équipement s'ajoute au DataGrid
5. Base de données doit être mise à jour
```

## Vérification des Bindings

### 1. Vérifier dans Visual Studio
```
Outils → Options → Debugging → Output Window
Chercher les avertissements de binding
```

### 2. Vérifier les collections
```csharp
// Dans le ViewModel, s'assurer que :
[ObservableProperty]
private ObservableCollection<Equipement> _equipementsDeSalleSelectionnee = [];

// La propriété générée automatiquement est :
public ObservableCollection<Equipement> EquipementsDeSalleSelectionnee { get; set; }
```

### 3. Vérifier les updates
```csharp
// Quand on ajoute un équipement :
EquipementsDeSalleSelectionnee.Add(newEquipement);
SelectedSalle.Equipements.Add(newEquipement);

// Quand on supprime :
EquipementsDeSalleSelectionnee.Remove(equipement);
SelectedSalle.Equipements.Remove(equipement);
```

## Points de Breakpoint Recommandés

### Dans SallesGestionViewModel.cs

1. **LoadDataAsync** ligne ~89 :
   ```csharp
   Salles = new ObservableCollection<Salle>(_allSalles);
   ```
   Vérifier que chaque salle a Equipements remplie

2. **EditSalle** ligne ~170 :
   ```csharp
   EquipementsDeSalleSelectionnee = new ObservableCollection<Equipement>(salle.Equipements);
   ```
   Vérifier que salle.Equipements n'est pas null/vide

3. **SaveEquipement** ligne ~525 :
   ```csharp
   EquipementsDeSalleSelectionnee.Add(newEquipement);
   ```
   Vérifier que l'ajout fonctionne

4. **DeleteEquipement** ligne ~590 :
   ```csharp
   EquipementsDeSalleSelectionnee.Remove(equipement);
   ```
   Vérifier que la suppression fonctionne

### Dans SalleRepository.cs

**GetAllSalles** ligne ~40 :
```csharp
foreach (var salle in salles)
{
    var equipements = equipementRepository.GetSalleEquipements(salle.Id);
    salle.Equipements = equipements;
}
```
Vérifier que les équipements sont chargés

## Commandes de Test en PowerShell

```powershell
# Vérifier la base de données
sqlite3 database.db "SELECT COUNT(*) as nb_salles FROM Salle;"
sqlite3 database.db "SELECT COUNT(*) as nb_equipements FROM Equipement;"

# Voir les équipements d'une salle
sqlite3 database.db "SELECT e.nom, e.type, e.estFonctionnel FROM Equipement e WHERE e.salle_id = 1;"
```

## Erreurs Courantes et Solutions

### 1. "Object reference not set to an instance of an object" en EditSalle
**Cause** : salle.Equipements est null
**Solution** : Initialiser dans EditSalle
```csharp
if (salle.Equipements == null)
    salle.Equipements = new List<Equipement>();
```

### 2. Le DataGrid ne se met pas à jour après Add
**Cause** : ObservableCollection pas correctement bound
**Solution** : Utiliser .Add() sur la collection observable, pas sur une copie

### 3. Le formulaire équipement ne s'ouvre pas
**Cause** : IsEquipementFormVisible reste false
**Solution** : Vérifier que AddNewEquipementCommand exécute bien le code

### 4. Les équipements disparaissent après save
**Cause** : Salles sont rechargées sans préserver la sélection
**Solution** : Ne pas recharger complètement, ou re-sélectionner après

## Logs À Rechercher

```
✅ "SallesGestionViewModel: Starting data load..."
✅ "SallesGestionViewModel: Fetching salles from database..."
✅ "SallesGestionViewModel: Retrieved X salles"
✅ "SallesGestionViewModel: EditSalle for salle [NOM]"
✅ "SallesGestionViewModel: AddNewEquipement for salle [NOM]"
✅ "SallesGestionViewModel Error SaveEquipement:" (si erreur)
❌ "SallesGestionViewModel: RepositoryManager is null" (erreur sérieuse)
❌ "SallesGestionViewModel: Deserialization error" (erreur DB)
```

## Vérification Finale

- [ ] Éditer une salle existante → voir ses équipements
- [ ] Cliquer "+ Ajouter" → voir formulaire modal
- [ ] Ajouter un équipement → voir dans le DataGrid
- [ ] Éditer l'équipement → voir dans formulaire
- [ ] Supprimer → voir disparition du DataGrid
- [ ] Fermer et réouvrir → vérifier persistence BD
- [ ] Chercher un équipement → doit retourner les salles

