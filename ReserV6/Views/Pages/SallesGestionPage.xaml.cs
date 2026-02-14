using ReserV6.ViewModels.Pages;
using ReserV6.Models;
using Wpf.Ui.Abstractions.Controls;
using System.Windows;
using System.Windows.Controls;

namespace ReserV6.Views.Pages
{
    /// <summary>
    /// Page pour la gestion des salles (ajout, modification, suppression)
    /// </summary>
    public partial class SallesGestionPage : INavigableView<SallesGestionViewModel>
    {
        public SallesGestionViewModel ViewModel { get; }

        public SallesGestionPage(SallesGestionViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        /// <summary>
        /// Event handler pour le bouton Éditer dans le DataGrid
        /// </summary>
        private void OnEditSalleClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Salle salle)
            {
                System.Diagnostics.Debug.WriteLine($"OnEditSalleClick: Salle {salle.Id} - {salle.Nom}");
                ViewModel.EditSalleCommand.Execute(salle);
            }
        }

        /// <summary>
        /// Event handler pour le bouton Supprimer dans le DataGrid
        /// </summary>
        private async void OnDeleteSalleClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Salle salle)
            {
                System.Diagnostics.Debug.WriteLine($"OnDeleteSalleClick: Salle {salle.Id} - {salle.Nom}");
                await ViewModel.DeleteSalleCommand.ExecuteAsync(salle);
            }
        }

        /// <summary>
        /// Event handler pour le bouton Éditer équipement dans le DataGrid des équipements
        /// </summary>
        private void OnEditEquipementClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Equipement equipement)
            {
                System.Diagnostics.Debug.WriteLine($"OnEditEquipementClick: Equipement {equipement.Id} - {equipement.Nom}");
                ViewModel.EditEquipementCommand.Execute(equipement);
            }
        }

        /// <summary>
        /// Event handler pour le bouton Supprimer équipement dans le DataGrid des équipements
        /// </summary>
        private async void OnDeleteEquipementClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Equipement equipement)
            {
                System.Diagnostics.Debug.WriteLine($"OnDeleteEquipementClick: Equipement {equipement.Id} - {equipement.Nom}");
                await ViewModel.DeleteEquipementCommand.ExecuteAsync(equipement);
            }
        }
    }
}
