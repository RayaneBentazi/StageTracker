# StageTracker

Application pour suivre mes candidatures de stage : entreprises, offres, statuts, historique daté, relances et entrevues. Une API REST en ASP.NET Core et une interface en React.

Projet personnel développé pendant ma dernière session du DEC en Techniques de l'informatique au Cégep Édouard-Montpetit.

## Le problème

Chercher un stage veut dire suivre des dizaines d'offres réparties sur plusieurs sites, avec des dates limites différentes et des relances à faire au bon moment. Un fichier Excel finit par mentir : on écrase la case « statut » et on perd la trace de la date d'envoi.

StageTracker garde l'historique complet de chaque candidature. Chaque changement de statut crée une ligne datée au lieu d'écraser la précédente.

## Fonctionnalités

- Gestion des entreprises, des contacts et des offres de stage
- Suivi d'une candidature à travers six statuts, de « repérée » à « refus »
- Historique daté de chaque transition, avec commentaire
- Détection des offres en double par empreinte SHA-256
- **Relances** : quand une candidature passe à « envoyée », une relance est planifiée 7 jours plus tard. La liste des relances en retard est disponible, et chaque relance faite repousse la suivante d'une semaine
- **Entrevues** : date, type (téléphonique, vidéo, en personne, technique), durée et interlocuteurs. Ajouter une entrevue fait passer la candidature à « entrevue » et l'inscrit dans l'historique
- **Dates limites** : les offres qui ferment bientôt et pour lesquelles je n'ai pas encore postulé
- **Statistiques** : candidatures par statut, taux de réponse, relances en retard, entrevues à venir
- **Interface React** : tableau de bord, liste filtrable des candidatures, fiche détaillée avec historique et entrevues, gestion des offres et des entreprises
- Tests automatisés (xUnit pour l'API, Vitest pour l'interface) lancés à chaque push par GitHub Actions

## Stack

| Couche | Technologie |
|---|---|
| Plateforme | .NET 10 |
| API | ASP.NET Core Web API |
| Accès aux données | Entity Framework Core |
| Base de données | SQL Server (LocalDB) |
| Documentation | Swagger (Swashbuckle) |
| Interface | React 19 + TypeScript, Vite, React Router |
| Tests | xUnit, WebApplicationFactory, SQLite en mémoire, Vitest, Testing Library |
| Intégration continue | GitHub Actions |

## Modèle de données

Six entités :

- **Entreprise** — celle qui publie l'offre, avec ses contacts
- **Contact** — une personne-ressource chez l'entreprise
- **Offre** — l'offre de stage, avec son empreinte d'unicité
- **Candidature** — ma candidature pour une offre, avec son statut courant et ses dates de relance
- **EvenementCandidature** — une ligne d'historique par changement de statut
- **Entrevue** — une entrevue liée à une candidature

Une offre mène à au plus une candidature. Une candidature accumule autant d'événements que de transitions, et peut avoir plusieurs entrevues.

## Décisions techniques

**Le statut est stocké à deux endroits.** La candidature porte son statut courant, et la table d'événements garde toutes les transitions. C'est une dénormalisation volontaire : afficher une liste de candidatures sans ce champ obligerait à charger tous les événements de chacune pour trouver le dernier. Le champ est le cache, la table est la vérité.

**La logique d'un changement de statut est dans l'entité.** `Candidature.ChangerStatut()` met à jour le statut, remplit la date d'envoi, planifie la relance et ajoute l'événement. Le contrôleur des candidatures et l'ajout d'une entrevue appellent la même méthode, et elle se teste sans base de données.

**L'empreinte est calculée à la création.** Un hash SHA-256 de (titre, entreprise, URL), normalisé en minuscules, avec un index unique en base. La même offre entrée deux fois, même avec une casse différente, est refusée avec un 409.

**Les entités ne sortent jamais du contrôleur.** Toutes les réponses passent par des DTO. Sérialiser une `Offre` qui contient une `Entreprise` qui contient une liste d'`Offre` provoquerait un cycle infini.

**La relation un-à-un est déclarée explicitement.** Entity Framework ne peut pas deviner de quel côté placer la clé étrangère dans une relation un-à-un, d'où le `HasForeignKey<Candidature>` dans `OnModelCreating`.

**Les interlocuteurs d'une entrevue sont du texte libre.** Un lien vers `Contact` aurait créé deux chemins de suppression en cascade vers la table des entrevues (par l'offre et par le contact), ce que SQL Server refuse.

**Les tests d'intégration n'ont pas besoin de SQL Server.** Dans l'environnement `Testing`, l'API n'enregistre pas SQL Server et les tests la démarrent avec une base SQLite en mémoire, neuve pour chaque test.

**L'interface passe par un proxy Vite.** En développement, les appels `/api` de React sont redirigés vers l'API, donc pas besoin de configurer CORS.

## Endpoints

| Méthode | Route | Description |
|---|---|---|
| GET | `/api/entreprises` | Liste des entreprises |
| POST | `/api/entreprises` | Créer une entreprise |
| GET | `/api/offres` | Liste des offres, filtrable par statut |
| GET | `/api/offres/echeances?jours=7` | Offres dont la date limite approche, sans candidature envoyée |
| POST | `/api/offres` | Créer une offre, retourne 409 si doublon |
| GET | `/api/candidatures/{id}` | Une candidature avec son historique et ses entrevues |
| POST | `/api/candidatures` | Créer une candidature, retourne 409 si l'offre en a déjà une |
| POST | `/api/candidatures/{id}/statut` | Changer le statut et enregistrer l'événement |
| GET | `/api/candidatures/a-relancer` | Candidatures envoyées dont la relance est due |
| POST | `/api/candidatures/{id}/relance` | Noter une relance et planifier la suivante |
| GET / POST | `/api/candidatures/{id}/entrevues` | Entrevues d'une candidature / en ajouter une |
| GET | `/api/entrevues/a-venir` | Prochaines entrevues |
| PUT / DELETE | `/api/entrevues/{id}` | Modifier / supprimer une entrevue |
| GET | `/api/statistiques` | Tableau de bord |
| GET | `/api/contacts` | Liste des contacts, filtrable par entreprise |

La liste complète est disponible dans Swagger.

## Installation

Prérequis : SDK .NET 10, SQL Server LocalDB (installé avec Visual Studio) et Node.js 22.

```bash
git clone https://github.com/RayaneBentazi/StageTracker.git
cd StageTracker/StageTrackerAPI
dotnet restore
dotnet ef database update
dotnet run
```

Swagger est ensuite accessible à `http://localhost:5188/swagger`.

Dans un deuxième terminal, pour l'interface :

```bash
cd StageTracker/stagetracker-web
npm install
npm run dev
```

L'interface est accessible à `http://localhost:5173`.

La chaîne de connexion par défaut pointe vers LocalDB. Pour utiliser une autre instance, modifier `ConnectionStrings:Default` dans `appsettings.json`.

## Tests

```bash
dotnet test StageTrackerAPI/StageTrackerAPI.slnx   # API
cd stagetracker-web && npm test                  # interface
```

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

Un `GET /api/candidatures/1` retourne alors la candidature avec ses deux événements datés et la prochaine relance prévue une semaine plus tard. D'autres exemples sont dans `StageTrackerAPI/StageTracker.http`.

## Suite prévue

- Modifier et supprimer les offres et les candidatures depuis l'interface
- Notifications pour les relances et les dates limites
- Déploiement (Azure App Service pour l'API, Azure Static Web Apps pour l'interface)

## Auteur

Rayane Bentazi — Techniques de l'informatique, Cégep Édouard-Montpetit
