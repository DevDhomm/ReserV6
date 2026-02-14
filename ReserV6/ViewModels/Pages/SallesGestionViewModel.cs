using System.Collections.ObjectModel;
using Wpf.Ui.Abstractions.Controls;

namespace ReserV6.ViewModels.Pages
{
    /// <summary>
    /// ViewModel pour la gestion des salles (CRUD - Créer, Lire, Éditer, Supprimer)
    /// </summary>
    public partial class SallesGestionViewModel : ObservableObject, INavigationAware
    {
        private static ReservationSystemInitializer? _initializer;
        private RepositoryManager? _repositoryManager;
        private List<Salle> _allSalles = [];

        // Types de salles disponibles
        public ObservableCollection<string> RoomTypes { get; } = new()
        {
            "Réunion",
            "Conférence",
            "Formation",
            "Bureau",
            "Autre"
        };

        // Types d'équipements disponibles
        public ObservableCollection<string> EquipementTypes { get; } = new()
        {
            "Vidéoprojecteur",
            "Tableau interactif",
            "Écran plat",
            "Système audio",
            "Ordinateur",
            "Mobilier",
            "Autre"
        };

        [ObservableProperty]
        private ObservableCollection<Salle> _salles = [];

        [ObservableProperty]
        private Salle? _selectedSalle;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isFormVisible = false;

        [ObservableProperty]
        private string _formTitle = "Ajouter une salle";

        // Formulaire properties
        [ObservableProperty]
        private string _salleNom = string.Empty;

        [ObservableProperty]
        private string _salleDescription = string.Empty;

        [ObservableProperty]
        private int _salleCapacite = 10;

        [ObservableProperty]
        private string _salleType = string.Empty;

        [ObservableProperty]
        private int _salleEtage = 1;

        [ObservableProperty]
        private bool _salleDispo = true;

        [ObservableProperty]
        private ObservableCollection<Equipement> _equipementsDeSalleSelectionnee = [];

        // Formulaire équipement
        [ObservableProperty]
        private bool _isEquipementFormVisible = false;

        [ObservableProperty]
        private string _equipementNom = string.Empty;

        [ObservableProperty]
        private string _equipementDescription = string.Empty;

        [ObservableProperty]
        private string _equipementType = string.Empty;

        [ObservableProperty]
        private bool _equipementEstFonctionnel = true;

        [ObservableProperty]
        private Equipement? _selectedEquipement;

        public SallesGestionViewModel()
        {
        }

        public async Task OnNavigatedToAsync()
        {
            await LoadDataAsync();
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        private async Task LoadDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: Starting data load...");

                if (_initializer == null)
                {
                    System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: Initializing system...");
                    _initializer = new ReservationSystemInitializer();
                    _initializer.Initialize();
                }

                _repositoryManager = _initializer.GetRepositoryManager();

                var salles = await Task.Run(() =>
                {
                    System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: Fetching salles from database...");
                    var result = _repositoryManager.Salles.GetAllSalles();
                    System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: Retrieved {result?.Count ?? 0} salles");
                    return result ?? [];
                });

                _allSalles = salles.ToList();
                Salles = new ObservableCollection<Salle>(_allSalles);

                System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: {_allSalles.Count} salles loaded successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel Error: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur lors du chargement: {ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Ouvre le formulaire pour ajouter une nouvelle salle
        /// </summary>
        [RelayCommand]
        public void AddNewSalle()
        {
            System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: AddNewSalle command triggered");

            // Reset form
            SalleNom = string.Empty;
            SalleDescription = string.Empty;
            SalleCapacite = 10;
            SalleType = string.Empty;
            SalleEtage = 1;
            SalleDispo = true;
            SelectedSalle = null;
            EquipementsDeSalleSelectionnee = new ObservableCollection<Equipement>();

            FormTitle = "Ajouter une salle";
            IsFormVisible = true;
        }

        /// <summary>
        /// Ouvre le formulaire pour éditer une salle sélectionnée
        /// </summary>
        [RelayCommand]
        public void EditSalle(Salle? salle)
        {
            if (salle == null)
            {
                System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: EditSalle called with null salle");
                System.Windows.MessageBox.Show(
                    "Veuillez sélectionner une salle à éditer",
                    "Information",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
                return;
            }

            System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: EditSalle for salle {salle.Nom}");

            // Load form with selected salle
            SalleNom = salle.Nom;
            SalleDescription = salle.Description;
            SalleCapacite = salle.Capacite;
            SalleType = salle.Type;
            SalleEtage = salle.Etage;
            SalleDispo = salle.Disponibilite;
            SelectedSalle = salle;

            // Load equipements for this room
            EquipementsDeSalleSelectionnee = new ObservableCollection<Equipement>(salle.Equipements);

            FormTitle = $"Éditer : {salle.Nom}";
            IsFormVisible = true;
        }

        /// <summary>
        /// Enregistre la salle (créer ou éditer)
        /// </summary>
        [RelayCommand]
        public async Task SaveSalle()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(SalleNom))
                {
                    System.Windows.MessageBox.Show(
                        "Le nom de la salle est requis",
                        "Validation",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning
                    );
                    return;
                }

                if (SalleCapacite <= 0)
                {
                    System.Windows.MessageBox.Show(
                        "La capacité doit être supérieure à 0",
                        "Validation",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning
                    );
                    return;
                }

                if (_repositoryManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: RepositoryManager is null");
                    return;
                }

                if (SelectedSalle == null)
                {
                    // Créer une nouvelle salle
                    var newSalle = new Salle
                    {
                        Nom = SalleNom,
                        Description = SalleDescription,
                        Capacite = SalleCapacite,
                        Type = SalleType,
                        Etage = SalleEtage,
                        Disponibilite = SalleDispo,
                        DateCreation = DateTime.Now
                    };

                    System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: Creating new salle {newSalle.Nom}");

                    var id = await Task.Run(() => _repositoryManager.Salles.AddSalle(newSalle));

                    if (id > 0)
                    {
                        newSalle.Id = id;
                        newSalle.Equipements = new List<Equipement>();
                        _allSalles.Add(newSalle);
                        Salles = new ObservableCollection<Salle>(_allSalles);

                        System.Windows.MessageBox.Show(
                            $"Salle '{newSalle.Nom}' créée avec succès!",
                            "Succès",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information
                        );

                        IsFormVisible = false;
                    }
                }
                else
                {
                    // Éditer la salle existante
                    SelectedSalle.Nom = SalleNom;
                    SelectedSalle.Description = SalleDescription;
                    SelectedSalle.Capacite = SalleCapacite;
                    SelectedSalle.Type = SalleType;
                    SelectedSalle.Etage = SalleEtage;
                    SelectedSalle.Disponibilite = SalleDispo;

                    System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: Updating salle {SelectedSalle.Nom}");

                    var updated = await Task.Run(() => _repositoryManager.Salles.UpdateSalle(SelectedSalle));

                    if (updated)
                    {
                        // Refresh the list
                        Salles = new ObservableCollection<Salle>(_allSalles);

                        System.Windows.MessageBox.Show(
                            $"Salle '{SelectedSalle.Nom}' mise à jour avec succès!",
                            "Succès",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information
                        );

                        IsFormVisible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel SaveSalle Error: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur lors de la sauvegarde: {ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Annule l'édition/création
        /// </summary>
        [RelayCommand]
        public void CancelForm()
        {
            System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: CancelForm triggered");
            IsFormVisible = false;
            SelectedSalle = null;
            EquipementsDeSalleSelectionnee = new ObservableCollection<Equipement>();
            SelectedEquipement = null;
        }

        /// <summary>
        /// Supprime la salle sélectionnée
        /// </summary>
        [RelayCommand]
        public async Task DeleteSalle(Salle? salle)
        {
            if (salle == null)
            {
                System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: DeleteSalle called with null salle");
                System.Windows.MessageBox.Show(
                    "Veuillez sélectionner une salle à supprimer",
                    "Information",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information
                );
                return;
            }

            try
            {
                var result = System.Windows.MessageBox.Show(
                    $"Êtes-vous sûr de vouloir supprimer la salle '{salle.Nom}'?\n\nCette action ne peut pas être annulée.",
                    "Confirmation de suppression",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning
                );

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: Deletion cancelled by user");
                    return;
                }

                if (_repositoryManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: RepositoryManager is null");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: Deleting salle {salle.Nom}");

                var deleted = await Task.Run(() => _repositoryManager.Salles.DeleteSalle(salle.Id));

                if (deleted)
                {
                    _allSalles.Remove(salle);
                    Salles = new ObservableCollection<Salle>(_allSalles);

                    System.Windows.MessageBox.Show(
                        $"Salle '{salle.Nom}' supprimée avec succès!",
                        "Succès",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );

                    SelectedSalle = null;
                    IsFormVisible = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel DeleteSalle Error: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur lors de la suppression: {ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Filtre les salles selon la recherche
        /// </summary>
        [RelayCommand]
        public void SearchSalles()
        {
            System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: Searching for '{SearchText}'");

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Salles = new ObservableCollection<Salle>(_allSalles);
            }
            else
            {
                var search = SearchText.ToLower();
                var filtered = _allSalles
                    .Where(s => s.Nom.ToLower().Contains(search) ||
                               s.Description.ToLower().Contains(search) ||
                               s.Type.ToLower().Contains(search) ||
                               s.Equipements.Any(e => e.Nom.ToLower().Contains(search) || 
                                                      e.Type.ToLower().Contains(search) ||
                                                      e.Description.ToLower().Contains(search)))
                    .ToList();

                Salles = new ObservableCollection<Salle>(filtered);
            }

            System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: Found {Salles.Count} salles");
        }

        /// <summary>
        /// Ajoute un nouvel équipement à la salle sélectionnée
        /// </summary>
        [RelayCommand]
        public void AddNewEquipement()
        {
            if (SelectedSalle == null)
            {
                System.Windows.MessageBox.Show(
                    "Veuillez sélectionner une salle d'abord",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: AddNewEquipement for salle {SelectedSalle.Nom}");

            EquipementNom = string.Empty;
            EquipementDescription = string.Empty;
            EquipementType = string.Empty;
            EquipementEstFonctionnel = true;
            SelectedEquipement = null;

            IsEquipementFormVisible = true;
        }

        /// <summary>
        /// Édite un équipement existant
        /// </summary>
        [RelayCommand]
        public void EditEquipement(Equipement? equipement)
        {
            if (equipement == null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: EditEquipement {equipement.Nom}");

            EquipementNom = equipement.Nom;
            EquipementDescription = equipement.Description;
            EquipementType = equipement.Type;
            EquipementEstFonctionnel = equipement.EstFonctionnel;
            SelectedEquipement = equipement;

            IsEquipementFormVisible = true;
        }

        /// <summary>
        /// Enregistre un équipement (créer ou éditer)
        /// </summary>
        [RelayCommand]
        public async Task SaveEquipement()
        {
            if (SelectedSalle == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(EquipementNom))
            {
                System.Windows.MessageBox.Show(
                    "Le nom de l'équipement est obligatoire",
                    "Erreur de validation",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            if (string.IsNullOrWhiteSpace(EquipementType))
            {
                System.Windows.MessageBox.Show(
                    "Le type d'équipement est obligatoire",
                    "Erreur de validation",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
                return;
            }

            try
            {
                if (SelectedEquipement == null)
                {
                    // CREATE new equipement
                    var newEquipement = new Equipement
                    {
                        Nom = EquipementNom,
                        Description = EquipementDescription,
                        Type = EquipementType,
                        EstFonctionnel = EquipementEstFonctionnel,
                        SalleId = SelectedSalle.Id,
                        DateCreation = DateTime.Now
                    };

                    var id = await Task.Run(() => _repositoryManager!.Equipements.AddEquipement(newEquipement));
                    if (id > 0)
                    {
                        newEquipement.Id = id;
                        EquipementsDeSalleSelectionnee.Add(newEquipement);
                        SelectedSalle.Equipements.Add(newEquipement);

                        System.Windows.MessageBox.Show(
                            $"Équipement '{EquipementNom}' ajouté avec succès",
                            "Succès",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information
                        );
                    }
                }
                else
                {
                    // UPDATE existing equipement
                    SelectedEquipement.Nom = EquipementNom;
                    SelectedEquipement.Description = EquipementDescription;
                    SelectedEquipement.Type = EquipementType;
                    SelectedEquipement.EstFonctionnel = EquipementEstFonctionnel;

                    var updated = await Task.Run(() => _repositoryManager!.Equipements.UpdateEquipement(SelectedEquipement));
                    if (updated)
                    {
                        System.Windows.MessageBox.Show(
                            $"Équipement '{EquipementNom}' modifié avec succès",
                            "Succès",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information
                        );
                    }
                }

                IsEquipementFormVisible = false;
                EquipementNom = string.Empty;
                EquipementDescription = string.Empty;
                EquipementType = string.Empty;
                EquipementEstFonctionnel = true;
                SelectedEquipement = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel Error SaveEquipement: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur lors de l'enregistrement: {ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Annule l'édition d'un équipement
        /// </summary>
        [RelayCommand]
        public void CancelEquipementForm()
        {
            System.Diagnostics.Debug.WriteLine("SallesGestionViewModel: CancelEquipementForm");
            IsEquipementFormVisible = false;
        }

        /// <summary>
        /// Supprime un équipement
        /// </summary>
        [RelayCommand]
        public async Task DeleteEquipement(Equipement? equipement)
        {
            if (equipement == null || SelectedSalle == null)
            {
                return;
            }

            System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel: DeleteEquipement {equipement.Nom}");

            var result = System.Windows.MessageBox.Show(
                $"Êtes-vous sûr de vouloir supprimer l'équipement '{equipement.Nom}' ?",
                "Confirmation",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question
            );

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var deleted = await Task.Run(() => _repositoryManager!.Equipements.DeleteEquipement(equipement.Id));
                if (deleted)
                {
                    EquipementsDeSalleSelectionnee.Remove(equipement);
                    SelectedSalle.Equipements.Remove(equipement);

                    System.Windows.MessageBox.Show(
                        $"Équipement '{equipement.Nom}' supprimé avec succès",
                        "Succès",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SallesGestionViewModel Error DeleteEquipement: {ex.Message}");
                System.Windows.MessageBox.Show(
                    $"Erreur lors de la suppression: {ex.Message}",
                    "Erreur",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        /// <summary>
        /// Handler automatique pour les changements de propriétés
        /// </summary>
        partial void OnSearchTextChanged(string oldValue, string newValue)
        {
            SearchSalles();
        }
    }
}
