namespace StageTrackerAPI.Models
{
    public class Candidature
    {
        // Nombre de jours a attendre avant de relancer une entreprise
        public const int DelaiRelanceJours = 7;

        public int Id { get; set; }
        public int OffreId { get; set; }
        public Offre? Offre { get; set; }

        public Statut Statut { get; set; } = Statut.Reperee;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime? DateEnvoi { get; set; }
        public DateTime? DateDerniereRelance { get; set; }
        public DateTime? DateProchaineRelance { get; set; }
        public string? Notes { get; set; }

        public List<EvenementCandidature> Evenements { get; set; } = new();
        public List<Entrevue> Entrevues { get; set; } = new();

        // Toute la logique d'un changement de statut est ici pour que le
        // controleur des candidatures et celui des entrevues fassent
        // exactement la meme chose (et pour pouvoir la tester sans base).
        public void ChangerStatut(Statut nouveauStatut, string? commentaire, DateTime maintenant)
        {
            Statut = nouveauStatut;

            // Au premier passage a Envoyee : on note la date d'envoi
            // et on planifie la premiere relance.
            if (nouveauStatut == Statut.Envoyee && DateEnvoi is null)
            {
                DateEnvoi = maintenant;
                DateProchaineRelance ??= maintenant.AddDays(DelaiRelanceJours);
            }

            Evenements.Add(new EvenementCandidature
            {
                Statut = nouveauStatut,
                Date = maintenant,
                Commentaire = commentaire
            });
        }

        // Une relance ne change pas le statut, elle repousse seulement la suivante.
        public void Relancer(DateTime maintenant)
        {
            DateDerniereRelance = maintenant;
            DateProchaineRelance = maintenant.AddDays(DelaiRelanceJours);
        }

        public bool EstARelancer(DateTime maintenant) =>
            Statut == Statut.Envoyee
            && DateProchaineRelance is not null
            && DateProchaineRelance <= maintenant;
    }
}