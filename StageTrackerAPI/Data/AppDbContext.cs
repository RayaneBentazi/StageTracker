using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Models;

namespace StageTrackerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Entreprise> Entreprises => Set<Entreprise>();
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Offre> Offres => Set<Offre>();
        public DbSet<Candidature> Candidatures => Set<Candidature>();
        public DbSet<EvenementCandidature> Evenements => Set<EvenementCandidature>();
        public DbSet<Entrevue> Entrevues => Set<Entrevue>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.Entity<Offre>(e =>
            {
                e.Property(x => x.Titre).IsRequired().HasMaxLength(300);
                e.Property(x => x.Empreinte).IsRequired().HasMaxLength(64);
                e.HasIndex(x => x.Empreinte).IsUnique();
            });

            b.Entity<Candidature>(e =>
            {
                e.HasOne(x => x.Offre)
                 .WithOne(x => x.Candidature)
                 .HasForeignKey<Candidature>(x => x.OffreId);

                e.HasIndex(x => x.OffreId).IsUnique();
            });

            b.Entity<Entreprise>(e =>
            {
                e.Property(x => x.Nom).IsRequired().HasMaxLength(200);
            });

            b.Entity<Entrevue>(e =>
            {
                e.HasOne(x => x.Candidature)
                 .WithMany(x => x.Entrevues)
                 .HasForeignKey(x => x.CandidatureId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.Property(x => x.Interlocuteurs).HasMaxLength(300);
                e.Property(x => x.Notes).HasMaxLength(2000);
            });
        }
    }
}
