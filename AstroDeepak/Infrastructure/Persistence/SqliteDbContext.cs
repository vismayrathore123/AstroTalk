using AstroDeepak.Domain.Entities;
using SQLite;

namespace AstroDeepak.Infrastructure.Persistence
{
    public class SqliteDbContext
    {
        private SQLiteAsyncConnection? _db;

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_db != null) return _db;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "astrodeepak.db3");
            System.Diagnostics.Debug.WriteLine($"DATABASE PATH: {dbPath}");
            _db = new SQLiteAsyncConnection(dbPath);

            await _db.CreateTableAsync<PersonEntity>();
            await _db.CreateTableAsync<NavgrahMasterEntity>();
            await _db.CreateTableAsync<GrahanMasterEntity>();

            await SeedMasterDataAsync(_db);

            return _db;
        }

        private static async Task SeedMasterDataAsync(SQLiteAsyncConnection db)
        {
            var navgrahCount = await db.Table<NavgrahMasterEntity>().CountAsync();
            if (navgrahCount == 0)
            {
                await db.InsertAllAsync(new List<NavgrahMasterEntity>
                {
                    new() { Name = "Surya",   Symbol = "☀️", SortOrder = 1 },
                    new() { Name = "Chandra", Symbol = "🌙", SortOrder = 2 },
                    new() { Name = "Mangal",  Symbol = "🔴", SortOrder = 3 },
                    new() { Name = "Budh",    Symbol = "💚", SortOrder = 4 },
                    new() { Name = "Guru",    Symbol = "🟡", SortOrder = 5 },
                    new() { Name = "Shukra",  Symbol = "🤍", SortOrder = 6 },
                    new() { Name = "Shani",   Symbol = "⚫", SortOrder = 7 },
                    new() { Name = "Rahu",    Symbol = "🐉", SortOrder = 8 },
                    new() { Name = "Ketu",    Symbol = "🌫️", SortOrder = 9 },
                });
            }

            var grahanCount = await db.Table<GrahanMasterEntity>().CountAsync();
            if (grahanCount == 0)
            {
                await db.InsertAllAsync(new List<GrahanMasterEntity>
                {
                    new() { Name = "Surya Grahan",   Symbol = "🌞", SortOrder = 1 },
                    new() { Name = "Chandra Grahan", Symbol = "🌘", SortOrder = 2 },
                });
            }
        }
    }
}