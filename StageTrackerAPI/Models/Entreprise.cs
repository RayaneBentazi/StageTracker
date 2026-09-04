namespace StageTrackerAPI.Models
{
    public class Entreprise
    {
        public int Id { get; set; }

        public string Nom { get; set; } = string.Empty;

        public string? SiteWeb { get; set; }
        public string? Ville { get; set; }
        public string? Notes { get; set; }

        public List<Offre> Offres { get; set; } = new();
        public List<Contact> Contacts { get; set; } = new();
    }
}