using Microsoft.EntityFrameworkCore;
using StudioManager.DataLayer.Models;
using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.UserModels;

namespace StudioManager.DataLayer.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<GearCategory> GearCategories { get; set; } = null!;
        public DbSet<GearItem> GearItems { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

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

            SeedGearCategories(modelBuilder);
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
