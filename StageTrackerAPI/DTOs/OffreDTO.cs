using StageTrackerAPI.Models;

namespace StageTrackerAPI.DTOs
{
    public record OffreCreationDto(
        int EntrepriseId,
        string Titre,
        string? Url,
        string? Source,
        string? Description,
        string? Ville,
        DateTime? DateAffichage,
        DateTime? DateLimite);

    public record OffreLectureDto(
        int Id,
        string Titre,
        string? Url,
        string? Source,
        string? Ville,
        DateTime? DateLimite,
        DateTime DateDecouverte,
        string NomEntreprise,
        Statut? StatutCandidature);
}
