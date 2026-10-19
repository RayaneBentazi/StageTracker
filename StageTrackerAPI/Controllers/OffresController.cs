using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Data;
using StageTrackerAPI.DTOs;
using StageTrackerAPI.Models;
using System.Security.Cryptography;
using System.Text;

namespace StageTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffresController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OffresController(AppDbContext db) => _db = db;

        // Filtre optionnel par statut de candidature : api/offres?statut=Envoyee
        [HttpGet]
        public async Task<ActionResult<List<OffreLectureDto>>> Lister([FromQuery] Statut? statut)
        {
            var requete = _db.Offres
                .Include(o => o.Entreprise)
                .Include(o => o.Candidature)
                .AsQueryable();

            if (statut is not null)
                requete = requete.Where(o => o.Candidature != null && o.Candidature.Statut == statut);

            var offres = await requete
                .OrderByDescending(o => o.DateDecouverte)
                .Select(o => new OffreLectureDto(
                    o.Id, o.Titre, o.Url, o.Source, o.Ville,
                    o.DateLimite, o.DateDecouverte,
                    o.Entreprise!.Nom,
                    o.Candidature != null ? o.Candidature.Statut : null))
                .ToListAsync();

            return Ok(offres);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OffreLectureDto>> Obtenir(int id)
        {
            var o = await _db.Offres
                .Include(x => x.Entreprise)
                .Include(x => x.Candidature)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (o is null) return NotFound();

            return Ok(new OffreLectureDto(
                o.Id, o.Titre, o.Url, o.Source, o.Ville,
                o.DateLimite, o.DateDecouverte,
                o.Entreprise!.Nom,
                o.Candidature?.Statut));
        }

        // Offres dont la date limite arrive dans les prochains jours et ou je
        // n'ai pas encore envoye ma candidature : api/offres/echeances?jours=7
        [HttpGet("echeances")]
        public async Task<ActionResult<List<OffreLectureDto>>> Echeances([FromQuery] int jours = 7)
        {
            if (jours < 1 || jours > 365)
                return BadRequest("Le nombre de jours doit etre entre 1 et 365.");

            var debut = DateTime.Today;
            var fin = debut.AddDays(jours + 1);   // inclut toute la derniere journee

            var offres = await _db.Offres
                .Include(o => o.Entreprise)
                .Include(o => o.Candidature)
                .Where(o => o.DateLimite != null
                         && o.DateLimite >= debut
                         && o.DateLimite < fin
                         && (o.Candidature == null || o.Candidature.Statut == Statut.Reperee))
                .OrderBy(o => o.DateLimite)
                .Select(o => new OffreLectureDto(
                    o.Id, o.Titre, o.Url, o.Source, o.Ville,
                    o.DateLimite, o.DateDecouverte,
                    o.Entreprise!.Nom,
                    o.Candidature != null ? o.Candidature.Statut : null))
                .ToListAsync();

            return Ok(offres);
        }

        [HttpPost]
        public async Task<ActionResult<OffreLectureDto>> Creer(OffreCreationDto dto)
        {
            var entreprise = await _db.Entreprises.FindAsync(dto.EntrepriseId);
            if (entreprise is null)
                return BadRequest($"Entreprise {dto.EntrepriseId} introuvable.");

            var empreinte = CalculerEmpreinte(dto.Titre, entreprise.Nom, dto.Url);

            // Verification explicite pour renvoyer un 409 lisible plutot que
            // de laisser SQL Server lever une exception sur l'index unique.
            if (await _db.Offres.AnyAsync(o => o.Empreinte == empreinte))
                return Conflict("Cette offre existe deja.");

            var offre = new Offre
            {
                EntrepriseId = dto.EntrepriseId,
                Titre = dto.Titre.Trim(),
                Url = dto.Url,
                Source = dto.Source,
                Description = dto.Description,
                Ville = dto.Ville,
                DateAffichage = dto.DateAffichage,
                DateLimite = dto.DateLimite,
                Empreinte = empreinte
            };

            _db.Offres.Add(offre);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Obtenir), new { id = offre.Id },
                new OffreLectureDto(offre.Id, offre.Titre, offre.Url, offre.Source,
                    offre.Ville, offre.DateLimite, offre.DateDecouverte,
                    entreprise.Nom, null));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var offre = await _db.Offres.FindAsync(id);
            if (offre is null) return NotFound();

            _db.Offres.Remove(offre);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Normalisation (minuscules, espaces retires) pour que la meme offre
        // entree deux fois avec une casse differente donne la meme empreinte.
        public static string CalculerEmpreinte(string titre, string entreprise, string? url)
        {
            var brut = $"{titre.Trim().ToLowerInvariant()}|" +
                       $"{entreprise.Trim().ToLowerInvariant()}|" +
                       $"{url?.Trim().ToLowerInvariant()}";

            var octets = SHA256.HashData(Encoding.UTF8.GetBytes(brut));
            return Convert.ToHexString(octets).ToLowerInvariant();
        }
    }
}
