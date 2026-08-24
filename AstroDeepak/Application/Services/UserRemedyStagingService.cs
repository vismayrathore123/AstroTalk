using AstroDeepak.Application.DTOs;
using AstroDeepak.Application.Interfaces;
using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;

namespace AstroDeepak.Application.Services
{
    public class UserRemedyStagingService : IUserRemedyStagingService
    {
        private readonly IUserRemedyStagingRepository _repository;

        public UserRemedyStagingService(IUserRemedyStagingRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> SaveAsync(UserRemedyStagingDto dto)
        {
            var entity = ToDomain(dto);
            return await _repository.SaveAsync(entity);
        }

        public async Task<UserRemedyStagingDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : ToDto(entity);
        }

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);

        private static UserRemedyStagingDto ToDto(UserRemedyStaging s) => new()
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
            CountryCode = s.CountryCode,
            SelectedPrecautions = s.SelectedPrecautions,
            Selections = s.Selections.Select(sel => new GrahRemedySelectionDto
            {
                NavgrahId = sel.NavgrahId,
                NavgrahName = sel.NavgrahName,
                Remedies = sel.Remedies.Select(r => new RemedyChoiceDto
                {
                    Name = r.Name,
                    IsPermanent = r.IsPermanent,
                    IsYearly = r.IsYearly
                }).ToList()
            }).ToList()
        };

        private static UserRemedyStaging ToDomain(UserRemedyStagingDto d) => new()
        {
            Id = d.Id,
            PersonId = d.PersonId,
            Name = d.Name,
            FatherName = d.FatherName,
            Gotra = d.Gotra,
            DOB = d.DOB,
            Time = d.Time,
            BirthPlace = d.BirthPlace,
            SelectedPrecautions = d.SelectedPrecautions,
            PhoneNo = d.PhoneNo,
            Address = d.Address,
            Grahan = d.Grahan,
            CountryCode = d.CountryCode,
            Selections = d.Selections.Select(sel => new GrahRemedySelection
            {
                NavgrahId = sel.NavgrahId,
                NavgrahName = sel.NavgrahName,
                Remedies = sel.Remedies.Select(r => new RemedyChoice
                {
                    Name = r.Name,
                    IsPermanent = r.IsPermanent,
                    IsYearly = r.IsYearly
                }).ToList()
            }).ToList()
        };
    }
}