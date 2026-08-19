using AstroDeepak.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Domain.Abstractions
{
    public interface IMasterDataRepository
    {
        Task<List<NavgrahMaster>> GetNavgrahsAsync();
        Task<List<GrahanMaster>> GetGrahansAsync();
    }
}
