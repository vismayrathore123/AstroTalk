using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class RemedyRepository : IRemedyRepository
    {
        private readonly SqliteDbContext _context;

        public RemedyRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<List<RemedyMaster>> GetAllRemediesAsync()
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<RemedyMasterEntity>().ToListAsync();
            return rows.OrderBy(r => r.SortOrder)
                       .Select(r => new RemedyMaster { Id = r.Id, Name = r.Name, SortOrder = r.SortOrder })
                       .ToList();
        }

        public async Task<List<string>> GetRemediesForNavgrahAsync(string navgrahName)
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<NavgrahRemedyEntity>()
                                .Where(r => r.NavgrahName == navgrahName)
                                .ToListAsync();
            return rows.Select(r => r.RemedyName).ToList();
        }

        public async Task SaveNavgrahRemediesAsync(string navgrahName, List<string> remedyNames)
        {
            var db = await _context.GetConnectionAsync();

            var existing = await db.Table<NavgrahRemedyEntity>()
                                    .Where(r => r.NavgrahName == navgrahName)
                                    .ToListAsync();
            foreach (var row in existing)
                await db.DeleteAsync(row);

            var toInsert = remedyNames.Select(name => new NavgrahRemedyEntity
            {
                NavgrahName = navgrahName,
                RemedyName = name
            }).ToList();

            if (toInsert.Count > 0)
                await db.InsertAllAsync(toInsert);
        }

        public async Task AddRemedyMasterAsync(string name)
        {
            var db = await _context.GetConnectionAsync();

            var existing = await db.Table<RemedyMasterEntity>().ToListAsync();
            if (existing.Any(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)))
                return;

            var nextOrder = existing.Count == 0 ? 1 : existing.Max(r => r.SortOrder) + 1;

            await db.InsertAsync(new RemedyMasterEntity { Name = name, SortOrder = nextOrder });
        }
    }
}