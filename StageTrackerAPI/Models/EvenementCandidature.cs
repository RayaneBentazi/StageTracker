namespace StageTrackerAPI.Models
{
    public class EvenementCandidature
    {
        public int Id { get; set; }
        public int CandidatureId { get; set; }
        public Candidature? Candidature { get; set; }

        public Statut Statut { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string? Commentaire { get; set; }
    }
}