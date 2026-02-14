// Exemples d'utilisation du ConflictResolutionService

using ReserV6.Services;
using ReserV6.Models;

namespace ReserV6.Examples
{
    /// <summary>
    /// Exemples d'utilisation du service de résolution des conflits
    /// </summary>
    public class ConflictResolutionServiceExamples
    {
        private readonly ConflictResolutionService _conflictService;

        public ConflictResolutionServiceExamples(RepositoryManager repositoryManager)
        {
            _conflictService = new ConflictResolutionService(repositoryManager);
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 1: Vérification simple d'un créneau pré-défini
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example1_CheckCreneauConflict()
        {
            Console.WriteLine("=== EXEMPLE 1: Vérifier un créneau pré-défini ===\n");

            int salleId = 1;
            int creneauId = 5;

            bool hasConflict = _conflictService.CheckCreneauConflict(salleId, creneauId);

            if (hasConflict)
            {
                Console.WriteLine($"❌ Le créneau {creneauId} est déjà réservé pour la salle {salleId}");
            }
            else
            {
                Console.WriteLine($"✅ Le créneau {creneauId} est disponible pour la salle {salleId}");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 2: Vérification d'une plage horaire personnalisée
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example2_CheckTimeConflict()
        {
            Console.WriteLine("=== EXEMPLE 2: Vérifier une plage horaire personnalisée ===\n");

            int salleId = 1;
            DateTime startTime = new DateTime(2024, 1, 15, 09, 0, 0);  // 09:00
            DateTime endTime = new DateTime(2024, 1, 15, 10, 30, 0);   // 10:30

            bool hasConflict = _conflictService.CheckTimeConflict(salleId, startTime, endTime);

            if (hasConflict)
            {
                Console.WriteLine($"❌ Conflit détecté: La salle est occupée entre {startTime:HH:mm} et {endTime:HH:mm}");
            }
            else
            {
                Console.WriteLine($"✅ OK: La salle est disponible entre {startTime:HH:mm} et {endTime:HH:mm}");
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 3: Obtenir les réservations conflictuelles
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example3_GetConflictingReservations()
        {
            Console.WriteLine("=== EXEMPLE 3: Lister les réservations conflictuelles ===\n");

            int salleId = 1;
            DateTime startTime = new DateTime(2024, 1, 15, 09, 0, 0);
            DateTime endTime = new DateTime(2024, 1, 15, 11, 0, 0);

            var conflictingReservations = _conflictService.GetConflictingReservations(salleId, startTime, endTime);

            if (conflictingReservations.Count == 0)
            {
                Console.WriteLine("✅ Aucune réservation conflictuelle");
            }
            else
            {
                Console.WriteLine($"❌ {conflictingReservations.Count} réservation(s) en conflit:\n");

                foreach (var reservation in conflictingReservations)
                {
                    if (reservation.Creneau != null)
                    {
                        Console.WriteLine($"  • {reservation.Creneau.Debut:dd/MM/yyyy HH:mm} - {reservation.Creneau.Fin:HH:mm}");
                        Console.WriteLine($"    Motif: {reservation.Motif}");
                        Console.WriteLine($"    Utilisateur: {reservation.User?.Nom}");
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 4: Vérifier la disponibilité globale
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example4_IsSalleAvailable()
        {
            Console.WriteLine("=== EXEMPLE 4: Vérifier la disponibilité globale ===\n");

            int salleId = 2;
            DateTime startTime = new DateTime(2024, 1, 15, 14, 0, 0);
            DateTime endTime = new DateTime(2024, 1, 15, 15, 0, 0);

            bool isAvailable = _conflictService.IsSalleAvailable(salleId, startTime, endTime);

            Console.WriteLine($"Salle {salleId} pour {startTime:dd/MM/yyyy} de {startTime:HH:mm} à {endTime:HH:mm}:");
            Console.WriteLine(isAvailable ? "✅ Disponible" : "❌ Non disponible");
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 5: Générer un message d'erreur détaillé
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example5_GenerateConflictErrorMessage()
        {
            Console.WriteLine("=== EXEMPLE 5: Générer message d'erreur détaillé ===\n");

            int salleId = 1;
            DateTime startTime = new DateTime(2024, 1, 15, 09, 0, 0);
            DateTime endTime = new DateTime(2024, 1, 15, 11, 0, 0);
            string salleName = "Salle de Conférence A";

            string errorMessage = _conflictService.GenerateConflictErrorMessage(salleId, startTime, endTime, salleName);

            Console.WriteLine(errorMessage);
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 6: Récupérer les créneaux libres
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example6_GetAvailableCreneaux()
        {
            Console.WriteLine("=== EXEMPLE 6: Récupérer les créneaux libres ===\n");

            int salleId = 1;
            DateTime date = new DateTime(2024, 1, 15);

            var availableCreneaux = _conflictService.GetAvailableCreneaux(salleId, date);

            Console.WriteLine($"Créneaux disponibles pour la salle {salleId} le {date:dd/MM/yyyy}:\n");

            if (availableCreneaux.Count == 0)
            {
                Console.WriteLine("Aucun créneau disponible");
            }
            else
            {
                foreach (var creneau in availableCreneaux)
                {
                    Console.WriteLine($"  • {creneau.Debut:HH:mm} - {creneau.Fin:HH:mm} ({creneau.Duree.TotalHours}h)");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 7: Obtenir le statut complet de disponibilité
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example7_GetSalleAvailabilityStatus()
        {
            Console.WriteLine("=== EXEMPLE 7: Obtenir le statut complet de disponibilité ===\n");

            int salleId = 1;
            DateTime startTime = new DateTime(2024, 1, 15, 09, 0, 0);
            DateTime endTime = new DateTime(2024, 1, 15, 11, 0, 0);

            var status = _conflictService.GetSalleAvailabilityStatus(salleId, startTime, endTime);

            Console.WriteLine($"Statut de disponibilité pour {startTime:dd/MM/yyyy HH:mm} - {endTime:HH:mm}:\n");
            Console.WriteLine($"  Disponible: {(status.IsAvailable ? "✅ Oui" : "❌ Non")}");
            Console.WriteLine($"  Nombre de conflits: {status.ConflictCount}");

            if (status.ConflictingReservations.Any())
            {
                Console.WriteLine("\n  Réservations en conflit:");
                foreach (var reservation in status.ConflictingReservations)
                {
                    if (reservation.Creneau != null)
                    {
                        Console.WriteLine($"    • {reservation.Creneau.Debut:HH:mm} - {reservation.Creneau.Fin:HH:mm}");
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 8: Validation complète avant création de réservation
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example8_CompleteValidationWorkflow()
        {
            Console.WriteLine("=== EXEMPLE 8: Workflow complet de validation ===\n");

            // Données de test
            int salleId = 1;
            string salleName = "Salle 101";
            DateTime startTime = new DateTime(2024, 1, 15, 10, 0, 0);
            DateTime endTime = new DateTime(2024, 1, 15, 11, 30, 0);
            string motif = "Réunion d'équipe";

            Console.WriteLine("📋 Données de la réservation:");
            Console.WriteLine($"  Salle: {salleName}");
            Console.WriteLine($"  Période: {startTime:dd/MM/yyyy HH:mm} - {endTime:HH:mm}");
            Console.WriteLine($"  Motif: {motif}\n");

            // Étape 1: Vérifier la disponibilité
            Console.WriteLine("Étape 1️⃣: Vérification de disponibilité...");
            var status = _conflictService.GetSalleAvailabilityStatus(salleId, startTime, endTime);

            if (!status.IsAvailable)
            {
                Console.WriteLine("❌ REFUSÉE - Conflits détectés:");
                Console.WriteLine(_conflictService.GenerateConflictErrorMessage(salleId, startTime, endTime, salleName));
                return;
            }

            Console.WriteLine("✅ Disponible\n");

            // Étape 2: Valider les données
            Console.WriteLine("Étape 2️⃣: Validation des données...");
            if (string.IsNullOrWhiteSpace(motif))
            {
                Console.WriteLine("❌ REFUSÉE - Motif manquant");
                return;
            }

            if (startTime >= endTime)
            {
                Console.WriteLine("❌ REFUSÉE - Dates/heures invalides");
                return;
            }

            Console.WriteLine("✅ Données valides\n");

            // Étape 3: Double-check avant création
            Console.WriteLine("Étape 3️⃣: Double-vérification avant création...");
            bool hasConflict = _conflictService.CheckTimeConflict(salleId, startTime, endTime);

            if (hasConflict)
            {
                Console.WriteLine("❌ REFUSÉE - Un conflit a été détecté");
                return;
            }

            Console.WriteLine("✅ Pas de conflit\n");

            // Étape 4: Création (simulée)
            Console.WriteLine("Étape 4️⃣: Création de la réservation...");
            Console.WriteLine("✅ RÉSERVATION CONFIRMÉE");
            Console.WriteLine($"\nID Réservation: #RES-2024-001");
            Console.WriteLine($"Salle: {salleName}");
            Console.WriteLine($"Période: {startTime:dd/MM/yyyy HH:mm} - {endTime:HH:mm}");
            Console.WriteLine($"Durée: {(endTime - startTime).TotalHours}h");
            Console.WriteLine($"Motif: {motif}");
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // EXEMPLE 9: Gestion des cas limites
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void Example9_EdgeCases()
        {
            Console.WriteLine("=== EXEMPLE 9: Cas limites ===\n");

            int salleId = 1;

            // Cas 1: Créneaux adjacents (pas de conflit)
            Console.WriteLine("Cas 1️⃣: Créneaux adjacents");
            var time1_Start = new DateTime(2024, 1, 15, 09, 0, 0);
            var time1_End = new DateTime(2024, 1, 15, 10, 0, 0);
            var time2_Start = new DateTime(2024, 1, 15, 10, 0, 0);
            var time2_End = new DateTime(2024, 1, 15, 11, 0, 0);

            bool conflict1 = _conflictService.CheckTimeConflict(salleId, time2_Start, time2_End);
            Console.WriteLine($"  09:00-10:00 (existant) vs 10:00-11:00 (demandé) = {(conflict1 ? "❌ Conflit" : "✅ OK")}\n");

            // Cas 2: Chevauchement total
            Console.WriteLine("Cas 2️⃣: Chevauchement total");
            var time3_Start = new DateTime(2024, 1, 15, 09, 0, 0);
            var time3_End = new DateTime(2024, 1, 15, 10, 0, 0);
            var time4_Start = new DateTime(2024, 1, 15, 08, 30, 0);
            var time4_End = new DateTime(2024, 1, 15, 10, 30, 0);

            bool conflict2 = _conflictService.CheckTimeConflict(salleId, time4_Start, time4_End);
            Console.WriteLine($"  09:00-10:00 (existant) vs 08:30-10:30 (demandé) = {(conflict2 ? "❌ Conflit" : "✅ OK")}\n");

            // Cas 3: Chevauchement partiel
            Console.WriteLine("Cas 3️⃣: Chevauchement partiel");
            var time5_Start = new DateTime(2024, 1, 15, 09, 0, 0);
            var time5_End = new DateTime(2024, 1, 15, 10, 0, 0);
            var time6_Start = new DateTime(2024, 1, 15, 09, 30, 0);
            var time6_End = new DateTime(2024, 1, 15, 10, 30, 0);

            bool conflict3 = _conflictService.CheckTimeConflict(salleId, time6_Start, time6_End);
            Console.WriteLine($"  09:00-10:00 (existant) vs 09:30-10:30 (demandé) = {(conflict3 ? "❌ Conflit" : "✅ OK")}\n");

            // Cas 4: Créneau inclus
            Console.WriteLine("Cas 4️⃣: Créneau inclus");
            var time7_Start = new DateTime(2024, 1, 15, 09, 0, 0);
            var time7_End = new DateTime(2024, 1, 15, 10, 0, 0);
            var time8_Start = new DateTime(2024, 1, 15, 09, 15, 0);
            var time8_End = new DateTime(2024, 1, 15, 09, 45, 0);

            bool conflict4 = _conflictService.CheckTimeConflict(salleId, time8_Start, time8_End);
            Console.WriteLine($"  09:00-10:00 (existant) vs 09:15-09:45 (demandé) = {(conflict4 ? "❌ Conflit" : "✅ OK")}");
        }

        // ═══════════════════════════════════════════════════════════════════════════════════════
        // Point d'entrée pour tester tous les exemples
        // ═══════════════════════════════════════════════════════════════════════════════════════
        public void RunAllExamples()
        {
            try
            {
                Example1_CheckCreneauConflict();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example2_CheckTimeConflict();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example3_GetConflictingReservations();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example4_IsSalleAvailable();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example5_GenerateConflictErrorMessage();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example6_GetAvailableCreneaux();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example7_GetSalleAvailabilityStatus();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example8_CompleteValidationWorkflow();
                Console.WriteLine("\n" + new string('=', 80) + "\n");

                Example9_EdgeCases();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur: {ex.Message}");
            }
        }
    }
}
