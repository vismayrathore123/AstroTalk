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

        public async Task<List<RemedyMaster>> GetRemediesByNavgrahIdAsync(int navgrahId)
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<RemedyMasterEntity>()
                                .Where(r => r.NavgrahId == navgrahId)
                                .ToListAsync();
            return rows.OrderBy(r => r.SortOrder)
                       .Select(r => new RemedyMaster
                       {
                           Id = r.Id,
                           Name = r.Name,
                           SortOrder = r.SortOrder,
                           NavgrahId = r.NavgrahId,
                           CreatedAt = r.CreatedAt,
                           UpdatedAt = r.UpdatedAt
                       })
                       .ToList();
        }

        public async Task AddRemedyMasterAsync(string name, int navgrahId)
        {
            var db = await _context.GetConnectionAsync();

            var existing = await db.Table<RemedyMasterEntity>()
                                    .Where(r => r.NavgrahId == navgrahId)
                                    .ToListAsync();
            if (existing.Any(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)))
                return;

            var nextOrder = existing.Count == 0 ? 1 : existing.Max(r => r.SortOrder) + 1;
            var now = DateTime.Now;

            await db.InsertAsync(new RemedyMasterEntity
            {
                Name = name,
                SortOrder = nextOrder,
                NavgrahId = navgrahId,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        public async Task UpdateRemedyMasterAsync(int id, string name)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<RemedyMasterEntity>().Where(r => r.Id == id).FirstOrDefaultAsync();
            if (entity == null) return;

            entity.Name = name;
            entity.UpdatedAt = DateTime.Now;
            await db.UpdateAsync(entity);
        }

        public async Task DeleteRemedyMasterAsync(int id)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<RemedyMasterEntity>()
                                  .Where(r => r.Id == id)
                                  .FirstOrDefaultAsync();
            if (entity != null)
                await db.DeleteAsync(entity);
        }
    }
}