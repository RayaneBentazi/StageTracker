namespace StageTrackerAPI.DTOs
{

    public record EntrepriseCreationDto(
        string Nom,
        string? SiteWeb,
        string? Ville,
        string? Notes);

    public record EntrepriseLectureDto(
        int Id,
        string Nom,
        string? SiteWeb,
        string? Ville,
        string? Notes,
        int NombreOffres);
}
