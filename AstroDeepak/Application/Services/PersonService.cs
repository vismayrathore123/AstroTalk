using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _repository;
        private readonly IAppLogger _logger;

        public PersonService(IPersonRepository repository, IAppLogger logger)
        {
            _repository = repository;
            _logger = logger;
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

        // Returns the saved person's Id. This is also used to persist the "New Kundli"
        // form as a real row the moment the user hits Submit, so that navigating back
        // from the Grah-selection screen reloads real data instead of a blank form.
        public async Task<int> SaveAsync(PersonDto dto)
        {
            try
            {
                var entity = ToEntity(dto);
                var id = await _repository.SaveAsync(entity);
                _logger.LogInfo($"Person {(dto.Id == 0 ? "created" : "updated")}. PersonId={id}, Name={dto.Name}");
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save person '{dto.Name}' (Id={dto.Id})", ex);
                throw;
            }
        }

        public async Task<int> DeleteAsync(int id)
        {
            var person = await _repository.GetByIdAsync(id);
            if (person == null)
            {
                _logger.LogWarning($"DeleteAsync called for non-existent PersonId={id}");
                return 0;
            }

            var result = await _repository.DeleteAsync(person);
            _logger.LogInfo($"Person deleted. PersonId={id}");
            return result;
        }

        public async Task<List<PersonDto>> SearchAsync(string term)
        {
            var people = await _repository.SearchAsync(term ?? string.Empty);
            _logger.LogDebug($"Search for '{term}' returned {people.Count} result(s)");
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
            CountryCode = p.CountryCode,
            PhoneNo = p.PhoneNo,
            Address = p.Address,
            Grah = p.Grah,
            Grahan = p.Grahan,
            CreatedAt = p.CreatedAt,
            Precautions = p.Precautions,
            UpdatedAt = p.UpdatedAt
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
            CountryCode = d.CountryCode,
            PhoneNo = d.PhoneNo,
            Address = d.Address,
            Precautions = d.Precautions,
            Grah = d.Grah,
            Grahan = d.Grahan
            // CreatedAt/UpdatedAt are set by the repository based on insert vs update.
        };
    }
}