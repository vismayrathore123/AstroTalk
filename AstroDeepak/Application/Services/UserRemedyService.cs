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
        private readonly IAppLogger _logger;

        public UserRemedyService(IUsersRemedyRepository repository, IAppLogger logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<UserRemedyDto?> GetAsync(int personId, int navgrahId)
        {
            var entity = await _repository.GetByPersonAndNavgrahAsync(personId, navgrahId);
            return entity == null ? null : ToDto(entity);
        }

        public async Task SaveSelectedRemediesAsync(int personId, int navgrahId, List<string> selectedRemedyNames)
        {
            try
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
                // WhatsApp flag untouched here - set separately via MarkWhatsAppStatusAsync.

                await _repository.SaveAsync(entity);

                _logger.LogInfo(
                    $"Remedies saved for PersonId={personId}, NavgrahId={navgrahId}. " +
                    $"Count={selectedRemedyNames.Count}, Remedies=[{string.Join(", ", selectedRemedyNames)}]");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed saving remedies for PersonId={personId}, NavgrahId={navgrahId}", ex);
                throw;
            }
        }

        public async Task MarkWhatsAppStatusAsync(int personId, int navgrahId, bool sent)
        {
            var existing = await _repository.GetByPersonAndNavgrahAsync(personId, navgrahId);
            if (existing == null)
            {
                _logger.LogWarning($"MarkWhatsAppStatusAsync: no UserRemedy row for PersonId={personId}, NavgrahId={navgrahId}");
                return;
            }

            existing.WhatsApp = sent;
            await _repository.SaveAsync(existing);
            _logger.LogInfo($"WhatsApp status updated for PersonId={personId}, NavgrahId={navgrahId}. Sent={sent}");
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