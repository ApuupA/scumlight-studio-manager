using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using StudioManager.DataLayer.Models;
using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.DataLayer.Models.ProjectModels;
using StudioManager.DataLayer.Models.SessionModels;
using System.Linq.Expressions;

namespace StudioManager.DataLayer.Context
{
    /// <summary>
    /// Which projects a user sees - in ONE place, for every query.
    ///
    /// The owner sees everything. An assistant sees a project if
    ///   - one of their groups is a group of the project, or
    ///   - the project is "visible to everyone", or
    ///   - they created it themselves.
    /// Everything that belongs to a project (songs, sessions, mix versions, input lists, ...) is visible exactly
    /// when its project is. Clients and bands are visible through their projects (or when the assistant created the client).
    ///
    /// This works with EF Core query filters: EF adds the condition to every query of these entities automatically -
    /// lists, single items (e.g. /api/Song/17), includes and counts. A service can not forget it.
    /// Changes are checked in SaveChanges: an assistant can not add or move data into a project they do not see.
    /// </summary>
    public partial class AppDbContext
    {
        /// <summary>Read by the query filters on every query (EF re-reads it for the current request).</summary>
        private bool SeesAllProjects => _access?.SeesAllProjects ?? true;

        private int? AccessUserId => _access?.UserId;

        /// <summary>The rule for a project (see the class comment).</summary>
        private Expression<Func<Project, bool>> ProjectIsVisible =>
            p => SeesAllProjects
                 || p.IsVisibleToAll
                 || p.CreatedByUserId == AccessUserId
                 || p.Groups.Any(g => g.Users.Any(u => u.Id == AccessUserId));

        /// <summary>
        /// The rule for a client: it has a visible project, or the assistant created it (a new client without projects yet).
        /// </summary>
        private Expression<Func<Client, bool>> ClientIsVisible
        {
            get
            {
                Expression<Func<Client, bool>> ownClient = c => SeesAllProjects || c.CreatedByUserId == AccessUserId;
                var client = ownClient.Parameters[0];

                // c.Projects.Any(<ProjectIsVisible>)
                var hasVisibleProject = Expression.Call(
                    typeof(Enumerable), nameof(Enumerable.Any), [typeof(Project)],
                    Expression.Property(client, nameof(Client.Projects)),
                    ProjectIsVisible);

                return Expression.Lambda<Func<Client, bool>>(Expression.OrElse(ownClient.Body, hasVisibleProject), client);
            }
        }

        /// <summary>
        /// Registers the query filters. Every new entity that belongs to a project must be added here -
        /// the test AccessFilterTests.EveryEntity_ShouldHaveAnAccessFilter_OrBeOnTheGlobalList fails otherwise.
        /// </summary>
        private void AddAccessFilters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>().HasQueryFilter(ProjectIsVisible);
            modelBuilder.Entity<Client>().HasQueryFilter(ClientIsVisible);
            FilterBy<Band, Client>(modelBuilder, b => b.Client!, ClientIsVisible);

            // Project data
            FilterByProject<Song>(modelBuilder, s => s.Project!);
            FilterByProject<SongTempo>(modelBuilder, t => t.Song!.Project!);
            FilterByProject<SongSection>(modelBuilder, s => s.Song!.Project!);
            FilterByProject<SongTrack>(modelBuilder, t => t.Song!.Project!);
            FilterByProject<ProjectLink>(modelBuilder, l => l.Project!);
            FilterByProject<MixVersion>(modelBuilder, v => v.Project!);
            FilterByProject<MixFeedback>(modelBuilder, f => f.MixVersion!.Project!);
            FilterByProject<ProjectDeliverable>(modelBuilder, d => d.Project!);
            FilterByProject<VoiceMemo>(modelBuilder, m => m.Project!);
            FilterByProject<DrumTuning>(modelBuilder, d => d.Project!);
            FilterByProject<ExternalGear>(modelBuilder, g => g.Project!);
            FilterByProject<ProjectInstrument>(modelBuilder, i => i.Project!);
            FilterByProject<InstrumentSetupItem>(modelBuilder, i => i.ProjectInstrument!.Project!);
            FilterByProject<InputList>(modelBuilder, l => l.Project!);
            FilterByProject<InputListRow>(modelBuilder, r => r.InputList!.Project!);
            FilterByProject<Session>(modelBuilder, s => s.Project!);
            FilterByProject<SessionTask>(modelBuilder, t => t.Session!.Project!);

            // Finance - only the owner gets there anyway (Authorize), filtered so nothing leaks through an include
            FilterByProject<Invoice>(modelBuilder, i => i.Project!);
            FilterByProject<InvoiceLine>(modelBuilder, l => l.Invoice!.Project!);
            FilterByProject<Payment>(modelBuilder, p => p.Invoice!.Project!);
            FilterByProject<Quote>(modelBuilder, q => q.Project!);
            FilterByProject<QuoteLine>(modelBuilder, l => l.Quote!.Project!);
            FilterByProject<TimeEntry>(modelBuilder, t => t.Project!);
            FilterByProject<ProjectCostItem>(modelBuilder, c => c.Project!);
            FilterByProject<Expense>(modelBuilder, e => e.Project!);
        }

        /// <summary>T is visible when the project reached through the path is visible, e.g. a section through s.Song.Project.</summary>
        private void FilterByProject<T>(ModelBuilder modelBuilder, Expression<Func<T, Project>> pathToProject) where T : class
        {
            FilterBy(modelBuilder, pathToProject, ProjectIsVisible);
        }

        /// <summary>
        /// Builds "x => rule(path(x))": the rule of the parent, applied to the parent reached through the path.
        /// </summary>
        private static void FilterBy<T, TParent>(ModelBuilder modelBuilder, Expression<Func<T, TParent>> pathToParent,
            Expression<Func<TParent, bool>> parentIsVisible) where T : class
        {
            var body = ReplacingExpressionVisitor.Replace(parentIsVisible.Parameters[0], pathToParent.Body, parentIsVisible.Body);
            modelBuilder.Entity<T>().HasQueryFilter(Expression.Lambda<Func<T, bool>>(body, pathToParent.Parameters));
        }

        // ---------- Checking changes ----------

        /// <summary>
        /// The assistant who creates a project or a client is noted - they always see it (also without a group).
        /// </summary>
        private void SetCreatedBy()
        {
            if (AccessUserId == null)
            {
                return;
            }

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                switch (entry.Entity)
                {
                    case Project project:
                        project.CreatedByUserId = AccessUserId;
                        break;
                    case Client client:
                        client.CreatedByUserId = AccessUserId;
                        break;
                }
            }
        }

        /// <summary>
        /// Saves - and for an assistant checks afterwards that every added or changed row is still visible to them
        /// (e.g. a song section added to the song of a foreign project, found by trying ids). If not, nothing is saved:
        /// the transaction is rolled back and UnauthorizedAccessException (403) is thrown.
        /// </summary>
        private async Task<int> SaveCheckedAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken)
        {
            SetTimestamps();
            SetCreatedBy();

            var changed = SeesAllProjects
                ? []
                : ChangeTracker.Entries<BaseEntity>()
                    .Where(e => e.State is EntityState.Added or EntityState.Modified && e.Metadata.GetDeclaredQueryFilters().Count > 0)
                    .Select(e => e.Entity)
                    .ToList();

            if (changed.Count == 0)
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }

            // The in-memory database of the tests has no transactions
            await using var transaction = Database.IsRelational() && Database.CurrentTransaction == null
                ? await Database.BeginTransactionAsync(cancellationToken)
                : null;

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            foreach (var entity in changed)
            {
                if (!await IsVisibleAsync(entity, cancellationToken))
                {
                    // Leaving without Commit: the transaction is rolled back when it is disposed
                    throw new UnauthorizedAccessException($"{entity.GetType().Name} {entity.Id} belongs to a project you do not see.");
                }
            }

            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return result;
        }

        /// <summary>Is the row found by a query with the filters of the current user?</summary>
        private Task<bool> IsVisibleAsync(BaseEntity entity, CancellationToken cancellationToken)
        {
            var method = typeof(AppDbContext).GetMethod(nameof(IsVisibleTypedAsync), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entity.GetType());
            return (Task<bool>)method.Invoke(this, [entity.Id, cancellationToken])!;
        }

        private Task<bool> IsVisibleTypedAsync<T>(int id, CancellationToken cancellationToken) where T : BaseEntity
        {
            return Set<T>().AsNoTracking().AnyAsync(e => e.Id == id, cancellationToken);
        }
    }
}
