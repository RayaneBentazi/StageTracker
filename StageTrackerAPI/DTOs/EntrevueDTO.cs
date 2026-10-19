using StageTrackerAPI.Models;

namespace StageTrackerAPI.DTOs
{
    public record EntrevueCreationDto(
        DateTime Date,
        TypeEntrevue Type,
        int? DureeMinutes,
        string? Interlocuteurs,
        string? Notes);

    public record EntrevueLectureDto(
        int Id,
        int CandidatureId,
        DateTime Date,
        TypeEntrevue Type,
        int? DureeMinutes,
        string? Interlocuteurs,
        string? Notes);

    // Version pour la liste des prochaines entrevues : on ajoute l'offre
    // et l'entreprise pour savoir de quoi il s'agit sans autre requete.
    public record EntrevueAVenirDto(
        int Id,
        int CandidatureId,
        string TitreOffre,
        string NomEntreprise,
        DateTime Date,
        TypeEntrevue Type,
        string? Interlocuteurs);
}
