# 🌌 Cosmo Drift

Bienvenue dans **Cosmo Drift**, un jeu d’arcade spatial où vous sautez de planète en planète dans une galaxie sans fin... jusqu’à ce que vous tombiez.

---

## 🚀 Fonctionnalités principales

- 🪐 Génération dynamique de planètes avec rotation et chute réalistes
- 💫 Score basé sur le temps de survie
- 📉 Difficulté progressive :
  - Les planètes tombent plus vite
  - Elles sont de plus en plus rares
  - Le fond défile plus rapidement
- 🌌 Fond dynamique géré par le `BackgroundManager`
- 🎮 Contrôles simples : **flèches directionnelles** / **A/D** pour tourner autour des planètes, **Espace** pour sauter
- 💾 Sauvegarde automatique du meilleur score

---

## 🎯 Objectif du jeu

- Survivez le plus longtemps possible en sautant de planète en planète
- Plus vous survivez, plus la difficulté augmente
- Atteignez le meilleur score sans tomber hors de l’écran !

---

## 🛠️ Installation et Lancement

> ⚠️ Jeu disponible en executable

### Étapes :

1. Rendez-vous sur la page des **releases** du projet
2. Cliquez sur la **version `v1.0.0`**
3. Téléchargez le fichier **`CosmoDrift.zip`**
4. Décompressez l’archive dans le dossier de votre choix
5. Lancez le fichier **`CosmoDrift.exe`**

---

## 📂 Contenu du zip

CosmoDrift/ ├── CosmoDrift.exe # Fichier principal du jeu
---

## 🧠 Architecture (vue UML)

Le projet suit une structure modulaire claire. Voici les composants clés :

- **GameManager** : Gère l'état du jeu, le score, la difficulté et l'interface.
- **PlayerController** : Mouvement, saut et atterrissage sur les planètes.
- **PlanetSpawner** : Génére les planètes dynamiquement.
- **Planet** : Comportement individuel des planètes (rotation, chute, destruction).
- **BackgroundManager** : Fait défiler le fond visuel.
- **ScrollingStars** : Décor etoiles secondaire en mouvement.

_Un diagramme UML est disponible dans le projet pour visualiser l'ensemble des relations entre classes._

---

## ✅ Crédits

- Projet créé avec Unity
- Design gameplay & programmation : **[AUbin et Amine]**
- UI et logique conçues pour être facilement modifiables et étendues

---

> ⭐ Amusez-vous bien, et n’oubliez pas de viser toujours plus haut dans **Cosmo Drift** ! 🚀