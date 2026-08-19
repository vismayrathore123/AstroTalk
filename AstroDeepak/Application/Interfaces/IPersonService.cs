using AstroDeepak.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Application.Interfaces
{
    public interface IPersonService
    {
        Task<List<PersonDto>> GetAllAsync();
        Task<PersonDto?> GetByIdAsync(int id);
        Task<int> SaveAsync(PersonDto dto);
        Task<int> DeleteAsync(int id);
    }
}
