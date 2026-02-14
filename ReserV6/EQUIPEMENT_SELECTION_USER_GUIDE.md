# 🎓 Guide Pratique - Sélection Multiple d'Équipements

## 📖 Table des Matières
1. [Utilisation Basique](#utilisation-basique)
2. [Cas d'Usage Avancés](#cas-dusage-avancés)
3. [Exemples Concrets](#exemples-concrets)
4. [Dépannage](#dépannage)

---

## 🎯 Utilisation Basique

### Étape 1 : Accéder au Filtre d'Équipement
```
1. Ouvrir l'application ReserV6
2. Naviguer vers la page "Salles"
3. Regarder le panneau de filtres en haut
4. Localiser la zone "Filtrer par equipement"
```

### Étape 2 : Voir les Équipements Disponibles
```
La liste affiche tous les équipements de la base de données:
- Vidéoprojecteur
- Tableau Interactif  
- Système Sonore
- Climatisation
- Connexion Internet
- etc.
```

### Étape 3 : Sélectionner un Équipement
```
1. Cliquer sur la case vide ☐ à côté de l'équipement
2. La case devient cochée ☑
3. Le compteur change: "Sélectionné: 1"
4. Les salles se filtrent automatiquement
```

### Étape 4 : Voir les Résultats
```
Seules les salles qui ont cet équipement s'affichent
dans la grille des salles ci-dessous
```

---

## 🔗 Cas d'Usage Avancés

### Cas 1 : Réservation avec Équipements Spécifiques
```
Scénario: "Je dois réserver une salle avec vidéoprojecteur"

Étapes:
1. Sélectionner "Vidéoprojecteur"
2. Résultat: Affiche uniquement les salles avec vidéoprojecteur ✅
3. Parmi ces salles, choisir la meilleure option
4. Cliquer "Réserver"
```

### Cas 2 : Réunion Pluridisciplinaire
```
Scénario: "Nous avons besoin d'une salle avec
           Vidéoprojecteur ET Tableau Interactif"

Étapes:
1. Sélectionner "Vidéoprojecteur"       ☑
2. Sélectionner "Tableau Interactif"    ☑
3. Résultat: Affiche UNIQUEMENT les salles 
   ayant les DEUX équipements
4. Compteur: "Sélectionné: 2"
5. Réserver la salle idéale
```

### Cas 3 : Recherche Combinée Avancée
```
Scénario: "Salle pour 50 personnes, étage 2, 
          avec Vidéoprojecteur ET Système Sonore"

Étapes:
1. Filtrer par nom: (vide - voir tous)
2. Capacité minimale: 50
3. Étage: 2
4. Sélectionner "Vidéoprojecteur"    ☑
5. Sélectionner "Système Sonore"     ☑
6. Résultat: Seulement les salles répondant 
   à TOUS ces critères (intersection logique)
```

### Cas 4 : Réinitialisation Rapide
```
Scénario: "Je viens de sélectionner 3 équipements
          par erreur, je veux tout effacer"

Étapes:
1. Cliquer le bouton "Réinitialiser"
2. Tous les CheckBox se désélectionnent ☐
3. Compteur: "Sélectionné: 0"
4. Toutes les salles réapparaissent
```

---

## 💡 Exemples Concrets

### Exemple 1 : Cours Magistral
```
╔════════════════════════════════════════════════════════╗
║ SITUATION                                              ║
║ - Professeur avec 100 étudiants                       ║
║ - Besoin de faire un diaporama                        ║
║ - Besoin de son de qualité                            ║
╚════════════════════════════════════════════════════════╝

FILTRES À APPLIQUER:
┌─────────────────────────────────────────────────────┐
│ Recherche par nom:           (vide)                  │
│ Capacité minimale:           100                     │
│ Étage:                       (tous)                  │
│ Équipements:                                         │
│  ☑ Vidéoprojecteur                                  │
│  ☑ Système Sonore                                   │
│  ☐ Tableau Interactif                               │
│  ☐ Climatisation                                    │
│  ☐ Connexion Internet                               │
│ Sélectionné: 2                    [Réinitialiser]  │
└─────────────────────────────────────────────────────┘

RÉSULTAT:
→ Affiche: Amphithéâtre A (250 pers, son intégré)
→ Affiche: Grande Salle B (150 pers, vidéo + son)
→ Affiche: Auditorium C (200 pers, système pro)

❌ Exclus: Salles sans vidéoprojecteur
❌ Exclus: Salles sans système sonore
❌ Exclus: Salles trop petites
```

### Exemple 2 : Atelier Interactif
```
╔════════════════════════════════════════════════════════╗
║ SITUATION                                              ║
║ - Atelier d'informatique pour 30 personnes           ║
║ - Besoin tableau interactif pour interactions        ║
║ - Besoin connexion Internet fiable                   ║
║ - Internet haut débit obligatoire                    ║
╚════════════════════════════════════════════════════════╝

FILTRES À APPLIQUER:
┌─────────────────────────────────────────────────────┐
│ Recherche par nom:           "info"                  │
│ Capacité minimale:           30                      │
│ Étage:                       1                       │
│ Équipements:                                         │
│  ☐ Vidéoprojecteur                                  │
│  ☑ Tableau Interactif                               │
│  ☐ Système Sonore                                   │
│  ☑ Connexion Internet                               │
│  ☐ Climatisation                                    │
│ Sélectionné: 2                    [Réinitialiser]  │
└─────────────────────────────────────────────────────┘

RÉSULTAT:
→ Affiche: Salle Informatique 1 (40 pers, étage 1)
→ Affiche: Labo Interactif 2 (35 pers, étage 1)

❌ Exclus: Salles sans tableau interactif
❌ Exclus: Salles sans connexion internet
❌ Exclus: Salles à d'autres étages
❌ Exclus: Salles trop petites
```

### Exemple 3 : Séance de Travail Collaborative
```
╔════════════════════════════════════════════════════════╗
║ SITUATION                                              ║
║ - Réunion d'équipe de 8 personnes                    ║
║ - Brainstorming avec écran partagé                   ║
║ - Pas d'équipements spécifiques requis               ║
║ - Just une "salle Meeting" tranquille                ║
╚════════════════════════════════════════════════════════╝

FILTRES À APPLIQUER:
┌─────────────────────────────────────────────────────┐
│ Recherche par nom:           "meeting"               │
│ Capacité minimale:           0                       │
│ Étage:                       (tous)                  │
│ Équipements:                                         │
│  ☐ Vidéoprojecteur                                  │
│  ☐ Tableau Interactif                               │
│  ☐ Système Sonore                                   │
│  ☐ Climatisation                                    │
│  ☐ Connexion Internet                               │
│ Sélectionné: 0                    [Réinitialiser]  │
└─────────────────────────────────────────────────────┘

RÉSULTAT:
→ Affiche: Salle Meeting A (8 pers)
→ Affiche: Salle Réunion 1 (10 pers)
→ Affiche: Conference Room (12 pers)

✅ Bonus: Filtre équipement pas appliqué (aucun sélectionné)
✅ Focus sur le nom de la salle
```

---

## 🐛 Dépannage

### Problème 1: Je ne vois pas la liste des équipements
```
❌ PROBLÈME:
La zone "Filtrer par equipement" semble vide

✅ SOLUTION:
1. Attendez 2-3 secondes (chargement en cours)
2. Vérifiez qu'il y a des équipements dans la base
3. Actualisez la page (F5)
4. Contactez l'administrateur si persiste
```

### Problème 2: Je sélectionne un équipement mais rien ne change
```
❌ PROBLÈME:
Aucune salle ne s'affiche après sélection

✅ CAUSE PROBABLE:
Aucune salle n'a cet équipement spécifique

✅ SOLUTION:
1. Cliquer "Réinitialiser"
2. Essayer un autre équipement
3. Vérifier les détails des salles en cliquant "Réserver"
4. Contacter l'administrateur pour ajouter équipements
```

### Problème 3: Le compteur ne change pas après sélection
```
❌ PROBLÈME:
"Sélectionné: 0" même après clic sur CheckBox

✅ SOLUTION:
1. Cliquer directement sur la case ☐, pas sur le texte
2. Vérifier que la case devient ☑
3. Actualiser la page si persiste
```

### Problème 4: Trop de salles affichées
```
❌ PROBLÈME:
J'ai sélectionné plusieurs équipements mais vois
toujours plein de salles

✅ VÉRIFIER:
1. Nombre d'équipements sélectionnés correct ?
   (Compteur affiche le bon nombre)
2. Les salles affichées ont-elles TOUS ces équipements ?
3. Essayer réinitialiser et resélectionner

✅ CONSEIL:
Rappelez-vous: Une salle s'affiche seulement si elle a
TOUS les équipements sélectionnés (ET logique)
```

### Problème 5: Je ne peux pas sélectionner certains équipements
```
❌ PROBLÈME:
Certains CheckBox sont grisés/désactivés

✅ EXPLICATIONS PROBABLES:
1. L'équipement n'existe pas dans la base
2. Aucune salle n'a cet équipement
3. Bug temporaire

✅ SOLUTION:
Contactez l'administrateur pour ajouter l'équipement
à une salle ou pour corriger la base de données
```

---

## 📚 Raccourcis Utiles

```
ACTION                          COMMENT FAIRE
─────────────────────────────────────────────────────
Voir tous les équipements      Scroll dans la liste
Sélectionner un équipement     Cliquer sur la case
Désélectionner un équipement   Recliquer sur la case
Tout effacer rapidement        Cliquer "Réinitialiser"
Vérifier la sélection          Regarder le compteur
Réserver une salle             Cliquer "Réserver"
```

---

## 🎓 Conseils d'Utilisation

✅ **Toujours commencer simple**
   - D'abord chercher par nom
   - Puis ajouter les filtres un par un

✅ **Utiliser le compteur**
   - Affiche le nombre exact d'équipements sélectionnés
   - Utile pour vérifier votre sélection

✅ **Réinitialiser souvent**
   - Si vos résultats ne conviennent pas
   - Pour essayer une nouvelle combinaison

✅ **Combiner les filtres efficacement**
   - Filtrer d'abord par capacité (plus restrictif)
   - Puis ajouter les équipements

✅ **Explorer la salle avant de réserver**
   - Cliquer "Réserver" pour voir les crénaux disponibles
   - Lire la description complète

---

## 🆘 Besoin d'Aide ?

Si vous rencontrez des problèmes:
1. Consultez la section Dépannage ci-dessus
2. Vérifiez qu'il y a des salles dans la base
3. Essayez de réinitialiser les filtres
4. Actualisez la page (F5)
5. Contactez l'support technique si persiste

---

## 📞 Support

**Email**: support@reserv6.local
**Chat**: Application de chat interne
**Documentation**: Voir EQUIPEMENT_SELECTION_FILTER_GUIDE.md
