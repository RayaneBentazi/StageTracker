using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Data;
using StageTrackerAPI.DTOs;
using StageTrackerAPI.Models;

namespace StageTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]   // devient api/entreprises
    public class EntreprisesController : ControllerBase
    {
        private readonly AppDbContext _db;

        // Injection de dependance : ASP.NET Core fournit le contexte,
        // le controleur ne le cree jamais lui-meme.
        public EntreprisesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<List<EntrepriseLectureDto>>> Lister()
        {
            var entreprises = await _db.Entreprises
                .OrderBy(e => e.Nom)
                .Select(e => new EntrepriseLectureDto(
                    e.Id, e.Nom, e.SiteWeb, e.Ville, e.Notes, e.Offres.Count))
                .ToListAsync();

            return Ok(entreprises);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EntrepriseLectureDto>> Obtenir(int id)
        {
            var e = await _db.Entreprises
                .Include(x => x.Offres)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e is null) return NotFound();

            return Ok(new EntrepriseLectureDto(
                e.Id, e.Nom, e.SiteWeb, e.Ville, e.Notes, e.Offres.Count));
        }

        [HttpPost]
        public async Task<ActionResult<EntrepriseLectureDto>> Creer(EntrepriseCreationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nom))
                return BadRequest("Le nom est obligatoire.");

            var entreprise = new Entreprise
            {
                Nom = dto.Nom.Trim(),
                SiteWeb = dto.SiteWeb,
                Ville = dto.Ville,
                Notes = dto.Notes
            };

            _db.Entreprises.Add(entreprise);
            await _db.SaveChangesAsync();

            // CreatedAtAction retourne un 201 avec l'en-tete Location
            // qui pointe vers l'endpoint permettant de relire la ressource.
            return CreatedAtAction(nameof(Obtenir), new { id = entreprise.Id },
                new EntrepriseLectureDto(entreprise.Id, entreprise.Nom,
                    entreprise.SiteWeb, entreprise.Ville, entreprise.Notes, 0));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Modifier(int id, EntrepriseCreationDto dto)
        {
            var entreprise = await _db.Entreprises.FindAsync(id);
            if (entreprise is null) return NotFound();

            entreprise.Nom = dto.Nom.Trim();
            entreprise.SiteWeb = dto.SiteWeb;
            entreprise.Ville = dto.Ville;
            entreprise.Notes = dto.Notes;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var entreprise = await _db.Entreprises.FindAsync(id);
            if (entreprise is null) return NotFound();

            // La cascade configuree dans AppDbContext supprime aussi
            // les offres et les contacts rattaches.
            _db.Entreprises.Remove(entreprise);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
