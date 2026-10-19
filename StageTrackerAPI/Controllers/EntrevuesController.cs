using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Data;
using StageTrackerAPI.DTOs;
using StageTrackerAPI.Models;

namespace StageTrackerAPI.Controllers
{
    // Les entrevues se creent a partir d'une candidature :
    // POST api/candidatures/{id}/entrevues (dans CandidaturesController).
    [ApiController]
    [Route("api/[controller]")]
    public class EntrevuesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public EntrevuesController(AppDbContext db) => _db = db;

        // La date d'une entrevue est l'heure locale ou elle a lieu,
        // donc on compare avec DateTime.Now et pas UtcNow.
        [HttpGet("a-venir")]
        public async Task<ActionResult<List<EntrevueAVenirDto>>> AVenir()
        {
            var maintenant = DateTime.Now;

            var entrevues = await _db.Entrevues
                .Include(e => e.Candidature).ThenInclude(c => c!.Offre).ThenInclude(o => o!.Entreprise)
                .Where(e => e.Date >= maintenant)
                .OrderBy(e => e.Date)
                .ToListAsync();

            return Ok(entrevues.Select(e => new EntrevueAVenirDto(
                e.Id,
                e.CandidatureId,
                e.Candidature?.Offre?.Titre ?? string.Empty,
                e.Candidature?.Offre?.Entreprise?.Nom ?? string.Empty,
                e.Date,
                e.Type,
                e.Interlocuteurs)).ToList());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EntrevueLectureDto>> Obtenir(int id)
        {
            var entrevue = await _db.Entrevues.FindAsync(id);
            if (entrevue is null) return NotFound();

            return Ok(Projeter(entrevue));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modifier(int id, EntrevueCreationDto dto)
        {
            var entrevue = await _db.Entrevues.FindAsync(id);
            if (entrevue is null) return NotFound();
            if (dto.Date == default)
                return BadRequest("La date de l'entrevue est obligatoire.");

            entrevue.Date = dto.Date;
            entrevue.Type = dto.Type;
            entrevue.DureeMinutes = dto.DureeMinutes;
            entrevue.Interlocuteurs = dto.Interlocuteurs;
            entrevue.Notes = dto.Notes;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var entrevue = await _db.Entrevues.FindAsync(id);
            if (entrevue is null) return NotFound();

            _db.Entrevues.Remove(entrevue);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Publique et statique pour etre reutilisee par CandidaturesController.
        public static EntrevueLectureDto Projeter(Entrevue e) => new(
            e.Id, e.CandidatureId, e.Date, e.Type, e.DureeMinutes, e.Interlocuteurs, e.Notes);
    }
}
