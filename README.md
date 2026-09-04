# StageTracker

API REST en ASP.NET Core pour suivre mes candidatures de stage : offres, statuts et historique daté.

Projet personnel développé pendant ma dernière session du DEC en Techniques de l'informatique au Cégep Édouard-Montpetit.

## Le problème

Chercher un stage veut dire suivre des dizaines d'offres réparties sur plusieurs sites, avec des dates limites différentes et des relances à faire au bon moment. Un fichier Excel finit par mentir : on écrase la case « statut » et on perd la trace de la date d'envoi.

StageTracker garde l'historique complet de chaque candidature. Chaque changement de statut crée une ligne datée au lieu d'écraser la précédente.

## Fonctionnalités

- Gestion des entreprises, des contacts et des offres de stage
- Suivi d'une candidature à travers six statuts, de « repérée » à « refus »
- Historique daté de chaque transition, avec commentaire
- Détection des offres en double par empreinte SHA-256
- Documentation interactive des endpoints avec Swagger

## Stack

| Couche | Technologie |
|---|---|
| Plateforme | .NET 10 |
| API | ASP.NET Core Web API |
| Accès aux données | Entity Framework Core |
| Base de données | SQL Server (LocalDB) |
| Documentation | Swagger (Swashbuckle) |

## Modèle de données

Cinq entités :

- **Entreprise** — celle qui publie l'offre, avec ses contacts
- **Contact** — une personne-ressource chez l'entreprise
- **Offre** — l'offre de stage, avec son empreinte d'unicité
- **Candidature** — ma candidature pour une offre, avec son statut courant
- **EvenementCandidature** — une ligne d'historique par changement de statut

Une offre mène à au plus une candidature. Une candidature accumule autant d'événements que de transitions.

## Décisions techniques

**Le statut est stocké à deux endroits.** La candidature porte son statut courant, et la table d'événements garde toutes les transitions. C'est une dénormalisation volontaire : afficher une liste de candidatures sans ce champ obligerait à charger tous les événements de chacune pour trouver le dernier. Le champ est le cache, la table est la vérité.

**L'empreinte est calculée à la création.** Un hash SHA-256 de (titre, entreprise, URL), normalisé en minuscules, avec un index unique en base. C'est ce qui empêchera le veilleur Python de créer des doublons quand il retrouvera la même offre le lendemain.

**Les entités ne sortent jamais du contrôleur.** Toutes les réponses passent par des DTO. Sérialiser une `Offre` qui contient une `Entreprise` qui contient une liste d'`Offre` provoquerait un cycle infini.

**La relation un-à-un est déclarée explicitement.** Entity Framework ne peut pas deviner de quel côté placer la clé étrangère dans une relation un-à-un, d'où le `HasForeignKey<Candidature>` dans `OnModelCreating`.

## Endpoints

| Méthode | Route | Description |
|---|---|---|
| GET | `/api/entreprises` | Liste des entreprises |
| POST | `/api/entreprises` | Créer une entreprise |
| GET | `/api/offres` | Liste des offres, filtrable par statut |
| POST | `/api/offres` | Créer une offre, retourne 409 si doublon |
| GET | `/api/candidatures/{id}` | Une candidature avec son historique complet |
| POST | `/api/candidatures` | Créer une candidature, retourne 409 si l'offre en a déjà une |
| POST | `/api/candidatures/{id}/statut` | Changer le statut et enregistrer l'événement |
| GET | `/api/contacts` | Liste des contacts, filtrable par entreprise |

La liste complète est disponible dans Swagger.

## Installation

```bash
git clone https://github.com/RayaneBentazi/stagetracker-api.git
cd StageTrackerAPI
dotnet restore
dotnet ef database update
dotnet run
```

Swagger est ensuite accessible à `http://localhost:5188/swagger`.

La chaîne de connexion par défaut pointe vers LocalDB. Pour utiliser une autre instance, modifier `ConnectionStrings:Default` dans `appsettings.json`.

## Exemple d'utilisation

Créer une entreprise, une offre, puis suivre la candidature :

```http
POST /api/entreprises
{ "nom": "Genetec", "ville": "Brossard" }

POST /api/offres
{ "entrepriseId": 1, "titre": "Stagiaire développement logiciel", "source": "Indeed" }

POST /api/candidatures
{ "offreId": 1 }

POST /api/candidatures/1/statut
{ "nouveauStatut": 1, "commentaire": "CV envoyé par courriel" }
```

Un `GET /api/candidatures/1` retourne alors la candidature avec ses deux événements datés.

## Suite prévue

- Veilleur Python qui collecte les offres sur plusieurs sites et les envoie à l'endpoint d'ingestion
- Tâche de fond pour signaler les dates limites approchantes et les relances en attente
- Interface React pour gérer les candidatures sans passer par Swagger

## Auteur

Rayane Bentazi — Techniques de l'informatique, Cégep Édouard-Montpetit
