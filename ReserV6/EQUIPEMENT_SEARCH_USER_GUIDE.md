# 🚀 GUIDE UTILISATION : Recherche par Équipements

## ✨ Nouvelle Fonctionnalité

Vous pouvez maintenant **rechercher les réservations par équipements** directement depuis la page Reservations.

---

## 📍 Localisation

**Page** : Reservations  
**Zone** : Barre de filtrage (sous le titre)

---

## 🔍 Comment Utiliser

### 1. Recherche Simple

1. Allez à la page **Reservations**
2. Dans le champ **"Rechercher par équipement"**, tapez un équipement
   - Exemple : `Vidéoprojecteur`
3. Les réservations s'actualisent automatiquement
4. Seules les salles **possédant cet équipement** s'affichent

### 2. Filtrage Combiné

Vous pouvez combiner le **filtre statut** + la **recherche équipement** :

1. Sélectionnez un statut dans la première liste
   - Exemple : `EnCours`
2. Tapez un équipement dans le champ
   - Exemple : `Tableau interactif`
3. Résultat : Réservations **actuelles** dans les salles avec **tableau interactif**

### 3. Recherche par Type

Les recherches acceptent :
- **Nom exact** : `Vidéoprojecteur Sony`
- **Type** : `Vidéoprojecteur`
- **Partiellement** : `Video` (trouve "Vidéoprojecteur")

---

## 💡 Exemples

### Exemple 1 : Trouver toutes les salles avec vidéoprojecteur

```
Statut : Tous
Équipement : Vidéoprojecteur
```
✅ Résultat : Toutes les réservations de salles équipées de vidéoprojecteur

### Exemple 2 : Réservations actuelles en salle avec système audio

```
Statut : EnCours
Équipement : Système audio
```
✅ Résultat : Réservations actuelles dans les salles avec système audio

### Exemple 3 : Réservations confirmées de salles "équipées"

```
Statut : Confirmee
Équipement : Ordinateur
```
✅ Résultat : Réservations confirmées des salles avec ordinateur

---

## ⚙️ Détails Techniques

### Critères de Recherche

La recherche s'effectue sur :
- **Nom d'équipement**
- **Type d'équipement**
- **Description d'équipement**

### Sensibilité

La recherche est **insensible à la casse** :
- `vidéoprojecteur` = `Vidéoprojecteur` = `VIDÉOPROJECTEUR`

### Performance

- ✅ Recherche en temps réel (au fur et à mesure de la saisie)
- ✅ Pas de rechargement de page
- ✅ Filtrage instantané

---

## 🎨 Interface

```
┌─────────────────────────────────────────────────────────────┐
│ Reservations                                                 │
│ Consultez et gerez vos reservations                          │
├─────────────────────────────────────────────────────────────┤
│ Filtre par statut: [Tous      ▼]   Rechercher par équipement:  │
│                                    [________________        ]  │
│ 💡 Tip: Vous pouvez rechercher par type d'équipement        │
├─────────────────────────────────────────────────────────────┤
│ Salle | Utilisateur | Motif | Début | Fin | Statut | Actions│
├─────────────────────────────────────────────────────────────┤
│ (Liste filtrée des réservations)                             │
└─────────────────────────────────────────────────────────────┘
```

---

## ❌ Ce qui n'existe Plus

- ~~Page Users~~ (supprimée)
- ~~Menu Users~~ (supprimé de la navigation)

### Navigation Actuelle

```
Menu
├─ Reservations (+ recherche équipement)
├─ Rooms
└─ Gestion Salles
```

---

## 🆘 Dépannage

### La recherche n'affiche rien
- Vérifiez l'orthographe de l'équipement
- Vérifiez que la salle possède effectivement cet équipement
- Essayez de taper "Tableau" au lieu de "Tableau interactif"

### Le champ est vide mais les réservations ne changent pas
- C'est normal, c'est équivalent à "Tous les équipements"
- Tapez un équipement pour filtrer

### Je veux chercher par salle, pas par équipement
- Allez à la page **Rooms** ou **Gestion Salles**
- Utilisez la barre de recherche des salles

---

## 📊 Avantages

✅ **Recherche ciblée** : Trouvez rapidement les salles avec équipements spécifiques  
✅ **Filtrage flexible** : Combinez plusieurs critères  
✅ **Interface intuitive** : Résultats en temps réel  
✅ **Gain de temps** : Plus besoin de consulter chaque réservation  

---

**Version** : 1.0  
**Date** : 2024  
**Statut** : ✅ Production Ready

