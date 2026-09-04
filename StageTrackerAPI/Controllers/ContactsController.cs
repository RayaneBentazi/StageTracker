using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Data;
using StageTrackerAPI.DTOs;
using StageTrackerAPI.Models;

namespace StageTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ContactsController(AppDbContext db) => _db = db;

        // Filtre optionnel : api/contacts?entrepriseId=3
        [HttpGet]
        public async Task<ActionResult<List<ContactLectureDto>>> Lister([FromQuery] int? entrepriseId)
        {
            var requete = _db.Contacts.Include(c => c.Entreprise).AsQueryable();

            if (entrepriseId is not null)
                requete = requete.Where(c => c.EntrepriseId == entrepriseId);

            var contacts = await requete
                .OrderBy(c => c.Nom)
                .Select(c => new ContactLectureDto(
                    c.Id, c.EntrepriseId, c.Entreprise!.Nom,
                    c.Nom, c.Titre, c.Courriel, c.Telephone))
                .ToListAsync();

            return Ok(contacts);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ContactLectureDto>> Obtenir(int id)
        {
            var c = await _db.Contacts
                .Include(x => x.Entreprise)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c is null) return NotFound();

            return Ok(new ContactLectureDto(
                c.Id, c.EntrepriseId, c.Entreprise!.Nom,
                c.Nom, c.Titre, c.Courriel, c.Telephone));
        }

        [HttpPost]
        public async Task<ActionResult<ContactLectureDto>> Creer(ContactCreationDto dto)
        {
            var entreprise = await _db.Entreprises.FindAsync(dto.EntrepriseId);
            if (entreprise is null)
                return BadRequest($"Entreprise {dto.EntrepriseId} introuvable.");

            var contact = new Contact
            {
                EntrepriseId = dto.EntrepriseId,
                Nom = dto.Nom.Trim(),
                Titre = dto.Titre,
                Courriel = dto.Courriel,
                Telephone = dto.Telephone
            };

            _db.Contacts.Add(contact);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Obtenir), new { id = contact.Id },
                new ContactLectureDto(contact.Id, contact.EntrepriseId, entreprise.Nom,
                    contact.Nom, contact.Titre, contact.Courriel, contact.Telephone));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Supprimer(int id)
        {
            var contact = await _db.Contacts.FindAsync(id);
            if (contact is null) return NotFound();

            _db.Contacts.Remove(contact);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
