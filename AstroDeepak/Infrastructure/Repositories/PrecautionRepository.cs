using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class PrecautionRepository : IPrecautionRepository
    {
        private readonly SqliteDbContext _context;

        public PrecautionRepository(SqliteDbContext context) => _context = context;

        public async Task<List<PrecautionMaster>> GetAllAsync()
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<PrecautionMasterEntity>().ToListAsync();
            return rows.OrderBy(r => r.SortOrder)
                       .Select(r => new PrecautionMaster
                       {
                           Id = r.Id,
                           Text = r.Text,
                           SortOrder = r.SortOrder,
                           CreatedAt = r.CreatedAt,
                           UpdatedAt = r.UpdatedAt
                       })
                       .ToList();
        }

        public async Task AddPrecautionAsync(string text)
        {
            var db = await _context.GetConnectionAsync();

            var existing = await db.Table<PrecautionMasterEntity>().ToListAsync();
            if (existing.Any(r => string.Equals(r.Text, text, StringComparison.OrdinalIgnoreCase)))
                return;

            var nextOrder = existing.Count == 0 ? 1 : existing.Max(r => r.SortOrder) + 1;
            var now = DateTime.Now;

            await db.InsertAsync(new PrecautionMasterEntity
            {
                Text = text,
                SortOrder = nextOrder,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        public async Task UpdatePrecautionAsync(int id, string text)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<PrecautionMasterEntity>().Where(r => r.Id == id).FirstOrDefaultAsync();
            if (entity == null) return;

            entity.Text = text;
            entity.UpdatedAt = DateTime.Now;
            await db.UpdateAsync(entity);
        }

        public async Task DeletePrecautionAsync(int id)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<PrecautionMasterEntity>().Where(r => r.Id == id).FirstOrDefaultAsync();
            if (entity != null)
                await db.DeleteAsync(entity);
        }
    }
}