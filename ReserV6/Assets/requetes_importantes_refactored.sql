-- ============================================
-- REQUÊTES IMPORTANTES
-- Système de Réservation de Salles
-- VERSION avec créneaux DATETIME flexibles
-- ============================================

-- ====================
-- 1. CONSULTER LES DISPONIBILITÉS
-- ====================

-- 1.1 Trouver toutes les salles disponibles pour une date donnée
SELECT DISTINCT s.id, s.nom, s.capacite, s.type, s.etage
FROM Salle s
WHERE s.disponibilite = 1
  AND s.id NOT IN (
    SELECT r.salle_id 
    FROM Reservation r
    JOIN Creneau c ON r.creneau_id = c.id
    WHERE DATE(c.debut) <= '2026-02-12' 
      AND DATE(c.fin) >= '2026-02-12'
      AND r.statut IN ('Confirmée', 'En attente')
  )
ORDER BY s.etage, s.capacite;

-- 1.2 Trouver les créneaux libres pour une salle spécifique
SELECT c.id, c.debut, c.fin,
       DATE(c.debut) AS date_debut,
       DATE(c.fin) AS date_fin,
       TIME(c.debut) AS heure_debut,
       TIME(c.fin) AS heure_fin,
       CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER) AS duree_heures
FROM Creneau c
WHERE c.id NOT IN (
    SELECT creneau_id 
    FROM Reservation 
    WHERE salle_id = 1
      AND statut IN ('Confirmée', 'En attente')
)
ORDER BY c.debut;

-- 1.3 Vérifier la disponibilité d'une salle pour un créneau spécifique
SELECT COUNT(*) AS est_disponible
FROM Creneau c
WHERE c.id = 5
  AND NOT EXISTS (
    SELECT 1 FROM Reservation r
    WHERE r.creneau_id = c.id
      AND r.salle_id = 2
      AND r.statut IN ('Confirmée', 'En attente')
  );

-- 1.4 Trouver les salles disponibles à un étage spécifique pour une période
SELECT DISTINCT s.id, s.nom, s.capacite, s.type, s.etage
FROM Salle s
WHERE s.disponibilite = 1
  AND s.etage = 1
  AND s.id NOT IN (
    SELECT r.salle_id 
    FROM Reservation r
    JOIN Creneau c ON r.creneau_id = c.id
    WHERE c.debut < '2026-02-12 18:00:00'
      AND c.fin > '2026-02-12 08:00:00'
      AND r.statut IN ('Confirmée', 'En attente')
  )
ORDER BY s.capacite;

-- 1.5 Recherche par étage et capacité minimale
SELECT s.id, s.nom, s.capacite, s.type, s.etage
FROM Salle s
WHERE s.disponibilite = 1
  AND s.etage IN (0, 1)
  AND s.capacite >= 30
ORDER BY s.etage, s.capacite;

-- 1.6 NOUVEAU: Vérifier disponibilité pour une période spécifique (chevauchement)
-- Vérifie si une salle est disponible entre deux dates/heures
SELECT s.id, s.nom, s.capacite, s.etage
FROM Salle s
WHERE s.disponibilite = 1
  AND s.id NOT IN (
    SELECT r.salle_id
    FROM Reservation r
    JOIN Creneau c ON r.creneau_id = c.id
    WHERE r.statut IN ('Confirmée', 'En attente')
      AND c.debut < '2026-02-15 18:00:00'  -- Fin de ma période souhaitée
      AND c.fin > '2026-02-15 08:00:00'    -- Début de ma période souhaitée
  )
ORDER BY s.etage, s.capacite;

-- ====================
-- 2. CRÉER UNE RÉSERVATION
-- ====================

-- 2.1 Vérifier qu'il n'y a pas de conflit de chevauchement
-- Pour une nouvelle réservation du 2026-02-13 09:00 au 2026-02-13 11:00 sur la salle 2
SELECT COUNT(*) AS conflits
FROM Reservation r
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.salle_id = 2
  AND r.statut IN ('Confirmée', 'En attente')
  AND c.debut < '2026-02-13 11:00:00'
  AND c.fin > '2026-02-13 09:00:00';

-- 2.2 Créer un nouveau créneau
INSERT INTO Creneau (debut, fin)
VALUES ('2026-02-13 09:00:00', '2026-02-13 11:00:00');

-- 2.3 Insérer une nouvelle réservation
INSERT INTO Reservation (dateReservation, motif, statut, user_id, salle_id, creneau_id)
VALUES (DATE('now'), 'Cours de programmation', 'En attente', 1, 2, last_insert_rowid());

-- 2.4 Enregistrer dans l'historique
INSERT INTO Historique (action, dateAction, reservation_id)
VALUES ('Réservation créée', DATETIME('now'), last_insert_rowid());

-- 2.5 NOUVEAU: Créer une réservation sur plusieurs jours
BEGIN TRANSACTION;

INSERT INTO Creneau (debut, fin)
VALUES ('2026-02-18 09:00:00', '2026-02-20 17:00:00');

INSERT INTO Reservation (dateReservation, motif, statut, user_id, salle_id, creneau_id)
VALUES (DATE('now'), 'Séminaire de recherche - 3 jours', 'En attente', 1, 5, last_insert_rowid());

INSERT INTO Historique (action, dateAction, reservation_id)
VALUES ('Réservation créée (multi-jours)', DATETIME('now'), last_insert_rowid());

COMMIT;

-- ====================
-- 3. MODIFIER UNE RÉSERVATION
-- ====================

-- 3.1 Confirmer une réservation
BEGIN TRANSACTION;

UPDATE Reservation 
SET statut = 'Confirmée' 
WHERE id = 4 AND statut = 'En attente';

INSERT INTO Historique (action, dateAction, reservation_id)
VALUES ('Réservation confirmée', DATETIME('now'), 4);

COMMIT;

-- 3.2 Modifier le créneau (vérifier les conflits)
BEGIN TRANSACTION;

UPDATE Reservation 
SET creneau_id = 14 
WHERE id = 8 
  AND NOT EXISTS (
    SELECT 1 
    FROM Reservation r
    JOIN Creneau c_new ON c_new.id = 14
    JOIN Creneau c_old ON c_old.id = (SELECT creneau_id FROM Reservation WHERE id = 8)
    WHERE r.salle_id = (SELECT salle_id FROM Reservation WHERE id = 8)
      AND r.creneau_id IN (
        SELECT c.id FROM Creneau c
        WHERE c.debut < c_new.fin AND c.fin > c_new.debut
      )
      AND r.statut IN ('Confirmée', 'En attente')
      AND r.id != 8
  );

INSERT INTO Historique (action, dateAction, reservation_id)
VALUES ('Créneau modifié', DATETIME('now'), 8);

COMMIT;

-- 3.3 Prolonger un créneau existant
UPDATE Creneau 
SET fin = DATETIME(fin, '+2 hours')
WHERE id = 5;

-- ====================
-- 4. ANNULER UNE RÉSERVATION
-- ====================

BEGIN TRANSACTION;

UPDATE Reservation 
SET statut = 'Annulée' 
WHERE id = 4;

INSERT INTO Historique (action, dateAction, reservation_id)
VALUES ('Réservation annulée', DATETIME('now'), 4);

COMMIT;

-- ====================
-- 5. CONSULTER L'HISTORIQUE
-- ====================

-- 5.1 Historique d'une réservation
SELECT 
    h.id, 
    h.action, 
    h.dateAction, 
    r.motif, 
    u.nom AS utilisateur, 
    s.nom AS salle,
    s.etage,
    c.debut,
    c.fin
FROM Historique h
JOIN Reservation r ON h.reservation_id = r.id
JOIN User u ON r.user_id = u.id
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.id = 1
ORDER BY h.dateAction;

-- 5.2 Historique d'un utilisateur
SELECT 
    h.action, 
    h.dateAction, 
    r.motif, 
    s.nom AS salle,
    s.etage,
    c.debut,
    c.fin,
    r.statut
FROM Historique h
JOIN Reservation r ON h.reservation_id = r.id
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.user_id = 1
ORDER BY h.dateAction DESC
LIMIT 20;

-- ====================
-- 6. GÉRER LES SALLES
-- ====================

-- 6.1 Créer une salle
INSERT INTO Salle (nom, capacite, type, etage, disponibilite)
VALUES ('Salle de cours 103', 35, 'Salle de cours', 1, 1);

-- 6.2 Modifier une salle
UPDATE Salle 
SET capacite = 40, etage = 2 
WHERE id = 8;

-- 6.3 Rendre indisponible
UPDATE Salle 
SET disponibilite = 0 
WHERE id = 7;

-- 6.4 Lister toutes les salles
SELECT 
    s.id,
    s.nom,
    s.capacite,
    s.type,
    s.etage,
    CASE WHEN s.disponibilite = 1 THEN 'Disponible' ELSE 'Indisponible' END AS statut,
    COUNT(DISTINCT e.id) AS nb_equipements
FROM Salle s
LEFT JOIN Equipement e ON s.id = e.salle_id
GROUP BY s.id
ORDER BY s.etage, s.nom;

-- 6.5 Lister les salles par étage
SELECT 
    s.etage,
    COUNT(*) AS nombre_salles,
    SUM(s.capacite) AS capacite_totale,
    AVG(s.capacite) AS capacite_moyenne
FROM Salle s
WHERE s.disponibilite = 1
GROUP BY s.etage
ORDER BY s.etage;

-- ====================
-- 7. GÉRER LES ÉQUIPEMENTS
-- ====================

-- 7.1 Ajouter un équipement
INSERT INTO Equipement (nom, description, type, estFonctionnel, salle_id)
VALUES ('Caméra HD', 'Logitech 4K', 'Visioconférence', 1, 5);

-- 7.2 Marquer comme non fonctionnel
UPDATE Equipement 
SET estFonctionnel = 0 
WHERE id = 10;

-- 7.3 Déplacer vers une autre salle
UPDATE Equipement 
SET salle_id = 8 
WHERE id = 11;

-- ====================
-- 8. STATISTIQUES
-- ====================

-- 8.1 Taux d'occupation des salles
SELECT 
    s.nom,
    s.etage,
    COUNT(r.id) AS total_reservations,
    SUM(CASE WHEN r.statut = 'Confirmée' THEN 1 ELSE 0 END) AS confirmees,
    ROUND(CAST(SUM(CASE WHEN r.statut = 'Confirmée' THEN 1 ELSE 0 END) AS FLOAT) / 
          NULLIF(COUNT(r.id), 0) * 100, 2) AS taux_confirmation
FROM Salle s
LEFT JOIN Reservation r ON s.id = r.salle_id
GROUP BY s.id
ORDER BY s.etage, total_reservations DESC;

-- 8.2 Salles les plus réservées
SELECT 
    s.nom,
    s.type,
    s.etage,
    COUNT(r.id) AS nombre_reservations,
    SUM(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER)) AS heures_totales
FROM Salle s
LEFT JOIN Reservation r ON s.id = r.salle_id
LEFT JOIN Creneau c ON r.creneau_id = c.id
WHERE r.statut = 'Confirmée'
GROUP BY s.id
ORDER BY nombre_reservations DESC
LIMIT 5;

-- 8.3 Utilisateurs les plus actifs
SELECT 
    u.nom,
    u.email,
    COUNT(r.id) AS nombre_reservations,
    SUM(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER)) AS heures_reservees
FROM User u
JOIN Reservation r ON u.id = r.user_id
JOIN Creneau c ON r.creneau_id = c.id
GROUP BY u.id
ORDER BY nombre_reservations DESC
LIMIT 10;

-- 8.4 Répartition par statut
SELECT 
    statut,
    COUNT(*) AS nombre,
    ROUND(CAST(COUNT(*) AS FLOAT) / (SELECT COUNT(*) FROM Reservation) * 100, 2) AS pourcentage
FROM Reservation
GROUP BY statut;

-- 8.5 Statistiques par étage
SELECT 
    s.etage,
    COUNT(DISTINCT s.id) AS nombre_salles,
    COUNT(r.id) AS total_reservations,
    SUM(CASE WHEN r.statut = 'Confirmée' THEN 1 ELSE 0 END) AS confirmees,
    ROUND(AVG(s.capacite), 2) AS capacite_moyenne
FROM Salle s
LEFT JOIN Reservation r ON s.id = r.salle_id
GROUP BY s.etage
ORDER BY s.etage;

-- 8.6 NOUVEAU: Durée moyenne des réservations
SELECT 
    AVG(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS REAL)) AS duree_moyenne_heures,
    MIN(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS REAL)) AS duree_min_heures,
    MAX(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS REAL)) AS duree_max_heures
FROM Creneau c
JOIN Reservation r ON c.id = r.creneau_id
WHERE r.statut = 'Confirmée';

-- 8.7 NOUVEAU: Réservations par durée
SELECT 
    CASE 
        WHEN CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER) <= 2 THEN '0-2h'
        WHEN CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER) <= 4 THEN '2-4h'
        WHEN CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER) <= 8 THEN '4-8h'
        WHEN CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER) <= 24 THEN '8-24h'
        ELSE '> 24h (multi-jours)'
    END AS categorie_duree,
    COUNT(*) AS nombre_reservations
FROM Reservation r
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.statut = 'Confirmée'
GROUP BY categorie_duree
ORDER BY 
    CASE categorie_duree
        WHEN '0-2h' THEN 1
        WHEN '2-4h' THEN 2
        WHEN '4-8h' THEN 3
        WHEN '8-24h' THEN 4
        ELSE 5
    END;

-- ====================
-- 9. RECHERCHES AVANCÉES
-- ====================

-- 9.1 Salles avec équipements spécifiques
SELECT DISTINCT 
    s.id, 
    s.nom, 
    s.capacite,
    s.etage,
    GROUP_CONCAT(DISTINCT e.nom, ', ') AS equipements
FROM Salle s
JOIN Equipement e ON s.id = e.salle_id
WHERE s.capacite >= 30
  AND s.disponibilite = 1
  AND e.type = 'Vidéoprojecteur'
  AND e.estFonctionnel = 1
GROUP BY s.id
ORDER BY s.etage;

-- 9.2 Réservations à venir
SELECT 
    r.id,
    s.nom AS salle,
    s.etage,
    c.debut,
    c.fin,
    DATE(c.debut) AS date_debut,
    DATE(c.fin) AS date_fin,
    CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) AS INTEGER) AS nb_jours,
    r.motif,
    r.statut
FROM Reservation r
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.user_id = 1
  AND c.debut >= DATETIME('now')
  AND r.statut IN ('Confirmée', 'En attente')
ORDER BY c.debut;

-- 9.3 Détecter les conflits (chevauchements)
SELECT 
    r1.id AS reservation1,
    r2.id AS reservation2,
    s.nom AS salle,
    s.etage,
    c1.debut AS debut1,
    c1.fin AS fin1,
    c2.debut AS debut2,
    c2.fin AS fin2
FROM Reservation r1
JOIN Reservation r2 ON r1.salle_id = r2.salle_id AND r1.id < r2.id
JOIN Salle s ON r1.salle_id = s.id
JOIN Creneau c1 ON r1.creneau_id = c1.id
JOIN Creneau c2 ON r2.creneau_id = c2.id
WHERE r1.statut IN ('Confirmée', 'En attente')
  AND r2.statut IN ('Confirmée', 'En attente')
  AND c1.debut < c2.fin
  AND c1.fin > c2.debut;

-- 9.4 Recherche multicritère (étage, capacité, équipements)
SELECT DISTINCT 
    s.id,
    s.nom,
    s.etage,
    s.capacite,
    s.type,
    COUNT(DISTINCT e.id) AS nb_equipements,
    GROUP_CONCAT(DISTINCT e.type, ', ') AS types_equipements
FROM Salle s
LEFT JOIN Equipement e ON s.id = e.salle_id AND e.estFonctionnel = 1
WHERE s.disponibilite = 1
  AND s.etage IN (1, 2)
  AND s.capacite >= 25
GROUP BY s.id
HAVING nb_equipements > 0
ORDER BY s.etage, s.capacite DESC;

-- 9.5 NOUVEAU: Trouver des alternatives pour une période occupée
-- Trouver des salles similaires disponibles quand la salle 1 est occupée
SELECT 
    s.id,
    s.nom,
    s.capacite,
    s.etage,
    ABS(s.capacite - (SELECT capacite FROM Salle WHERE id = 1)) AS diff_capacite
FROM Salle s
WHERE s.id != 1
  AND s.type = (SELECT type FROM Salle WHERE id = 1)
  AND s.disponibilite = 1
  AND s.id NOT IN (
    SELECT r.salle_id
    FROM Reservation r
    JOIN Creneau c ON r.creneau_id = c.id
    WHERE r.statut IN ('Confirmée', 'En attente')
      AND c.debut < '2026-02-12 12:00:00'
      AND c.fin > '2026-02-12 08:00:00'
  )
ORDER BY diff_capacite, ABS(s.etage - (SELECT etage FROM Salle WHERE id = 1))
LIMIT 5;

-- ====================
-- 10. NOTIFICATIONS
-- ====================

-- 10.1 Réservations démarrant dans les 24h
SELECT 
    u.nom,
    u.email,
    s.nom AS salle,
    s.etage,
    c.debut,
    c.fin,
    r.motif
FROM Reservation r
JOIN User u ON r.user_id = u.id
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.statut = 'Confirmée'
  AND c.debut BETWEEN DATETIME('now') AND DATETIME('now', '+1 day');

-- 10.2 Réservations en attente > 48h
SELECT 
    r.id,
    u.nom,
    s.nom AS salle,
    s.etage,
    r.dateReservation,
    c.debut,
    c.fin,
    CAST(JULIANDAY('now') - JULIANDAY(r.dateReservation) AS INTEGER) AS jours_attente
FROM Reservation r
JOIN User u ON r.user_id = u.id
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.statut = 'En attente'
  AND JULIANDAY('now') - JULIANDAY(r.dateReservation) > 2;

-- 10.3 Réservations en cours
SELECT 
    u.nom,
    s.nom AS salle,
    s.etage,
    c.debut,
    c.fin,
    r.motif,
    CAST((JULIANDAY(c.fin) - JULIANDAY('now')) * 24 AS INTEGER) AS heures_restantes
FROM Reservation r
JOIN User u ON r.user_id = u.id
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.statut = 'Confirmée'
  AND DATETIME('now') BETWEEN c.debut AND c.fin
ORDER BY s.etage, c.fin;

-- 10.4 NOUVEAU: Réservations multi-jours à venir
SELECT 
    u.nom,
    s.nom AS salle,
    s.etage,
    c.debut,
    c.fin,
    CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) AS INTEGER) AS nb_jours,
    r.motif
FROM Reservation r
JOIN User u ON r.user_id = u.id
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.statut = 'Confirmée'
  AND c.debut >= DATETIME('now')
  AND CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) AS INTEGER) >= 1
ORDER BY c.debut;

-- ====================
-- 11. GESTION
-- ====================

-- 11.1 Réservations à traiter
SELECT 
    r.id,
    u.nom AS demandeur,
    s.nom AS salle,
    s.etage,
    c.debut,
    c.fin,
    CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) AS INTEGER) AS nb_jours,
    r.motif,
    CAST(JULIANDAY('now') - JULIANDAY(r.dateReservation) AS INTEGER) AS attente_jours
FROM Reservation r
JOIN User u ON r.user_id = u.id
JOIN Salle s ON r.salle_id = s.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE r.statut = 'En attente'
ORDER BY r.dateReservation;

-- 11.2 Salles sous-utilisées
SELECT 
    s.nom,
    s.etage,
    s.capacite,
    COUNT(r.id) AS nombre_reservations,
    SUM(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER)) AS heures_reservees
FROM Salle s
LEFT JOIN Reservation r ON s.id = r.salle_id AND r.statut = 'Confirmée'
LEFT JOIN Creneau c ON r.creneau_id = c.id
GROUP BY s.id
HAVING nombre_reservations < 5
ORDER BY s.etage, nombre_reservations;

-- 11.3 Occupation par étage
SELECT 
    s.etage,
    CASE s.etage
        WHEN 0 THEN 'Rez-de-chaussée'
        WHEN 1 THEN '1er étage'
        WHEN 2 THEN '2ème étage'
        WHEN 3 THEN '3ème étage'
        ELSE s.etage || 'ème étage'
    END AS niveau,
    COUNT(DISTINCT s.id) AS nb_salles,
    COUNT(r.id) AS nb_reservations,
    SUM(CASE WHEN r.statut = 'Confirmée' THEN 1 ELSE 0 END) AS confirmees
FROM Salle s
LEFT JOIN Reservation r ON s.id = r.salle_id
GROUP BY s.etage
ORDER BY s.etage;

-- ====================
-- 12. VUES
-- ====================

-- 12.1 Réservations complètes
SELECT * FROM v_reservations_completes
WHERE statut = 'Confirmée'
ORDER BY etage, creneau_debut;

-- 12.2 Salles avec équipements
SELECT * FROM v_salles_equipements
WHERE disponibilite = 1
ORDER BY etage, capacite DESC;

-- 12.3 Statistiques utilisateurs
SELECT * FROM v_statistiques_utilisateurs
WHERE total_reservations > 0
ORDER BY total_reservations DESC;

-- ====================
-- 13. REQUÊTES LIÉES À L'ÉTAGE ET PÉRIODES FLEXIBLES
-- ====================

-- 13.1 Trouver la salle la plus proche (même étage ou étages adjacents)
SELECT 
    s.id,
    s.nom,
    s.etage,
    s.capacite,
    ABS(s.etage - (SELECT etage FROM Salle WHERE id = 4)) AS difference_etage
FROM Salle s
WHERE s.disponibilite = 1
  AND s.id != 4
ORDER BY difference_etage, s.capacite
LIMIT 5;

-- 13.2 Accessibilité - Salles au RDC
SELECT 
    s.id,
    s.nom,
    s.capacite,
    s.type,
    COUNT(e.id) AS nb_equipements
FROM Salle s
LEFT JOIN Equipement e ON s.id = e.salle_id AND e.estFonctionnel = 1
WHERE s.etage = 0
  AND s.disponibilite = 1
GROUP BY s.id
ORDER BY s.capacite DESC;

-- 13.3 Planning d'un étage pour une période
SELECT 
    s.nom AS salle,
    c.debut,
    c.fin,
    TIME(c.debut) AS heure_debut,
    TIME(c.fin) AS heure_fin,
    r.motif,
    u.nom AS utilisateur,
    r.statut
FROM Salle s
JOIN Reservation r ON s.id = r.salle_id
JOIN Creneau c ON r.creneau_id = c.id
JOIN User u ON r.user_id = u.id
WHERE s.etage = 1
  AND DATE(c.debut) <= '2026-02-12'
  AND DATE(c.fin) >= '2026-02-12'
  AND r.statut IN ('Confirmée', 'En attente')
ORDER BY s.nom, c.debut;

-- 13.4 NOUVEAU: Réservations qui se chevauchent avec une période donnée
-- Exemple: trouver toutes les réservations qui touchent la période du 15 au 17 février
SELECT 
    r.id,
    s.nom AS salle,
    s.etage,
    u.nom AS utilisateur,
    c.debut,
    c.fin,
    r.motif,
    r.statut
FROM Reservation r
JOIN Salle s ON r.salle_id = s.id
JOIN User u ON r.user_id = u.id
JOIN Creneau c ON r.creneau_id = c.id
WHERE c.debut < '2026-02-17 23:59:59'
  AND c.fin > '2026-02-15 00:00:00'
  AND r.statut IN ('Confirmée', 'En attente')
ORDER BY s.etage, c.debut;

-- 13.5 NOUVEAU: Créneaux disponibles pour une salle sur une semaine
SELECT 
    DATE(datetime_series.value) AS jour,
    COUNT(CASE 
        WHEN NOT EXISTS (
            SELECT 1 FROM Reservation r2
            JOIN Creneau c2 ON r2.creneau_id = c2.id
            WHERE r2.salle_id = 1
              AND r2.statut IN ('Confirmée', 'En attente')
              AND DATE(c2.debut) <= DATE(datetime_series.value)
              AND DATE(c2.fin) >= DATE(datetime_series.value)
        ) THEN 1 
    END) AS disponible
FROM (
    SELECT DATE('2026-02-10') AS value
    UNION SELECT DATE('2026-02-11')
    UNION SELECT DATE('2026-02-12')
    UNION SELECT DATE('2026-02-13')
    UNION SELECT DATE('2026-02-14')
    UNION SELECT DATE('2026-02-15')
    UNION SELECT DATE('2026-02-16')
) AS datetime_series
GROUP BY jour
ORDER BY jour;
