namespace ReserV6.Models
{
    /// <summary>
    /// Wrapper pour un équipement avec son état de sélection
    /// Permet le binding MVVM pur sans code-behind
    /// </summary>
    public partial class EquipementSelectionItem : ObservableObject
    {
        public Equipement Equipement { get; }

        [ObservableProperty]
        private bool isSelected;

        public EquipementSelectionItem(Equipement equipement, bool isSelected = false)
        {
            Equipement = equipement;
            IsSelected = isSelected;
        }
    }
}

