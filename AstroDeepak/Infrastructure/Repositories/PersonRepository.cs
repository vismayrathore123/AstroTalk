using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class PersonRepository:IPersonRepository
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

        public async Task<int> SaveAsync(Person person)
        {
            var db = await _context.GetConnectionAsync();
            var entity = ToEntity(person);
            return entity.Id != 0 ? await db.UpdateAsync(entity) : await db.InsertAsync(entity);
        }

        public async Task<int> DeleteAsync(Person person)
        {
            var db = await _context.GetConnectionAsync();
            return await db.DeleteAsync(ToEntity(person));
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
            PhoneNo = e.PhoneNo,
            Address = e.Address,
            GrahanType = e.GrahanType switch
            {
                "Chandra Grahan" => GrahanType.ChandraGrahan,
                "Surya Grahan" => GrahanType.SuryaGrahan,
                _ => GrahanType.None
            },
            SelectedNavgrah = e.SelectedNavgrah

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
            PhoneNo = p.PhoneNo,
            Address = p.Address,
            GrahanType = p.GrahanType.ToString(),
            SelectedNavgrah = p.SelectedNavgrah

        };
    }
}