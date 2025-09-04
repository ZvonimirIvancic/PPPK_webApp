using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PrviProjekt.Models;

namespace PrviProjekt.Data
{
    public class MedicinskiDbContext : DbContext
    {
        public MedicinskiDbContext(DbContextOptions<MedicinskiDbContext> options) : base(options)
        {
        }

        public DbSet<Pacijent> Pacijenti { get; set; }
        public DbSet<MedicinskaDokumentacija> MedicinskaDokumentacija { get; set; }
        public DbSet<Pregled> Pregledi { get; set; }
        public DbSet<Slika> Slike { get; set; }
        public DbSet<Recept> Recepti { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Pacijent>(entity =>
            {
                entity.ToTable("pacijenti");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.OIB).HasColumnName("oib").HasMaxLength(11).IsRequired();
                entity.Property(e => e.Ime).HasColumnName("ime").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Prezime).HasColumnName("prezime").HasMaxLength(100).IsRequired();
                entity.Property(e => e.DatumRodenja).HasColumnName("datum_rodenja");
                entity.Property(e => e.Spol).HasColumnName("spol").HasMaxLength(1);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasIndex(e => e.OIB).IsUnique();
                entity.HasIndex(e => e.Prezime);
            });

            modelBuilder.Entity<MedicinskaDokumentacija>(entity =>
            {
                entity.ToTable("medicinska_dokumentacija");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PacijentId).HasColumnName("pacijent_id");
                entity.Property(e => e.NazivBolesti).HasColumnName("naziv_bolesti").HasMaxLength(200).IsRequired();
                entity.Property(e => e.DatumPocetka).HasColumnName("datum_pocetka");
                entity.Property(e => e.DatumZavrsetka).HasColumnName("datum_zavrsetka");
                entity.Property(e => e.Opis).HasColumnName("opis");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.Pacijent)
                    .WithMany(p => p.MedicinskaDokumentacija)
                    .HasForeignKey(e => e.PacijentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Pregled>(entity =>
            {
                entity.ToTable("pregledi");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PacijentId).HasColumnName("pacijent_id");
                entity.Property(e => e.TipPregleda).HasColumnName("tip_pregleda").HasMaxLength(10).IsRequired();
                entity.Property(e => e.DatumPregleda).HasColumnName("datum_pregleda");
                entity.Property(e => e.VrijemePregleda).HasColumnName("vrijeme_pregleda");
                entity.Property(e => e.Opis).HasColumnName("opis");
                entity.Property(e => e.Nalaz).HasColumnName("nalaz");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.Pacijent)
                    .WithMany(p => p.Pregledi)
                    .HasForeignKey(e => e.PacijentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Slika>(entity =>
            {
                entity.ToTable("slike");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PregledId).HasColumnName("pregled_id");
                entity.Property(e => e.NazivDatoteke).HasColumnName("naziv_datoteke").HasMaxLength(255).IsRequired();
                entity.Property(e => e.Putanja).HasColumnName("putanja").HasMaxLength(500).IsRequired();
                entity.Property(e => e.TipDatoteke).HasColumnName("tip_datoteke").HasMaxLength(50);
                entity.Property(e => e.VelicinaDatoteke).HasColumnName("velicina_datoteke");
                entity.Property(e => e.DatumUpload).HasColumnName("datum_upload");
                entity.Property(e => e.Opis).HasColumnName("opis");

                entity.HasOne(e => e.Pregled)
                    .WithMany(p => p.Slike)
                    .HasForeignKey(e => e.PregledId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Recept>(entity =>
            {
                entity.ToTable("recepti");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PacijentId).HasColumnName("pacijent_id");
                entity.Property(e => e.PregledId).HasColumnName("pregled_id");
                entity.Property(e => e.NazivLijeka).HasColumnName("naziv_lijeka").HasMaxLength(200).IsRequired();
                entity.Property(e => e.Doza).HasColumnName("doza").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Upute).HasColumnName("upute");
                entity.Property(e => e.DatumIzdavanja).HasColumnName("datum_izdavanja");
                entity.Property(e => e.DatumVazenja).HasColumnName("datum_vazenja");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.Pacijent)
                    .WithMany(p => p.Recepti)
                    .HasForeignKey(e => e.PacijentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Pregled)
                    .WithMany(p => p.Recepti)
                    .HasForeignKey(e => e.PregledId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
