using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using ReserV6.Models;

namespace ReserV6.Converters
{
    /// <summary>
    /// Convertisseur pour gérer la sélection multiple d'équipements
    /// </summary>
    public class EquipementSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ObservableCollection<Equipement> selectedEquipements || parameter is not Equipement equipement)
            {
                return false;
            }

            return selectedEquipements.Any(e => e.Id == equipement.Id);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Not used for this binding
            return null!;
        }
    }
}
