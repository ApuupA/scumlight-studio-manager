using Microsoft.EntityFrameworkCore;
using StudioManager.DataLayer.Models;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using StudioManager.DataLayer.Models.SettingsModels;
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
        public DbSet<SongTempo> SongTempos { get; set; } = null!;
        public DbSet<SongSection> SongSections { get; set; } = null!;
        public DbSet<SongTrack> SongTracks { get; set; } = null!;
        public DbSet<ProjectLink> ProjectLinks { get; set; } = null!;
        public DbSet<MixVersion> MixVersions { get; set; } = null!;
        public DbSet<MixFeedback> MixFeedback { get; set; } = null!;
        public DbSet<ProjectDeliverable> ProjectDeliverables { get; set; } = null!;
        public DbSet<VoiceMemo> VoiceMemos { get; set; } = null!;
        public DbSet<DrumTuning> DrumTunings { get; set; } = null!;
        public DbSet<ExternalGear> ExternalGear { get; set; } = null!;

        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<SessionTask> SessionTasks { get; set; } = null!;

        public DbSet<ProjectInstrument> ProjectInstruments { get; set; } = null!;
        public DbSet<InstrumentSetupItem> InstrumentSetupItems { get; set; } = null!;
        public DbSet<SetupTemplate> SetupTemplates { get; set; } = null!;
        public DbSet<SetupTemplateItem> SetupTemplateItems { get; set; } = null!;

        public DbSet<InputGroup> InputGroups { get; set; } = null!;
        public DbSet<StudioInput> StudioInputs { get; set; } = null!;
        public DbSet<InputList> InputLists { get; set; } = null!;
        public DbSet<InputListRow> InputListRows { get; set; } = null!;

        public DbSet<BusinessSettings> BusinessSettings { get; set; } = null!;
        public DbSet<StandardTask> StandardTasks { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<InvoiceLine> InvoiceLines { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<Quote> Quotes { get; set; } = null!;
        public DbSet<QuoteLine> QuoteLines { get; set; } = null!;
        public DbSet<Expense> Expenses { get; set; } = null!;
        public DbSet<TimeEntry> TimeEntries { get; set; } = null!;
        public DbSet<ProjectCostItem> ProjectCostItems { get; set; } = null!;

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

                // Existing gear gets an empty list - without a default the column could not be added to filled tables
                entity.Property(g => g.GoodFor).HasDefaultValueSql("'{}'");

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
                entity.Property(u => u.CalendarToken).HasMaxLength(64);
                entity.HasIndex(u => u.CalendarToken).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(200).IsRequired();
                entity.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                entity.HasIndex(u => u.Email).IsUnique();
            });

            ConfigureClients(modelBuilder);
            ConfigureProjects(modelBuilder);
            ConfigureSessions(modelBuilder);
            ConfigureInstruments(modelBuilder);
            ConfigureInputLists(modelBuilder);
            ConfigureFinance(modelBuilder);

            SeedGearCategories(modelBuilder);
        }

        private static void ConfigureClients(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(c => c.PhotoPath).HasMaxLength(300);
                entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
                entity.Property(c => c.ContactPerson).HasMaxLength(200);
                entity.Property(c => c.Email).HasMaxLength(200);
                entity.Property(c => c.Phone).HasMaxLength(50);
                entity.Property(c => c.Address).HasMaxLength(500);
                entity.Property(c => c.Notes).HasMaxLength(4000);
            });

            modelBuilder.Entity<Band>(entity =>
            {
                entity.Property(b => b.PhotoPath).HasMaxLength(300);
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
                entity.Property(p => p.ExtraRevisionPrice).HasPrecision(10, 2);
                entity.Property(p => p.MaterialLink).HasMaxLength(2000);
                entity.Property(p => p.MaterialNotes).HasMaxLength(2000);
                entity.Property(p => p.PhotoPath).HasMaxLength(300);
                entity.Property(p => p.BandGearToken).HasMaxLength(64);
                entity.HasIndex(p => p.BandGearToken).IsUnique();
                entity.Property(p => p.Title).HasMaxLength(200).IsRequired();
                entity.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.Description).HasMaxLength(4000);
                entity.Property(p => p.Daw).HasMaxLength(200);
                entity.Property(p => p.TargetLufs).HasPrecision(4, 1);
                entity.Property(p => p.PricingMode).HasConversion<string>().HasMaxLength(20);
                entity.Property(p => p.HourlyRate).HasPrecision(10, 2);
                entity.Property(p => p.FlatPrice).HasPrecision(12, 2);
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
                entity.Property(s => s.MaterialNotes).HasMaxLength(2000);
                entity.Property(s => s.MixNotes).HasMaxLength(8000);
                entity.Property(s => s.Isrc).HasMaxLength(12);
                entity.Property(s => s.MeasuredLufs).HasPrecision(5, 2);
                entity.Property(s => s.TruePeak).HasPrecision(5, 2);
                entity.Property(s => s.GapBeforeSeconds).HasPrecision(5, 2);
                entity.Property(s => s.Title).HasMaxLength(200).IsRequired();
                entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(s => s.Tuning).HasMaxLength(50);
                entity.Property(s => s.Notes).HasMaxLength(4000);
                entity.Property(s => s.Lyrics).HasMaxLength(20000);

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

            modelBuilder.Entity<SongSection>(entity =>
            {
                entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
                entity.Property(s => s.Notes).HasMaxLength(1000);

                entity.HasOne(s => s.Song)
                      .WithMany(s => s.Sections)
                      .HasForeignKey(s => s.SongId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SongTrack>(entity =>
            {
                entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
                entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(t => t.Notes).HasMaxLength(500);

                entity.HasOne(t => t.Song)
                      .WithMany(s => s.Tracks)
                      .HasForeignKey(t => t.SongId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SongTempo>(entity =>
            {
                entity.Property(t => t.Bpm).HasPrecision(6, 2);

                entity.HasOne(t => t.Song)
                      .WithMany(s => s.Tempos)
                      .HasForeignKey(t => t.SongId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MixVersion>(entity =>
            {
                entity.Property(v => v.Name).HasMaxLength(30).IsRequired();
                entity.Property(v => v.ApprovedBy).HasMaxLength(100);
                entity.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(v => v.Link).HasMaxLength(2000);
                entity.Property(v => v.Notes).HasMaxLength(4000);
                // Mixes (v1, v2, ...) and demos (Demo 1, Demo 2, ...) are numbered separately
                entity.HasIndex(v => new { v.ProjectId, v.IsDemo, v.Number }).IsUnique();

                entity.HasOne(v => v.Project)
                      .WithMany(p => p.MixVersions)
                      .HasForeignKey(v => v.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                // The exported songs; deleting a song only removes it from the versions
                entity.HasMany(v => v.Songs)
                      .WithMany(s => s.MixVersions)
                      .UsingEntity(join => join.ToTable("MixVersionSongs"));
            });

            modelBuilder.Entity<MixFeedback>(entity =>
            {
                entity.Property(f => f.Timecode).HasMaxLength(10);
                entity.Property(f => f.Author).HasMaxLength(100);
                entity.Property(f => f.Text).HasMaxLength(1000).IsRequired();

                entity.HasOne(f => f.MixVersion)
                      .WithMany(v => v.Feedback)
                      .HasForeignKey(f => f.MixVersionId)
                      .OnDelete(DeleteBehavior.Cascade);

                // A deleted song turns its feedback into general feedback
                entity.HasOne(f => f.Song)
                      .WithMany()
                      .HasForeignKey(f => f.SongId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ProjectDeliverable>(entity =>
            {
                entity.Property(d => d.Name).HasMaxLength(200).IsRequired();
                entity.Property(d => d.Notes).HasMaxLength(1000);

                entity.HasOne(d => d.Project)
                      .WithMany(p => p.Deliverables)
                      .HasForeignKey(d => d.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<VoiceMemo>(entity =>
            {
                entity.Property(m => m.Title).HasMaxLength(200).IsRequired();
                entity.Property(m => m.Notes).HasMaxLength(2000);
                entity.Property(m => m.FilePath).HasMaxLength(500).IsRequired();
                entity.Property(m => m.FileName).HasMaxLength(260).IsRequired();

                entity.HasOne(m => m.Project)
                      .WithMany()
                      .HasForeignKey(m => m.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                // A deleted song or part sends its memos back to the inbox - a memo is never lost with a song
                entity.HasOne(m => m.Song)
                      .WithMany()
                      .HasForeignKey(m => m.SongId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(m => m.SongSection)
                      .WithMany()
                      .HasForeignKey(m => m.SongSectionId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ProjectLink>(entity =>
            {
                entity.Property(l => l.AddedBy).HasMaxLength(100);
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

                entity.HasMany(s => s.Instruments)
                      .WithMany(instrument => instrument.Sessions)
                      .UsingEntity(join => join.ToTable("SessionInstruments"));

                // Deleting an input list keeps the sessions, they just have no input list anymore
                entity.HasOne(s => s.InputList)
                      .WithMany(l => l.Sessions)
                      .HasForeignKey(s => s.InputListId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SessionTask>(entity =>
            {
                entity.Property(t => t.Text).HasMaxLength(500).IsRequired();

                entity.HasOne(t => t.Session)
                      .WithMany(s => s.Tasks)
                      .HasForeignKey(t => t.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureInstruments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProjectInstrument>(entity =>
            {
                entity.Property(i => i.Name).HasMaxLength(100).IsRequired();
                entity.Property(i => i.Notes).HasMaxLength(2000);

                entity.HasOne(i => i.Project)
                      .WithMany(p => p.Instruments)
                      .HasForeignKey(i => i.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Band)
                      .WithMany()
                      .HasForeignKey(i => i.BandId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<InstrumentSetupItem>(entity =>
            {
                entity.Property(s => s.CustomName).HasMaxLength(200);
                entity.Property(s => s.UsedFor).HasMaxLength(100);
                entity.Property(s => s.Notes).HasMaxLength(500);

                entity.HasOne(s => s.ProjectInstrument)
                      .WithMany(i => i.SetupItems)
                      .HasForeignKey(s => s.ProjectInstrumentId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Studio gear used in a setup can not be deleted (GearItemService explains why)
                entity.HasOne(s => s.GearItem)
                      .WithMany()
                      .HasForeignKey(s => s.GearItemId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Band gear removed from the project disappears from the setups as well
                entity.HasOne(s => s.ExternalGear)
                      .WithMany()
                      .HasForeignKey(s => s.ExternalGearId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SetupTemplate>(entity =>
            {
                entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
                entity.Property(t => t.Notes).HasMaxLength(2000);
            });

            modelBuilder.Entity<SetupTemplateItem>(entity =>
            {
                entity.Property(s => s.CustomName).HasMaxLength(200);
                entity.Property(s => s.UsedFor).HasMaxLength(100);
                entity.Property(s => s.Notes).HasMaxLength(500);

                entity.HasOne(s => s.SetupTemplate)
                      .WithMany(t => t.Items)
                      .HasForeignKey(s => s.SetupTemplateId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Deleting studio gear removes it from the templates (templates are less important than projects)
                entity.HasOne(s => s.GearItem)
                      .WithMany()
                      .HasForeignKey(s => s.GearItemId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureInputLists(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InputGroup>(entity =>
            {
                entity.Property(g => g.Name).HasMaxLength(100).IsRequired();
                entity.Property(g => g.Color).HasMaxLength(20).IsRequired();
            });

            modelBuilder.Entity<StudioInput>(entity =>
            {
                entity.Property(i => i.SnakeChannel).HasMaxLength(50);
                entity.Property(i => i.Preamp).HasMaxLength(100);
                entity.Property(i => i.DawChannel).HasMaxLength(50);
                entity.Property(i => i.Notes).HasMaxLength(200);

                // Deleting a group keeps its inputs (they just lose their color)
                entity.HasOne(i => i.InputGroup)
                      .WithMany(g => g.Inputs)
                      .HasForeignKey(i => i.InputGroupId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<InputList>(entity =>
            {
                entity.Property(l => l.Name).HasMaxLength(100).IsRequired();
                entity.Property(l => l.Notes).HasMaxLength(2000);

                entity.HasOne(l => l.Project)
                      .WithMany(p => p.InputLists)
                      .HasForeignKey(l => l.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InputListRow>(entity =>
            {
                entity.Property(r => r.Source).HasMaxLength(100);
                entity.Property(r => r.Microphone).HasMaxLength(100);
                entity.Property(r => r.Stand).HasMaxLength(100);
                entity.Property(r => r.Insert).HasMaxLength(100);
                entity.Property(r => r.Comment).HasMaxLength(200);

                // One row per studio input and list
                entity.HasIndex(r => new { r.InputListId, r.StudioInputId }).IsUnique();

                entity.HasOne(r => r.InputList)
                      .WithMany(l => l.Rows)
                      .HasForeignKey(r => r.InputListId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Deleting a studio input removes its rows from all input lists
                entity.HasOne(r => r.StudioInput)
                      .WithMany()
                      .HasForeignKey(r => r.StudioInputId)
                      .OnDelete(DeleteBehavior.Cascade);

                // The row stays when the setup entry is deleted - it was only the source of the pre-filled values
                entity.HasOne(r => r.SetupItem)
                      .WithMany()
                      .HasForeignKey(r => r.SetupItemId)
                      .OnDelete(DeleteBehavior.SetNull);
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

        /// <summary>
        /// Time tracking and project costs.
        /// </summary>
        private static void ConfigureFinance(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusinessSettings>(entity =>
            {
                entity.Property(s => s.DefaultHourlyRate).HasPrecision(10, 2);
                entity.Property(s => s.Currency).HasMaxLength(3).IsRequired();
                entity.Property(s => s.CompanyName).HasMaxLength(200);
                entity.Property(s => s.OwnerName).HasMaxLength(200);
                entity.Property(s => s.Street).HasMaxLength(200);
                entity.Property(s => s.PostalCode).HasMaxLength(20);
                entity.Property(s => s.City).HasMaxLength(100);
                entity.Property(s => s.Email).HasMaxLength(200);
                entity.Property(s => s.Phone).HasMaxLength(50);
                entity.Property(s => s.Website).HasMaxLength(200);
                entity.Property(s => s.TaxNumber).HasMaxLength(50);
                entity.Property(s => s.VatId).HasMaxLength(50);
                entity.Property(s => s.BankName).HasMaxLength(100);
                entity.Property(s => s.Iban).HasMaxLength(50);
                entity.Property(s => s.Bic).HasMaxLength(20);
                entity.Property(s => s.VatRate).HasPrecision(5, 2);
                entity.Property(s => s.InvoiceNumberPrefix).HasMaxLength(30).IsRequired();
                entity.Property(s => s.QuoteNumberPrefix).HasMaxLength(30).IsRequired();
            });

            modelBuilder.Entity<StandardTask>(entity =>
            {
                entity.Property(t => t.Name).HasMaxLength(100).IsRequired();
                entity.Property(t => t.Unit).HasConversion<string>().HasMaxLength(10);
                entity.Property(t => t.HoursPerUnit).HasPrecision(5, 2);
            });

            modelBuilder.Entity<TimeEntry>(entity =>
            {
                entity.Property(t => t.Hours).HasPrecision(5, 2);
                entity.Property(t => t.Description).HasMaxLength(500).IsRequired();

                entity.HasOne(t => t.Project)
                      .WithMany()
                      .HasForeignKey(t => t.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                // A deleted session or user keeps the time - only the link is removed
                entity.HasOne(t => t.Session)
                      .WithMany()
                      .HasForeignKey(t => t.SessionId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // A used standard task can not be deleted (StandardTaskService) - only archived
                entity.HasOne(t => t.StandardTask)
                      .WithMany()
                      .HasForeignKey(t => t.StandardTaskId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Deleting a draft releases the time (InvoiceService does it, the database as safety net)
                entity.HasOne(t => t.Invoice)
                      .WithMany()
                      .HasForeignKey(t => t.InvoiceId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ProjectCostItem>(entity =>
            {
                entity.Property(c => c.Description).HasMaxLength(300).IsRequired();
                entity.Property(c => c.Quantity).HasPrecision(10, 2);
                entity.Property(c => c.UnitPrice).HasPrecision(10, 2);

                entity.HasOne(c => c.Project)
                      .WithMany()
                      .HasForeignKey(c => c.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Invoice)
                      .WithMany()
                      .HasForeignKey(c => c.InvoiceId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                // Every number only once - also protects against two invoices issued at the same moment
                entity.Property(i => i.Number).HasMaxLength(50);
                entity.HasIndex(i => i.Number).IsUnique();

                entity.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(i => i.RecipientName).HasMaxLength(200).IsRequired();
                entity.Property(i => i.RecipientAddress).HasMaxLength(500);
                entity.Property(i => i.IntroText).HasMaxLength(2000);
                entity.Property(i => i.ClosingText).HasMaxLength(2000);
                entity.Property(i => i.VatRate).HasPrecision(5, 2);
                entity.Property(i => i.NetTotal).HasPrecision(12, 2);
                entity.Property(i => i.VatAmount).HasPrecision(12, 2);
                entity.Property(i => i.GrossTotal).HasPrecision(12, 2);

                // Invoices must be kept - a project or client with invoices can not be deleted
                entity.HasOne(i => i.Project)
                      .WithMany()
                      .HasForeignKey(i => i.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Client)
                      .WithMany()
                      .HasForeignKey(i => i.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.CancelsInvoice)
                      .WithMany()
                      .HasForeignKey(i => i.CancelsInvoiceId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InvoiceLine>(entity =>
            {
                entity.Property(l => l.Description).HasMaxLength(300).IsRequired();
                entity.Property(l => l.Unit).HasMaxLength(20);
                entity.Property(l => l.Quantity).HasPrecision(10, 2);
                entity.Property(l => l.UnitPrice).HasPrecision(12, 2);
                entity.Property(l => l.Total).HasPrecision(12, 2);

                entity.HasOne(l => l.Invoice)
                      .WithMany(i => i.Lines)
                      .HasForeignKey(l => l.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Quote>(entity =>
            {
                entity.Property(q => q.Number).HasMaxLength(50);
                entity.HasIndex(q => q.Number).IsUnique();

                entity.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);
                entity.Property(q => q.RecipientName).HasMaxLength(200).IsRequired();
                entity.Property(q => q.RecipientAddress).HasMaxLength(500);
                entity.Property(q => q.IntroText).HasMaxLength(2000);
                entity.Property(q => q.ClosingText).HasMaxLength(2000);
                entity.Property(q => q.VatRate).HasPrecision(5, 2);
                entity.Property(q => q.NetTotal).HasPrecision(12, 2);
                entity.Property(q => q.VatAmount).HasPrecision(12, 2);
                entity.Property(q => q.GrossTotal).HasPrecision(12, 2);

                // Quotes are kept with the project like invoices
                entity.HasOne(q => q.Project)
                      .WithMany()
                      .HasForeignKey(q => q.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(q => q.Client)
                      .WithMany()
                      .HasForeignKey(q => q.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(q => q.Invoice)
                      .WithMany()
                      .HasForeignKey(q => q.InvoiceId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<QuoteLine>(entity =>
            {
                entity.Property(l => l.Description).HasMaxLength(300).IsRequired();
                entity.Property(l => l.Unit).HasMaxLength(20);
                entity.Property(l => l.Quantity).HasPrecision(10, 2);
                entity.Property(l => l.UnitPrice).HasPrecision(12, 2);
                entity.Property(l => l.Total).HasPrecision(12, 2);

                entity.HasOne(l => l.Quote)
                      .WithMany(q => q.Lines)
                      .HasForeignKey(l => l.QuoteId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Expense>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(300).IsRequired();
                entity.Property(e => e.Supplier).HasMaxLength(200);
                entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(30);
                entity.Property(e => e.GrossAmount).HasPrecision(12, 2);
                entity.Property(e => e.VatRate).HasPrecision(5, 2);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.ReceiptPath).HasMaxLength(500);
                entity.Property(e => e.ReceiptFileName).HasMaxLength(255);
                entity.HasIndex(e => e.Date);

                // A deleted project keeps its expenses - only the link is removed
                entity.HasOne(e => e.Project)
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount).HasPrecision(12, 2);
                entity.Property(p => p.Note).HasMaxLength(300);

                entity.HasOne(p => p.Invoice)
                      .WithMany(i => i.Payments)
                      .HasForeignKey(p => p.InvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
