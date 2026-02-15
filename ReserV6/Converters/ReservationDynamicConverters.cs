using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ReserV6.Models;

namespace ReserV6.Converters
{
    /// <summary>
    /// Converter pour afficher le statut dynamique d'une réservation basé sur les dates actuelles
    /// </summary>
    public class ReservationStatusDynamicConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ReservationComplete reservation)
                return value;

            var now = DateTime.Now;
            var creneau = reservation.CreneauDebut; // Récupérer depuis la réservation
            var creneauFin = reservation.CreneauFin;

            // Logique identique à ReservationStatusService
            if (now >= creneauFin)
            {
                return "Terminee";
            }
            else if (now >= creneau && now < creneauFin)
            {
                return "EnCours";
            }
            else if (reservation.Statut == "Annulee")
            {
                return "Annulee";
            }
            else
            {
                return reservation.Statut; // Confirmée ou EnAttente
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter pour la couleur de fond d'une ligne selon le statut
    /// </summary>
    public class ReservationRowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ReservationComplete reservation)
                return System.Windows.Media.Brushes.Transparent;

            var now = DateTime.Now;
            var creneau = reservation.CreneauDebut;
            var creneauFin = reservation.CreneauFin;

            string status;
            if (now >= creneauFin)
                status = "Terminee";
            else if (now >= creneau && now < creneauFin)
                status = "EnCours";
            else if (reservation.Statut == "Annulee")
                status = "Annulee";
            else
                status = reservation.Statut;

            // Retourner une couleur basée sur le statut
            return status switch
            {
                "EnCours" => new SolidColorBrush(Color.FromRgb(200, 255, 200)), // Vert clair
                "Terminee" => new SolidColorBrush(Color.FromRgb(220, 220, 220)), // Gris clair
                "Annulee" => new SolidColorBrush(Color.FromRgb(255, 200, 200)), // Rouge clair
                _ => Brushes.White // Blanc pour Confirmée/EnAttente
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter pour déterminer si une réservation peut être éditée
    /// </summary>
    public class ReservationCanEditConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ReservationComplete reservation)
                return false;

            var now = DateTime.Now;

            // Peut éditer seulement si:
            // 1. La réservation n'est pas Annulée
            // 2. La réservation n'est pas Terminée
            // 3. La réservation n'est pas EnCours
            return reservation.Statut != "Annulee" && 
                   now < reservation.CreneauDebut;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter pour la couleur de badge du statut
    /// </summary>
    public class ReservationStatusBadgeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ReservationComplete reservation)
                return Brushes.Gray;

            var now = DateTime.Now;
            var creneauFin = reservation.CreneauFin;
            var creneauDebut = reservation.CreneauDebut;

            string status;
            if (now >= creneauFin)
                status = "Terminee";
            else if (now >= creneauDebut && now < creneauFin)
                status = "EnCours";
            else if (reservation.Statut == "Annulee")
                status = "Annulee";
            else
                status = reservation.Statut;

            // Retourner une couleur basée sur le statut pour le badge
            return status switch
            {
                "EnCours" => new SolidColorBrush(Color.FromRgb(59, 193, 168)), // Teal vif
                "Terminee" => new SolidColorBrush(Color.FromRgb(191, 198, 196)), // Gris
                "Annulee" => new SolidColorBrush(Color.FromRgb(242, 96, 118)), // Rose/Rouge
                "Confirmee" => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Bleu
                "EnAttente" => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                _ => Brushes.Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
