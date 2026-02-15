-- ============================================
-- SYSTÈME DE RÉSERVATION DE SALLES
-- Initialisation de la Base de Données
-- ============================================

-- Table des Utilisateurs
CREATE TABLE IF NOT EXISTS "User" (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nom TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE,
    role TEXT NOT NULL CHECK(role IN ('User', 'Admin')),
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table des Salles
CREATE TABLE IF NOT EXISTS Salle (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nom TEXT NOT NULL UNIQUE,
    description TEXT,
    capacite INTEGER NOT NULL CHECK(capacite > 0),
    type TEXT NOT NULL,
    etage INTEGER NOT NULL,
    disponibilite INTEGER NOT NULL DEFAULT 1 CHECK(disponibilite IN (0, 1)),
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table des Équipements
CREATE TABLE IF NOT EXISTS Equipement (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nom TEXT NOT NULL,
    description TEXT,
    type TEXT NOT NULL,
    estFonctionnel INTEGER NOT NULL DEFAULT 1 CHECK(estFonctionnel IN (0, 1)),
    salle_id INTEGER NOT NULL,
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (salle_id) REFERENCES Salle(id) ON DELETE CASCADE
);

-- Table des Créneaux Horaires
CREATE TABLE IF NOT EXISTS Creneau (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    debut TEXT NOT NULL,
    fin TEXT NOT NULL,
    dateCreation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Table des Réservations
CREATE TABLE IF NOT EXISTS Reservation (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    dateReservation TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    motif TEXT,
    statut TEXT NOT NULL DEFAULT 'Confirmée' CHECK(statut IN ('En attente', 'Confirmée', 'Annulée', 'Terminée')),
    user_id INTEGER NOT NULL,
    salle_id INTEGER NOT NULL,
    creneau_id INTEGER,
    dateDebut TEXT NOT NULL,
    dateFin TEXT NOT NULL,
    heureDebut TEXT NOT NULL,
    heureFin TEXT NOT NULL,
    FOREIGN KEY (user_id) REFERENCES "User"(id) ON DELETE CASCADE,
    FOREIGN KEY (salle_id) REFERENCES Salle(id) ON DELETE CASCADE,
    FOREIGN KEY (creneau_id) REFERENCES Creneau(id) ON DELETE CASCADE
);

-- Table de l'Historique
CREATE TABLE IF NOT EXISTS Historique (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    action TEXT NOT NULL,
    dateAction TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    reservation_id INTEGER NOT NULL,
    FOREIGN KEY (reservation_id) REFERENCES Reservation(id) ON DELETE CASCADE
);

-- ============================================
-- VUES
-- ============================================

-- Vue: Réservations Complètes
CREATE VIEW IF NOT EXISTS v_reservations_completes AS
SELECT 
    r.id,
    r.dateReservation,
    r.motif,
    r.statut,
    u.id AS user_id,
    u.nom AS user_nom,
    u.email AS user_email,
    u.role AS user_role,
    s.id AS salle_id,
    s.nom AS salle_nom,
    s.capacite,
    s.type AS salle_type,
    s.etage,
    s.disponibilite,
    r.creneau_id,
    r.dateDebut,
    r.dateFin,
    r.heureDebut,
    r.heureFin,
    COALESCE(c.debut, datetime(r.dateDebut || ' ' || r.heureDebut)) AS creneau_debut,
    COALESCE(c.fin, datetime(r.dateFin || ' ' || r.heureFin)) AS creneau_fin,
    CAST((JULIANDAY(COALESCE(c.fin, datetime(r.dateFin || ' ' || r.heureFin))) - JULIANDAY(COALESCE(c.debut, datetime(r.dateDebut || ' ' || r.heureDebut)))) * 24 AS INTEGER) AS duree_heures
FROM Reservation r
JOIN "User" u ON r.user_id = u.id
JOIN Salle s ON r.salle_id = s.id
LEFT JOIN Creneau c ON r.creneau_id = c.id;

-- Vue: Salles avec Équipements
CREATE VIEW IF NOT EXISTS v_salles_equipements AS
SELECT 
    s.id,
    s.nom,
    s.capacite,
    s.type,
    s.etage,
    s.disponibilite,
    COUNT(DISTINCT CASE WHEN e.estFonctionnel = 1 THEN e.id END) AS nb_equipements_fonctionnels,
    COUNT(DISTINCT e.id) AS nb_equipements_total,
    GROUP_CONCAT(DISTINCT CASE WHEN e.estFonctionnel = 1 THEN e.nom END, ', ') AS equipements
FROM Salle s
LEFT JOIN Equipement e ON s.id = e.salle_id
GROUP BY s.id;

-- Vue: Statistiques Utilisateurs
CREATE VIEW IF NOT EXISTS v_statistiques_utilisateurs AS
SELECT 
    u.id,
    u.nom,
    u.email,
    u.role,
    COUNT(r.id) AS total_reservations,
    SUM(CASE WHEN r.statut = 'Confirmée' THEN 1 ELSE 0 END) AS confirmees,
    SUM(CASE WHEN r.statut = 'En attente' THEN 1 ELSE 0 END) AS en_attente,
    SUM(CASE WHEN r.statut = 'Annulée' THEN 1 ELSE 0 END) AS annulees,
    COALESCE(SUM(CAST((JULIANDAY(c.fin) - JULIANDAY(c.debut)) * 24 AS INTEGER)), 0) AS heures_totales
FROM "User" u
LEFT JOIN Reservation r ON u.id = r.user_id
LEFT JOIN Creneau c ON r.creneau_id = c.id
GROUP BY u.id;

-- ============================================
-- INDEX
-- ============================================

CREATE INDEX IF NOT EXISTS idx_reservation_user ON Reservation(user_id);
CREATE INDEX IF NOT EXISTS idx_reservation_salle ON Reservation(salle_id);
CREATE INDEX IF NOT EXISTS idx_reservation_creneau ON Reservation(creneau_id);
CREATE INDEX IF NOT EXISTS idx_reservation_statut ON Reservation(statut);
CREATE INDEX IF NOT EXISTS idx_equipement_salle ON Equipement(salle_id);
CREATE INDEX IF NOT EXISTS idx_historique_reservation ON Historique(reservation_id);
CREATE INDEX IF NOT EXISTS idx_creneau_debut ON Creneau(debut);
CREATE INDEX IF NOT EXISTS idx_creneau_fin ON Creneau(fin);

-- ============================================
-- DONNÉES INITIALES
-- ============================================

-- Utilisateur par défaut
INSERT OR IGNORE INTO "User" (nom, email, role)
VALUES ('Jean Dupont', 'jean.dupont@ecole.fr', 'User');

-- Salles
INSERT OR IGNORE INTO Salle (nom, capacite, type, etage, disponibilite)
VALUES 
    ('Amphithéâtre A101', 150, 'Amphithéâtre', 1, 1),
    ('Salle de Cours B201', 35, 'Salle de cours', 2, 1),
    ('Salle de Cours B202', 35, 'Salle de cours', 2, 1),
    ('Laboratoire C301', 25, 'Laboratoire', 3, 1),
    ('Laboratoire C302', 25, 'Laboratoire', 3, 1),
    ('Salle de Séminaire D102', 50, 'Salle de séminaire', 1, 1),
    ('Salle de Réunion E103', 15, 'Salle de réunion', 1, 1),
    ('Salle de Travail Collaboratif F104', 20, 'Salle de travail collaboratif', 1, 1);

-- Créneaux horaires prédéfinis (journée standard)
INSERT OR IGNORE INTO Creneau (debut, fin)
VALUES
    ('2026-02-12 08:00:00', '2026-02-12 10:00:00'),
    ('2026-02-12 10:00:00', '2026-02-12 12:00:00'),
    ('2026-02-12 12:00:00', '2026-02-12 14:00:00'),
    ('2026-02-12 14:00:00', '2026-02-12 16:00:00'),
    ('2026-02-12 16:00:00', '2026-02-12 18:00:00'),
    ('2026-02-13 08:00:00', '2026-02-13 10:00:00'),
    ('2026-02-13 10:00:00', '2026-02-13 12:00:00'),
    ('2026-02-13 12:00:00', '2026-02-13 14:00:00'),
    ('2026-02-13 14:00:00', '2026-02-13 16:00:00'),
    ('2026-02-13 16:00:00', '2026-02-13 18:00:00'),
    ('2026-02-14 08:00:00', '2026-02-14 10:00:00'),
    ('2026-02-14 10:00:00', '2026-02-14 12:00:00'),
    ('2026-02-14 12:00:00', '2026-02-14 14:00:00'),
    ('2026-02-14 14:00:00', '2026-02-14 16:00:00'),
    ('2026-02-14 16:00:00', '2026-02-14 18:00:00');

-- Équipements
INSERT OR IGNORE INTO Equipement (nom, description, type, estFonctionnel, salle_id)
VALUES
    ('Vidéoprojecteur HD', 'Projecteur haute définition', 'Vidéoprojecteur', 1, 1),
    ('Tableau blanc interactif', 'Écran tactile 65 pouces', 'Tableau interactif', 1, 1),
    ('Caméra HD', 'Logitech 4K pour visioconférence', 'Visioconférence', 1, 2),
    ('Micro sans fil', 'Système de microphone sans fil', 'Audio', 1, 2),
    ('Écran tactile 55p', 'Écran tactile interactif', 'Affichage', 1, 3),
    ('Ordinateurs (10)', '10 postes de travail', 'Informatique', 1, 4),
    ('Microscopes (8)', '8 microscopes optiques', 'Équipement scientifique', 1, 4),
    ('Microscopes (8)', '8 microscopes optiques', 'Équipement scientifique', 1, 5),
    ('Vidéoprojecteur HD', 'Projecteur haute définition', 'Vidéoprojecteur', 1, 6),
    ('Tableau blanc', 'Tableau blanc traditionnel', 'Tableau', 1, 7),
    ('Caméra HD', 'Caméra pour enregistrement', 'Enregistrement', 1, 8);

-- Quelques réservations de démonstration
INSERT OR IGNORE INTO Reservation (dateReservation, motif, statut, user_id, salle_id, creneau_id)
VALUES
    (CURRENT_TIMESTAMP, 'Cours de Programmation Avancée', 'Confirmée', 1, 1, 1),
    (CURRENT_TIMESTAMP, 'TP Chimie Organique', 'Confirmée', 1, 4, 2),
    (CURRENT_TIMESTAMP, 'Réunion d''équipe pédagogique', 'Confirmée', 1, 7, 3);

-- Historique des réservations
INSERT OR IGNORE INTO Historique (action, dateAction, reservation_id)
VALUES
    ('Réservation créée', CURRENT_TIMESTAMP, 1),
    ('Réservation confirmée', CURRENT_TIMESTAMP, 1),
    ('Réservation créée', CURRENT_TIMESTAMP, 2),
    ('Réservation confirmée', CURRENT_TIMESTAMP, 2),
    ('Réservation créée', CURRENT_TIMESTAMP, 3),
    ('Réservation confirmée', CURRENT_TIMESTAMP, 3);
