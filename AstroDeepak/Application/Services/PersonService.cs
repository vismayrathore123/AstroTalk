using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _repository;

        public PersonService(IPersonRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PersonDto>> GetAllAsync()
        {
            var people = await _repository.GetAllAsync();
            return people.Select(ToDto).ToList();
        }

        public async Task<PersonDto?> GetByIdAsync(int id)
        {
            var person = await _repository.GetByIdAsync(id);
            return person == null ? null : ToDto(person);
        }

        public async Task<int> SaveAsync(PersonDto dto)
        {
            var entity = ToEntity(dto);
            return await _repository.SaveAsync(entity);
        }

        public async Task<int> DeleteAsync(int id)
        {
            var person = await _repository.GetByIdAsync(id);
            if (person == null) return 0;
            return await _repository.DeleteAsync(person);
        }

        public async Task<List<PersonDto>> SearchAsync(string term)
        {
            var people = await _repository.SearchAsync(term ?? string.Empty);
            return people.Select(ToDto).ToList();
        }

        public async Task<List<PersonDto>> GetRecentAsync(int count = 10)
        {
            var people = await _repository.GetRecentAsync(count);
            return people.Select(ToDto).ToList();
        }

        private static PersonDto ToDto(Person p) => new()
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
            SelectedGrah = p.SelectedGrah,
            SelectedGrahan = p.SelectedGrahan,
            SelectedRemedies = p.SelectedRemedies,
            CreatedAt = p.CreatedAt
        };

        private static Person ToEntity(PersonDto d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            FatherName = d.FatherName,
            Gotra = d.Gotra,
            DOB = d.DOB ?? DateTime.MinValue,
            Time = d.Time,
            BirthPlace = d.BirthPlace,
            PhoneNo = d.PhoneNo,
            Address = d.Address,
            SelectedGrah = d.SelectedGrah,
            SelectedGrahan = d.SelectedGrahan,
            SelectedRemedies = d.SelectedRemedies,
            CreatedAt = d.CreatedAt == default ? DateTime.Now : d.CreatedAt
        };
    }
}