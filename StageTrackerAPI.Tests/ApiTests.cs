using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.DTOs;
using StageTrackerAPI.Models;

namespace StageTrackerAPI.Tests
{
    // Tests d'integration : on appelle les vrais endpoints de l'API.
    public class ApiTests
    {
        private static async Task<int> CreerEntreprise(HttpClient client, string nom = "Genetec")
        {
            var reponse = await client.PostAsJsonAsync("/api/entreprises",
                new EntrepriseCreationDto(nom, null, "Brossard", null));
            var entreprise = await reponse.Content.ReadFromJsonAsync<EntrepriseLectureDto>();
            return entreprise!.Id;
        }

        private static async Task<HttpResponseMessage> PosterOffre(HttpClient client, int entrepriseId,
            string titre = "Stagiaire developpement", string? url = null, DateTime? dateLimite = null)
        {
            return await client.PostAsJsonAsync("/api/offres",
                new OffreCreationDto(entrepriseId, titre, url, "Indeed", null, "Brossard", null, dateLimite));
        }

        private static async Task<int> CreerOffre(HttpClient client, int entrepriseId,
            string titre = "Stagiaire developpement", DateTime? dateLimite = null)
        {
            var reponse = await PosterOffre(client, entrepriseId, titre, dateLimite: dateLimite);
            var offre = await reponse.Content.ReadFromJsonAsync<OffreLectureDto>();
            return offre!.Id;
        }

        private static async Task<CandidatureLectureDto> CreerCandidature(HttpClient client, int offreId)
        {
            var reponse = await client.PostAsJsonAsync("/api/candidatures", new CandidatureCreationDto(offreId, null));
            Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);
            return (await reponse.Content.ReadFromJsonAsync<CandidatureLectureDto>())!;
        }

        private static async Task<HttpResponseMessage> ChangerStatut(HttpClient client, int id, Statut statut) =>
            await client.PostAsJsonAsync($"/api/candidatures/{id}/statut", new ChangementStatutDto(statut, null));

        [Fact]
        public async Task Offre_EnDouble_Retourne409()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var entrepriseId = await CreerEntreprise(client);

            var premiere = await PosterOffre(client, entrepriseId, url: "https://exemple.com/stage");
            // Meme offre avec des majuscules et des espaces : meme empreinte
            var doublon = await PosterOffre(client, entrepriseId, "  STAGIAIRE Developpement ", "https://EXEMPLE.com/stage");

            Assert.Equal(HttpStatusCode.Created, premiere.StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, doublon.StatusCode);
        }

        [Fact]
        public async Task Candidature_Creee_CommenceAvecUnEvenementReperee()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var offreId = await CreerOffre(client, await CreerEntreprise(client));

            var candidature = await CreerCandidature(client, offreId);

            Assert.Equal(Statut.Reperee, candidature.Statut);
            Assert.Equal(Statut.Reperee, Assert.Single(candidature.Evenements).Statut);
        }

        [Fact]
        public async Task Candidature_DeuxFoisPourLaMemeOffre_Retourne409()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var offreId = await CreerOffre(client, await CreerEntreprise(client));
            await CreerCandidature(client, offreId);

            var doublon = await client.PostAsJsonAsync("/api/candidatures", new CandidatureCreationDto(offreId, null));

            Assert.Equal(HttpStatusCode.Conflict, doublon.StatusCode);
        }

        [Fact]
        public async Task ChangerStatut_VersEnvoyee_PlanifieLaRelance()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var offreId = await CreerOffre(client, await CreerEntreprise(client));
            var candidature = await CreerCandidature(client, offreId);

            var reponse = await ChangerStatut(client, candidature.Id, Statut.Envoyee);

            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            var resultat = await reponse.Content.ReadFromJsonAsync<CandidatureLectureDto>();
            Assert.Equal(2, resultat!.Evenements.Count);
            Assert.NotNull(resultat.DateEnvoi);
            Assert.Equal(resultat.DateEnvoi!.Value.AddDays(7), resultat.DateProchaineRelance);
        }

        [Fact]
        public async Task ChangerStatut_VersLeMemeStatut_Retourne400()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var offreId = await CreerOffre(client, await CreerEntreprise(client));
            var candidature = await CreerCandidature(client, offreId);

            var reponse = await ChangerStatut(client, candidature.Id, Statut.Reperee);

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        }

        [Fact]
        public async Task ARelancer_RetourneSeulementLesRelancesEnRetard()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var entrepriseId = await CreerEntreprise(client);
            var enRetard = await CreerCandidature(client, await CreerOffre(client, entrepriseId, "Stage A"));
            var pasEncore = await CreerCandidature(client, await CreerOffre(client, entrepriseId, "Stage B"));
            await ChangerStatut(client, enRetard.Id, Statut.Envoyee);
            await ChangerStatut(client, pasEncore.Id, Statut.Envoyee);

            // On recule la date de relance de la premiere pour simuler 8 jours sans reponse
            await factory.ModifierBase(async db =>
            {
                var c = await db.Candidatures.SingleAsync(x => x.Id == enRetard.Id);
                c.DateProchaineRelance = DateTime.UtcNow.AddDays(-1);
            });

            var aRelancer = await client.GetFromJsonAsync<List<CandidatureLectureDto>>("/api/candidatures/a-relancer");

            Assert.Equal(enRetard.Id, Assert.Single(aRelancer!).Id);
        }

        [Fact]
        public async Task Relancer_UneCandidatureNonEnvoyee_Retourne400()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var offreId = await CreerOffre(client, await CreerEntreprise(client));
            var candidature = await CreerCandidature(client, offreId);

            var reponse = await client.PostAsync($"/api/candidatures/{candidature.Id}/relance", null);

            Assert.Equal(HttpStatusCode.BadRequest, reponse.StatusCode);
        }

        [Fact]
        public async Task Relancer_RepousseLaProchaineRelance()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var offreId = await CreerOffre(client, await CreerEntreprise(client));
            var candidature = await CreerCandidature(client, offreId);
            await ChangerStatut(client, candidature.Id, Statut.Envoyee);

            var reponse = await client.PostAsync($"/api/candidatures/{candidature.Id}/relance", null);

            Assert.Equal(HttpStatusCode.OK, reponse.StatusCode);
            var resultat = await reponse.Content.ReadFromJsonAsync<CandidatureLectureDto>();
            Assert.NotNull(resultat!.DateDerniereRelance);
            Assert.Equal(resultat.DateDerniereRelance!.Value.AddDays(7), resultat.DateProchaineRelance);
        }

        [Fact]
        public async Task AjouterEntrevue_PasseLaCandidatureAEntrevue()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var offreId = await CreerOffre(client, await CreerEntreprise(client));
            var candidature = await CreerCandidature(client, offreId);
            await ChangerStatut(client, candidature.Id, Statut.Envoyee);

            var reponse = await client.PostAsJsonAsync($"/api/candidatures/{candidature.Id}/entrevues",
                new EntrevueCreationDto(DateTime.Now.AddDays(3), TypeEntrevue.Video, 60, "Deux developpeurs", null));

            Assert.Equal(HttpStatusCode.Created, reponse.StatusCode);

            var detail = await client.GetFromJsonAsync<CandidatureLectureDto>($"/api/candidatures/{candidature.Id}");
            Assert.Equal(Statut.Entrevue, detail!.Statut);
            Assert.Equal(3, detail.Evenements.Count);
            Assert.Single(detail.Entrevues);

            var aVenir = await client.GetFromJsonAsync<List<EntrevueAVenirDto>>("/api/entrevues/a-venir");
            Assert.Equal("Genetec", Assert.Single(aVenir!).NomEntreprise);
        }

        [Fact]
        public async Task Echeances_RetourneLesOffresSansCandidatureEnvoyee()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var entrepriseId = await CreerEntreprise(client);
            var bientot = DateTime.Today.AddDays(3);

            await CreerOffre(client, entrepriseId, "Bientot", bientot);
            await CreerOffre(client, entrepriseId, "Trop loin", DateTime.Today.AddDays(30));
            var dejaEnvoyee = await CreerCandidature(client, await CreerOffre(client, entrepriseId, "Deja envoyee", bientot));
            await ChangerStatut(client, dejaEnvoyee.Id, Statut.Envoyee);

            var offres = await client.GetFromJsonAsync<List<OffreLectureDto>>("/api/offres/echeances?jours=7");

            Assert.Equal("Bientot", Assert.Single(offres!).Titre);
        }

        [Fact]
        public async Task Statistiques_CalculeLeTauxDeReponse()
        {
            using var factory = new StageTrackerFactory();
            var client = factory.CreateClient();
            var entrepriseId = await CreerEntreprise(client);
            var a = await CreerCandidature(client, await CreerOffre(client, entrepriseId, "A"));
            var b = await CreerCandidature(client, await CreerOffre(client, entrepriseId, "B"));
            await CreerCandidature(client, await CreerOffre(client, entrepriseId, "C"));

            await ChangerStatut(client, a.Id, Statut.Envoyee);
            await ChangerStatut(client, b.Id, Statut.Envoyee);
            await ChangerStatut(client, a.Id, Statut.Refus);

            var stats = await client.GetFromJsonAsync<StatistiquesDto>("/api/statistiques");

            Assert.Equal(1, stats!.TotalEntreprises);
            Assert.Equal(3, stats.TotalOffres);
            Assert.Equal(3, stats.TotalCandidatures);
            Assert.Equal(2, stats.CandidaturesEnvoyees);
            Assert.Equal(50.0, stats.TauxDeReponse);
            Assert.Equal(1, stats.ParStatut["Refus"]);
            Assert.Equal(1, stats.ParStatut["Reperee"]);
        }
    }
}
