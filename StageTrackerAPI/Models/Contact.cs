namespace StageTrackerAPI.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public int EntrepriseId { get; set; }
        public Entreprise? Entreprise { get; set; }

        public string Nom { get; set; } = string.Empty;
        public string? Titre { get; set; }
        public string? Courriel { get; set; }
        public string? Telephone { get; set; }
    }
}