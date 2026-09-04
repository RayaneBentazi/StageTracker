namespace StageTrackerAPI.DTOs
{
    public record ContactCreationDto(
        int EntrepriseId,
        string Nom,
        string? Titre,
        string? Courriel,
        string? Telephone);

    public record ContactLectureDto(
        int Id,
        int EntrepriseId,
        string NomEntreprise,
        string Nom,
        string? Titre,
        string? Courriel,
        string? Telephone);
}
