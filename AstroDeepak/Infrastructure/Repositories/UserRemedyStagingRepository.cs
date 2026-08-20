using System.Text.Json;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class UserRemedyStagingRepository : IUserRemedyStagingRepository
    {
        private readonly SqliteDbContext _context;

        public UserRemedyStagingRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveAsync(UserRemedyStaging staging)
        {
            var db = await _context.GetConnectionAsync();
            var entity = ToEntity(staging);
            entity.CreatedAt = DateTime.Now;

            if (entity.Id != 0)
                await db.UpdateAsync(entity);
            else
                await db.InsertAsync(entity);

            return entity.Id;
        }

        public async Task<UserRemedyStaging?> GetByIdAsync(int id)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<UserRemedyStagingEntity>().Where(s => s.Id == id).FirstOrDefaultAsync();
            return entity == null ? null : ToDomain(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<UserRemedyStagingEntity>().Where(s => s.Id == id).FirstOrDefaultAsync();
            if (entity != null)
                await db.DeleteAsync(entity);
        }

        private static UserRemedyStaging ToDomain(UserRemedyStagingEntity e) => new()
        {
            Id = e.Id,
            PersonId = e.PersonId,
            Name = e.Name,
            FatherName = e.FatherName,
            Gotra = e.Gotra,
            DOB = e.DOB,
            Time = e.Time,
            BirthPlace = e.BirthPlace,
            PhoneNo = e.PhoneNo,
            Address = e.Address,
            Grahan = e.Grahan,
            NavgrahId = e.NavgrahId,
            NavgrahName = e.NavgrahName,
            SelectedRemedies = string.IsNullOrWhiteSpace(e.SelectedRemediesJson)
                ? new List<string>()
                : (JsonSerializer.Deserialize<List<string>>(e.SelectedRemediesJson) ?? new List<string>()),
            CreatedAt = e.CreatedAt
        };

        private static UserRemedyStagingEntity ToEntity(UserRemedyStaging s) => new()
        {
            Id = s.Id,
            PersonId = s.PersonId,
            Name = s.Name,
            FatherName = s.FatherName,
            Gotra = s.Gotra,
            DOB = s.DOB,
            Time = s.Time,
            BirthPlace = s.BirthPlace,
            PhoneNo = s.PhoneNo,
            Address = s.Address,
            Grahan = s.Grahan,
            NavgrahId = s.NavgrahId,
            NavgrahName = s.NavgrahName,
            SelectedRemediesJson = JsonSerializer.Serialize(s.SelectedRemedies),
            CreatedAt = s.CreatedAt
        };
    }
}