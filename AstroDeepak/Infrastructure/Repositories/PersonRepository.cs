using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly SqliteDbContext _context;

        public PersonRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<List<Person>> GetAllAsync()
        {
            var db = await _context.GetConnectionAsync();
            var entities = await db.Table<PersonEntity>().ToListAsync();
            return entities.Select(ToDomain).ToList();
        }

        public async Task<Person?> GetByIdAsync(int id)
        {
            var db = await _context.GetConnectionAsync();
            var entity = await db.Table<PersonEntity>().Where(p => p.Id == id).FirstOrDefaultAsync();
            return entity == null ? null : ToDomain(entity);
        }

        // Returns the Id of the inserted/updated row (used by callers that need
        // the real PersonId right after saving, e.g. to attach UsersRemedies).
        public async Task<int> SaveAsync(Person person)
        {
            var db = await _context.GetConnectionAsync();
            var entity = ToEntity(person);
            var now = DateTime.Now;

            if (entity.Id != 0)
            {
                var existing = await db.Table<PersonEntity>().Where(p => p.Id == entity.Id).FirstOrDefaultAsync();
                entity.CreatedAt = existing?.CreatedAt ?? now;
                entity.UpdatedAt = now;
                await db.UpdateAsync(entity);
            }
            else
            {
                entity.CreatedAt = now;
                entity.UpdatedAt = now;
                await db.InsertAsync(entity); // sqlite-net populates entity.Id after insert
            }

            return entity.Id;
        }

        public async Task<int> DeleteAsync(Person person)
        {
            var db = await _context.GetConnectionAsync();
            return await db.DeleteAsync(ToEntity(person));
        }

        public async Task<List<Person>> SearchAsync(string term)
        {
            var db = await _context.GetConnectionAsync();
            var all = await db.Table<PersonEntity>().ToListAsync();
            var filtered = string.IsNullOrWhiteSpace(term)
                ? all
                : all.Where(p => p.Name != null &&
                                  p.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                     .ToList();
            return filtered.OrderByDescending(p => p.CreatedAt).Select(ToDomain).ToList();
        }

        public async Task<List<Person>> GetRecentAsync(int count)
        {
            var db = await _context.GetConnectionAsync();
            var all = await db.Table<PersonEntity>().ToListAsync();
            return all.OrderByDescending(p => p.CreatedAt)
                       .Take(count)
                       .Select(ToDomain)
                       .ToList();
        }

        private static Person ToDomain(PersonEntity e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            FatherName = e.FatherName,
            Gotra = e.Gotra,
            DOB = e.DOB,
            Time = e.Time,
            BirthPlace = e.BirthPlace,
            CountryCode = e.CountryCode ?? string.Empty,
            PhoneNo = e.PhoneNo,
            Address = e.Address,
            Grah = e.Grah ?? "None",
            Grahan = e.Grahan ?? "None",
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        private static PersonEntity ToEntity(Person p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            FatherName = p.FatherName,
            Gotra = p.Gotra,
            DOB = p.DOB,
            Time = p.Time,
            BirthPlace = p.BirthPlace,
            CountryCode = p.CountryCode,
            PhoneNo = p.PhoneNo,
            Address = p.Address,
            Grah = p.Grah,
            Grahan = p.Grahan,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}