using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Data;
using StageTrackerAPI.DTOs;
using StageTrackerAPI.Models;

namespace StageTrackerAPI.Controllers
{
    // Petit tableau de bord de la recherche de stage.
    [ApiController]
    [Route("api/[controller]")]
    public class StatistiquesController : ControllerBase
    {
        // Statuts qui veulent dire que l'entreprise a repondu, en bien ou en mal.
        private static readonly Statut[] StatutsAvecReponse =
            { Statut.Entrevue, Statut.OffreRecue, Statut.Refus };

        private readonly AppDbContext _db;

        public StatistiquesController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<StatistiquesDto>> Obtenir()
        {
            var maintenantUtc = DateTime.UtcNow;

            var candidatures = await _db.Candidatures.ToListAsync();

            var parStatut = Enum.GetValues<Statut>()
                .ToDictionary(s => s.ToString(), s => candidatures.Count(c => c.Statut == s));

            // Le taux de reponse se calcule seulement sur ce qui a ete envoye.
            var envoyees = candidatures.Where(c => c.DateEnvoi != null).ToList();
            var avecReponse = envoyees.Count(c => StatutsAvecReponse.Contains(c.Statut));
            var taux = envoyees.Count == 0
                ? 0
                : Math.Round(100.0 * avecReponse / envoyees.Count, 1);

            return Ok(new StatistiquesDto(
                await _db.Entreprises.CountAsync(),
                await _db.Offres.CountAsync(),
                candidatures.Count,
                parStatut,
                envoyees.Count,
                taux,
                candidatures.Count(c => c.EstARelancer(maintenantUtc)),
                await _db.Entrevues.CountAsync(e => e.Date >= DateTime.Now)));
        }
    }
}
