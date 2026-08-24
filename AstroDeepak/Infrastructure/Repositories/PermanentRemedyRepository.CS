using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class PermanentRemedyRepository : IPermanentRemedyRepository
    {
        private readonly SqliteDbContext _context;

        public PermanentRemedyRepository(SqliteDbContext context) => _context = context;

        public async Task<List<PermanentRemedy>> GetByPersonIdAsync(int personId)
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<PermanentRemedyEntity>().Where(r => r.PersonId == personId).ToListAsync();
            return rows.Select(ToDomain).ToList();
        }

        public async Task<List<PermanentRemedy>> GetByPersonAndNavgrahAsync(int personId, int navgrahId)
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<PermanentRemedyEntity>()
                                .Where(r => r.PersonId == personId && r.NavgrahId == navgrahId)
                                .ToListAsync();
            return rows.Select(ToDomain).ToList();
        }

        public async Task AddAsync(int personId, int navgrahId, string remedyName)
        {
            var db = await _context.GetConnectionAsync();

            var existing = await db.Table<PermanentRemedyEntity>()
                                    .Where(r => r.PersonId == personId && r.NavgrahId == navgrahId)
                                    .ToListAsync();
            if (existing.Any(r => string.Equals(r.RemedyName, remedyName, StringComparison.OrdinalIgnoreCase)))
                return; // already permanent

            await db.InsertAsync(new PermanentRemedyEntity
            {
                PersonId = personId,
                NavgrahId = navgrahId,
                RemedyName = remedyName,
                CreatedAt = DateTime.Now
            });
        }

        public async Task RemoveAsync(int personId, int navgrahId, string remedyName)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<PermanentRemedyEntity>()
                                  .Where(r => r.PersonId == personId
                                           && r.NavgrahId == navgrahId
                                           && r.RemedyName == remedyName)
                                  .FirstOrDefaultAsync();
            if (entity != null)
                await db.DeleteAsync(entity);
        }

        private static PermanentRemedy ToDomain(PermanentRemedyEntity e) => new()
        {
            Id = e.Id,
            NavgrahId = e.NavgrahId,
            PersonId = e.PersonId,
            RemedyName = e.RemedyName,
            CreatedAt = e.CreatedAt
        };
    }
}