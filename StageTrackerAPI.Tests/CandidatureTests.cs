using StageTrackerAPI.Models;

namespace StageTrackerAPI.Tests
{
    // Tests unitaires de la logique dans Candidature : pas besoin de base de donnees.
    public class CandidatureTests
    {
        private static readonly DateTime Maintenant = new(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void ChangerStatut_VersEnvoyee_RemplitDateEnvoiEtPlanifieRelance()
        {
            var c = new Candidature();

            c.ChangerStatut(Statut.Envoyee, "CV envoye", Maintenant);

            Assert.Equal(Statut.Envoyee, c.Statut);
            Assert.Equal(Maintenant, c.DateEnvoi);
            Assert.Equal(Maintenant.AddDays(Candidature.DelaiRelanceJours), c.DateProchaineRelance);
        }

        [Fact]
        public void ChangerStatut_AjouteUnEvenementDate()
        {
            var c = new Candidature();

            c.ChangerStatut(Statut.Envoyee, "CV envoye", Maintenant);

            var evenement = Assert.Single(c.Evenements);
            Assert.Equal(Statut.Envoyee, evenement.Statut);
            Assert.Equal(Maintenant, evenement.Date);
            Assert.Equal("CV envoye", evenement.Commentaire);
        }

        [Fact]
        public void ChangerStatut_DeuxiemePassageAEnvoyee_GardeLaPremiereDate()
        {
            var c = new Candidature();
            c.ChangerStatut(Statut.Envoyee, null, Maintenant);
            c.ChangerStatut(Statut.Entrevue, null, Maintenant.AddDays(3));

            c.ChangerStatut(Statut.Envoyee, null, Maintenant.AddDays(10));

            Assert.Equal(Maintenant, c.DateEnvoi);
            Assert.Equal(3, c.Evenements.Count);
        }

        [Fact]
        public void Relancer_RepousseLaProchaineRelance()
        {
            var c = new Candidature();
            c.ChangerStatut(Statut.Envoyee, null, Maintenant);
            var plusTard = Maintenant.AddDays(8);

            c.Relancer(plusTard);

            Assert.Equal(plusTard, c.DateDerniereRelance);
            Assert.Equal(plusTard.AddDays(Candidature.DelaiRelanceJours), c.DateProchaineRelance);
        }

        [Fact]
        public void EstARelancer_SeulementSiEnvoyeeEtDateDepassee()
        {
            var c = new Candidature();
            c.ChangerStatut(Statut.Envoyee, null, Maintenant);

            Assert.False(c.EstARelancer(Maintenant.AddDays(3)));
            Assert.True(c.EstARelancer(Maintenant.AddDays(7)));

            // Une fois qu'on a une reponse, plus besoin de relancer
            c.ChangerStatut(Statut.Refus, null, Maintenant.AddDays(8));
            Assert.False(c.EstARelancer(Maintenant.AddDays(9)));
        }
    }
}
