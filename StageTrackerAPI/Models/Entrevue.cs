namespace StageTrackerAPI.Models
{
    public class Entrevue
    {
        public int Id { get; set; }
        public int CandidatureId { get; set; }
        public Candidature? Candidature { get; set; }

        public DateTime Date { get; set; }
        public TypeEntrevue Type { get; set; }
        public int? DureeMinutes { get; set; }

        // Texte libre plutot qu'un lien vers Contact : une entrevue peut avoir
        // plusieurs personnes, et ca evite deux chemins de suppression en
        // cascade vers la meme table (SQL Server les refuse).
        public string? Interlocuteurs { get; set; }
        public string? Notes { get; set; }
    }
}
