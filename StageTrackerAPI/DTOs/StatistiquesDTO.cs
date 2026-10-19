namespace StageTrackerAPI.DTOs
{
    public record StatistiquesDto(
        int TotalEntreprises,
        int TotalOffres,
        int TotalCandidatures,
        Dictionary<string, int> ParStatut,
        int CandidaturesEnvoyees,
        double TauxDeReponse,
        int ARelancer,
        int EntrevuesAVenir);
}
