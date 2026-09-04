using StageTrackerAPI.Models;

namespace StageTrackerAPI.DTOs
{
    public record CandidatureCreationDto(int OffreId, string? Notes);

    public record ChangementStatutDto(Statut NouveauStatut, string? Commentaire);

    public record EvenementDto(Statut Statut, DateTime Date, string? Commentaire);

    public record CandidatureLectureDto(
        int Id,
        int OffreId,
        string TitreOffre,
        string NomEntreprise,
        Statut Statut,
        DateTime DateCreation,
        DateTime? DateEnvoi,
        string? Notes,
        List<EvenementDto> Evenements);
}
