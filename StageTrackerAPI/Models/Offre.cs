namespace StageTrackerAPI.Models
{
    public class Offre
    {
        public int Id { get; set; }
        public int EntrepriseId { get; set; }
        public Entreprise? Entreprise { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string? Url {  get; set; }
        public string? Source { get; set; }
        public string? Description { get; set; }
        public string? Ville { get; set; }
        public string Empreinte { get; set; } = string.Empty;
        public DateTime? DateAffichage { get; set; }
        public DateTime? DateLimite { get; set; }
        public DateTime DateDecouverte { get; set; } = DateTime.UtcNow;
        public Candidature? Candidature { get; set; }
    }
}