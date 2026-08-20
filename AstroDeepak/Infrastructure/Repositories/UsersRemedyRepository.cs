using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class UsersRemedyRepository : IUsersRemedyRepository
    {
        private readonly SqliteDbContext _context;

        public UsersRemedyRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<UserRemedy?> GetByPersonAndNavgrahAsync(int personId, int navgrahId)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<UsersRemedyEntity>()
                                  .Where(r => r.PersonId == personId && r.NavgrahId == navgrahId)
                                  .FirstOrDefaultAsync();
            return entity == null ? null : ToDomain(entity);
        }

        public async Task<List<UserRemedy>> GetByPersonIdAsync(int personId)
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<UsersRemedyEntity>()
                                .Where(r => r.PersonId == personId)
                                .ToListAsync();
            return rows.Select(ToDomain).ToList();
        }

        public async Task<int> SaveAsync(UserRemedy remedy)
        {
            var db = await _context.GetConnectionAsync();
            var entity = ToEntity(remedy);
            var now = DateTime.Now;

            if (entity.Id != 0)
            {
                var existing = await db.Table<UsersRemedyEntity>().Where(r => r.Id == entity.Id).FirstOrDefaultAsync();
                entity.CreatedAt = existing?.CreatedAt ?? now;
                entity.UpdatedAt = now;
                await db.UpdateAsync(entity);
            }
            else
            {
                entity.CreatedAt = now;
                entity.UpdatedAt = now;
                await db.InsertAsync(entity);
            }

            return entity.Id;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<UsersRemedyEntity>().Where(r => r.Id == id).FirstOrDefaultAsync();
            return entity == null ? 0 : await db.DeleteAsync(entity);
        }

        private static UserRemedy ToDomain(UsersRemedyEntity e) => new()
        {
            Id = e.Id,
            PersonId = e.PersonId,
            NavgrahId = e.NavgrahId,
            CurrentSuggestedRemedy = e.CurrentSuggestedRemedy ?? string.Empty,
            RemediesJson = string.IsNullOrWhiteSpace(e.RemediesJson) ? "[]" : e.RemediesJson,
            WhatsApp = e.WhatsApp,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        private static UsersRemedyEntity ToEntity(UserRemedy r) => new()
        {
            Id = r.Id,
            PersonId = r.PersonId,
            NavgrahId = r.NavgrahId,
            CurrentSuggestedRemedy = r.CurrentSuggestedRemedy,
            RemediesJson = r.RemediesJson,
            WhatsApp = r.WhatsApp,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}