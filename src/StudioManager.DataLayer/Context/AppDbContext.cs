using Microsoft.EntityFrameworkCore;
using StudioManager.DataLayer.Models;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.DataLayer.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<GearCategory> GearCategories { get; set; } = null!;
        public DbSet<GearItem> GearItems { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Band> Bands { get; set; } = null!;

        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Song> Songs { get; set; } = null!;
        public DbSet<ProjectLink> ProjectLinks { get; set; } = null!;
        public DbSet<DrumTuning> DrumTunings { get; set; } = null!;
        public DbSet<ExternalGear> ExternalGear { get; set; } = null!;

        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<SessionTask> SessionTasks { get; set; } = null!;
        public DbSet<GearBooking> GearBookings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GearCategory>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.HasIndex(c => c.Name).IsUnique();
            });

            modelBuilder.Entity<GearItem>(entity =>
            {
                entity.Property(g => g.Name).HasMaxLength(200).IsRequired();
                entity.Property(g => g.Manufacturer).HasMaxLength(100);
                entity.Property(g => g.Model).HasMaxLength(100);
                entity.Property(g => g.SerialNumber).HasMaxLength(100);
                entity.Property(g => g.Location).HasMaxLength(100);
                entity.Property(g => g.Notes).HasMaxLength(4000);
                entity.Property(g => g.PhotoPath).HasMaxLength(300);
                entity.Property(g => g.PurchasePrice).HasPrecision(10, 2);

                // Store enums as readable text ("InRepair") instead of numbers
                entity.Property(g => g.Status).HasConversion<string>().HasMaxLength(20);

                // A category that still contains gear can not be deleted (the service also checks this)
                entity.HasOne(g => g.GearCategory)
                      .WithMany(c => c.GearItems)
                      .HasForeignKey(g => g.GearCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
                entity.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            ConfigureClients(modelBuilder);
            ConfigureProjects(modelBuilder);
            ConfigureSessions(modelBuilder);

            SeedGearCategories(modelBuilder);
        }

        private static void ConfigureClients(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
                entity.Property(c => c.ContactPerson).HasMaxLength(200);
                entity.Property(c => c.Email).HasMaxLength(200);
                entity.Property(c => c.Phone).HasMaxLength(50);
                entity.Property(c => c.Address).HasMaxLength(500);
                entity.Property(c => c.Notes).HasMaxLength(4000);
            });

            modelBuilder.Entity<Band>(entity =>
            {
                entity.Property(b => b.Name).HasMaxLength(200).IsRequired();
                entity.Property(b => b.Genre).HasMaxLength(100);
                entity.Property(b => b.Members).HasMaxLength(1000);
                entity.Property(b => b.Email).HasMaxLength(200);
                entity.Property(b => b.Website).HasMaxLength(500);
                entity.Property(b => b.Notes).HasMaxLength(4000);

                // Deleting a client deletes its bands (the service blocks it while projects exist)
                entity.HasOne(b => b.Client)
                      .WithMany(c => c.Bands)
                      .HasForeignKey(b => b.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureProjects(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(entity =>
            {
                entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.Description).HasMaxLength(4000);
                entity.Property(p => p.Daw).HasMaxLength(200);
                entity.Property(p => p.TargetLufs).HasPrecision(4, 1);
                entity.Property(p => p.RecordingNotes).HasMaxLength(8000);
                entity.Property(p => p.MixingNotes).HasMaxLength(8000);
                entity.Property(p => p.MasteringNotes).HasMaxLength(8000);

                entity.HasOne(p => p.Client)
                      .WithMany(c => c.Projects)
                      .HasForeignKey(p => p.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many-to-many: EF creates the join table "ProjectBands" automatically
                entity.HasMany(p => p.Bands)
                      .WithMany(b => b.Projects)
                      .UsingEntity(join => join.ToTable("ProjectBands"));
            });

            modelBuilder.Entity<Song>(entity =>
            {
                entity.Property(s => s.Title).HasMaxLength(200).IsRequired();
                entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(s => s.Bpm).HasPrecision(6, 2);
                entity.Property(s => s.Key).HasMaxLength(50);
                entity.Property(s => s.Tuning).HasMaxLength(50);
                entity.Property(s => s.Notes).HasMaxLength(4000);

                entity.HasOne(s => s.Project)
                      .WithMany(p => p.Songs)
                      .HasForeignKey(s => s.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                // If a band is deleted, its songs stay in the project without band
                entity.HasOne(s => s.Band)
                      .WithMany()
                      .HasForeignKey(s => s.BandId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ProjectLink>(entity =>
            {
                entity.Property(l => l.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(l => l.Title).HasMaxLength(200).IsRequired();
                entity.Property(l => l.Url).HasMaxLength(2000).IsRequired();
                entity.Property(l => l.Notes).HasMaxLength(2000);

                entity.HasOne(l => l.Project)
                      .WithMany(p => p.Links)
                      .HasForeignKey(l => l.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Song)
                      .WithMany()
                      .HasForeignKey(l => l.SongId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DrumTuning>(entity =>
            {
                entity.Property(d => d.Drum).HasMaxLength(100).IsRequired();
                entity.Property(d => d.BatterHz).HasPrecision(7, 2);
                entity.Property(d => d.ResonantHz).HasPrecision(7, 2);
                entity.Property(d => d.Heads).HasMaxLength(200);
                entity.Property(d => d.Notes).HasMaxLength(2000);

                entity.HasOne(d => d.Project)
                      .WithMany(p => p.DrumTunings)
                      .HasForeignKey(d => d.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Song)
                      .WithMany()
                      .HasForeignKey(d => d.SongId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ExternalGear>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Owner).HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(2000);

                entity.HasOne(e => e.Project)
                      .WithMany(p => p.ExternalGear)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureSessions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>(entity =>
            {
                entity.Property(s => s.Title).HasMaxLength(200).IsRequired();
                entity.Property(s => s.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(s => s.Engineer).HasMaxLength(200);
                entity.Property(s => s.Attendees).HasMaxLength(500);
                entity.Property(s => s.Goals).HasMaxLength(4000);
                entity.Property(s => s.Summary).HasMaxLength(4000);
                entity.Property(s => s.NextSteps).HasMaxLength(4000);
                entity.Property(s => s.Notes).HasMaxLength(4000);
                entity.HasIndex(s => s.Date);

                // Deleting a project deletes its sessions (with tasks and gear bookings)
                entity.HasOne(s => s.Project)
                      .WithMany(p => p.Sessions)
                      .HasForeignKey(s => s.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(s => s.Songs)
                      .WithMany(song => song.Sessions)
                      .UsingEntity(join => join.ToTable("SessionSongs"));
            });

            modelBuilder.Entity<SessionTask>(entity =>
            {
                entity.Property(t => t.Text).HasMaxLength(500).IsRequired();

                entity.HasOne(t => t.Session)
                      .WithMany(s => s.Tasks)
                      .HasForeignKey(t => t.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GearBooking>(entity =>
            {
                entity.Property(b => b.Notes).HasMaxLength(500);

                entity.HasOne(b => b.Session)
                      .WithMany(s => s.GearBookings)
                      .HasForeignKey(b => b.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Deleting a gear item removes its bookings
                entity.HasOne(b => b.GearItem)
                      .WithMany()
                      .HasForeignKey(b => b.GearItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        /// <summary>
        /// Default categories that exist after the first migration. They can be renamed or deleted in the app.
        /// </summary>
        private static void SeedGearCategories(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc);
            string[] names =
            [
                "Microphones", "Preamps", "Dynamics", "Equalizers", "Effects", "Interfaces & Converters",
                "Monitoring", "Headphones", "Instruments", "Amplifiers", "Drums & Percussion",
                "DI Boxes", "Cables", "Stands", "Other"
            ];

            modelBuilder.Entity<GearCategory>().HasData(names.Select((name, index) => new GearCategory
            {
                Id = index + 1,
                Name = name,
                SortOrder = (index + 1) * 10,
                DateCreated = seedDate,
                DateUpdated = seedDate
            }));
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            SetTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        /// <summary>
        /// Keeps DateUpdated current on every change and protects DateCreated from being overwritten by updates.
        /// </summary>
        private void SetTimestamps()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.DateCreated = now;
                    entry.Entity.DateUpdated = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.DateUpdated = now;
                    entry.Property(e => e.DateCreated).IsModified = false;
                }
            }
        }
    }
}
