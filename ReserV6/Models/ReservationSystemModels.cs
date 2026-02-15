namespace ReserV6.Models
{
    /// <summary>
    /// Représente un utilisateur du système
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "User" ou "Admin"
        public DateTime DateCreation { get; set; }
    }

    /// <summary>
    /// Représente une salle réservable
    /// </summary>
    public class Salle
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacite { get; set; }
        public string Type { get; set; } = string.Empty; // "Amphithéâtre", "Laboratoire", "Salle de cours", etc.
        public int Etage { get; set; }
        public bool Disponibilite { get; set; } = true;
        public DateTime DateCreation { get; set; }

        public ICollection<Equipement> Equipements { get; set; } = new List<Equipement>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    /// <summary>
    /// Représente un équipement dans une salle
    /// </summary>
    public class Equipement
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Vidéoprojecteur", "Tableau interactif", etc.
        public bool EstFonctionnel { get; set; } = true;
        public int SalleId { get; set; }
        public DateTime DateCreation { get; set; }

        public Salle? Salle { get; set; }
    }

    /// <summary>
    /// Représente un créneau horaire
    /// </summary>
    public class Creneau
    {
        public int Id { get; set; }
        public DateTime Debut { get; set; }
        public DateTime Fin { get; set; }
        public DateTime DateCreation { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public TimeSpan Duree => Fin - Debut;
    }

    /// <summary>
    /// Représente une réservation de salle
    /// </summary>
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime DateReservation { get; set; }
        public string Motif { get; set; } = string.Empty;
        public ReservationStatut Statut { get; set; } = ReservationStatut.Confirmée;
        public int UserId { get; set; }
        public int SalleId { get; set; }
        public int? CreneauId { get; set; }

        // Nouvelles propriétés pour support multi-jours avec horaires spécifiques
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }

        public User? User { get; set; }
        public Salle? Salle { get; set; }
        public Creneau? Creneau { get; set; }
        public ICollection<Historique> Historiques { get; set; } = new List<Historique>();

        /// <summary>
        /// Propriétés calculées pour la plage horaire complète
        /// </summary>
        public DateTime DateTimeDebut => DateDebut.Date.Add(HeureDebut);
        public DateTime DateTimeFin => DateFin.Date.Add(HeureFin);
        public TimeSpan DureeTotal => DateTimeFin - DateTimeDebut;
    }

    /// <summary>
    /// Énumération des statuts de réservation
    /// </summary>
    public enum ReservationStatut
    {
        EnAttente,      // En attente de confirmation
        Confirmée,      // Confirmée et à venir
        EnCours,        // Actuellement en cours
        Terminée,       // Terminée (date de fin dépassée)
        Annulée         // Annulée
    }

    /// <summary>
    /// Représente une action historique sur une réservation
    /// </summary>
    public class Historique
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime DateAction { get; set; }
        public int ReservationId { get; set; }

        public Reservation? Reservation { get; set; }
    }

    /// <summary>
    /// DTO pour les réservations complètes avec toutes les informations
    /// </summary>
    public class ReservationComplete
    {
        public int Id { get; set; }
        public DateTime DateReservation { get; set; }
        public string Motif { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserNom { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public int SalleId { get; set; }
        public string SalleNom { get; set; } = string.Empty;
        public int Capacite { get; set; }
        public string SalleType { get; set; } = string.Empty;
        public int Etage { get; set; }

        // Nouveaux champs pour dates et heures séparées
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }

        // Propriétés héritées (pour compatibilité)
        public DateTime CreneauDebut => DateDebut.Date.Add(HeureDebut);
        public DateTime CreneauFin => DateFin.Date.Add(HeureFin);
        public int DureeHeures => (int)CreneauFin.Subtract(CreneauDebut).TotalHours;

        /// <summary>
        /// Retourne vrai si le statut est "terminal" (Annulée ou Terminée)
        /// </summary>
        public bool IsNotFinalStatus
        {
            get
            {
                return Statut != "Annulée" && Statut != "Terminée";
            }
        }
    }

    /// <summary>
    /// DTO pour les salles avec leurs équipements
    /// </summary>
    public class SalleAvecEquipements
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int Capacite { get; set; }
        public string Type { get; set; } = string.Empty;
        public int Etage { get; set; }
        public bool Disponibilite { get; set; }
        public int NbEquipementsFonctionnels { get; set; }
        public int NbEquipementsTotal { get; set; }
        public string Equipements { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO pour les statistiques utilisateur
    /// </summary>
    public class StatistiquesUtilisateur
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int TotalReservations { get; set; }
        public int Confirmees { get; set; }
        public int EnAttente { get; set; }
        public int Annulees { get; set; }
        public int HeuresTotales { get; set; }
    }
}
