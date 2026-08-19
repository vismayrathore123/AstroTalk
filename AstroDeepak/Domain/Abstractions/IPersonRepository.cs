using AstroDeepak.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IPersonRepository
    {
        Task<List<Person>> GetAllAsync();
        Task<Person?> GetByIdAsync(int id);
        Task<int> SaveAsync(Person person);
        Task<int> DeleteAsync(Person person);
    }
}
