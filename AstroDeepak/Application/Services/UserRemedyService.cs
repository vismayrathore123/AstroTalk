using System.Text.Json;
using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Application.Services
{
    public class UserRemedyService : IUserRemedyService
    {
        private readonly IUsersRemedyRepository _repository;

        public UserRemedyService(IUsersRemedyRepository repository)
        {
            _repository = repository;
        }

        public async Task<UserRemedyDto?> GetAsync(int personId, int navgrahId)
        {
            var entity = await _repository.GetByPersonAndNavgrahAsync(personId, navgrahId);
            return entity == null ? null : ToDto(entity);
        }

        public async Task SaveSelectedRemediesAsync(int personId, int navgrahId, List<string> selectedRemedyNames, bool sentOnWhatsApp)
        {
            var existing = await _repository.GetByPersonAndNavgrahAsync(personId, navgrahId);

            var history = string.IsNullOrWhiteSpace(existing?.RemediesJson)
                ? new List<RemedyHistoryEntry>()
                : (JsonSerializer.Deserialize<List<RemedyHistoryEntry>>(existing.RemediesJson) ?? new List<RemedyHistoryEntry>());

            history.Add(new RemedyHistoryEntry
            {
                CreatedAt = DateTime.Now,
                Remedies = selectedRemedyNames
            });

            var entity = existing ?? new UserRemedy { PersonId = personId, NavgrahId = navgrahId };
            entity.CurrentSuggestedRemedy = string.Join(", ", selectedRemedyNames);
            entity.RemediesJson = JsonSerializer.Serialize(history);
            entity.WhatsApp = sentOnWhatsApp;

            await _repository.SaveAsync(entity);
        }

        private static UserRemedyDto ToDto(UserRemedy r) => new()
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