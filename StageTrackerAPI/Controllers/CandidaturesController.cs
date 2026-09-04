using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Data;
using StageTrackerAPI.DTOs;
using StageTrackerAPI.Models;

namespace StageTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CandidaturesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CandidaturesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<CandidatureLectureDto>>> Lister()
        {
            var candidatures = await _db.Candidatures
                .Include(c => c.Offre).ThenInclude(o => o!.Entreprise)
                .Include(c => c.Evenements)
                .OrderByDescending(c => c.DateCreation)
                .ToListAsync();

            return Ok(candidatures.Select(Projeter).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CandidatureLectureDto>> Obtenir(int id)
        {
            var c = await _db.Candidatures
                .Include(x => x.Offre).ThenInclude(o => o!.Entreprise)
                .Include(x => x.Evenements)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c is null) return NotFound();

            return Ok(Projeter(c));
        }

        [HttpPost]
        public async Task<ActionResult<CandidatureLectureDto>> Creer(CandidatureCreationDto dto)
        {
            var offre = await _db.Offres
                .Include(o => o.Entreprise)
                .Include(o => o.Candidature)
                .FirstOrDefaultAsync(o => o.Id == dto.OffreId);

            if (offre is null) return BadRequest("Offre introuvable.");
            if (offre.Candidature is not null) return Conflict("Cette offre a deja une candidature.");

            var candidature = new Candidature
            {
                OffreId = dto.OffreId,
                Statut = Statut.Reperee,
                Notes = dto.Notes
            };

            // Le premier evenement est cree en meme temps que la candidature :
            // l'historique ne doit jamais commencer par un trou.
            candidature.Evenements.Add(new EvenementCandidature
            {
                Statut = Statut.Reperee,
                Commentaire = "Candidature creee"
            });

            _db.Candidatures.Add(candidature);
            await _db.SaveChangesAsync();

            candidature.Offre = offre;

            return CreatedAtAction(nameof(Obtenir), new { id = candidature.Id }, Projeter(candidature));
        }

        // C'est l'endpoint central de la phase 1 : changer le statut
        // n'ecrase rien, il ajoute une ligne datee dans l'historique.
        [HttpPost("{id:int}/statut")]
        public async Task<ActionResult<CandidatureLectureDto>> ChangerStatut(int id, ChangementStatutDto dto)
        {
            var c = await _db.Candidatures
                .Include(x => x.Offre).ThenInclude(o => o!.Entreprise)
                .Include(x => x.Evenements)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c is null) return NotFound();
            if (c.Statut == dto.NouveauStatut)
                return BadRequest("La candidature est deja dans cet etat.");

            c.Statut = dto.NouveauStatut;

            // DateEnvoi se remplit une seule fois, au premier passage a Envoyee.
            if (dto.NouveauStatut == Statut.Envoyee && c.DateEnvoi is null)
                c.DateEnvoi = DateTime.UtcNow;

            c.Evenements.Add(new EvenementCandidature
            {
                Statut = dto.NouveauStatut,
                Commentaire = dto.Commentaire
            });

            await _db.SaveChangesAsync();
            return Ok(Projeter(c));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var c = await _db.Candidatures.FindAsync(id);
            if (c is null) return NotFound();

            _db.Candidatures.Remove(c);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Methode privee pour eviter de repeter la meme projection
        // dans quatre endpoints.
        private static CandidatureLectureDto Projeter(Candidature c) => new(
            c.Id,
            c.OffreId,
            c.Offre?.Titre ?? string.Empty,
            c.Offre?.Entreprise?.Nom ?? string.Empty,
            c.Statut,
            c.DateCreation,
            c.DateEnvoi,
            c.Notes,
            c.Evenements
                .OrderBy(e => e.Date)
                .Select(e => new EvenementDto(e.Statut, e.Date, e.Commentaire))
                .ToList());
    }
}
