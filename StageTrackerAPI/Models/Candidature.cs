namespace StageTrackerAPI.Models
{
    public class Candidature
    {
        public int Id { get; set; }
        public int OffreId { get; set; }
        public Offre? Offre { get; set; }

        public Statut Statut { get; set; } = Statut.Reperee;

        public DateTime DateCreation { get; set; } = DateTime.UtcNow;
        public DateTime? DateEnvoi { get; set; }
        public DateTime? DateDerniereRelance { get; set; }
        public string? Notes { get; set; }

        public List<EvenementCandidature> Evenements { get; set; } = new();
    }
}