using AstroDeepak.Domain.Abstractions;
using AstroDeepak.Domain.Entities;
using AstroDeepak.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroDeepak.Infrastructure.Repositories
{
    public class MasterDataRepository : IMasterDataRepository
    {
        private readonly SqliteDbContext _context;

        public MasterDataRepository(SqliteDbContext context)
        {
            _context = context;
        }

        public async Task<List<NavgrahMaster>> GetNavgrahsAsync()
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<NavgrahMasterEntity>().ToListAsync();
            return rows.OrderBy(r => r.SortOrder)
                      .Select(r => new NavgrahMaster { Id = r.Id, Name = r.Name, SortOrder = r.SortOrder })
                      .ToList();
        }

        public async Task<List<GrahanMaster>> GetGrahansAsync()
        {
            var db = await _context.GetConnectionAsync();
            var rows = await db.Table<GrahanMasterEntity>().ToListAsync();
            return rows.OrderBy(r => r.SortOrder)
                       .Select(r => new GrahanMaster { Id = r.Id, Name = r.Name, Symbol = r.Symbol, SortOrder = r.SortOrder })
                       .ToList();
        }
    }
}
