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

            _db = new SQLiteAsyncConnection(dbPath, storeDateTimeAsTicks: false);

            await _db.CreateTableAsync<PersonEntity>();
            await _db.CreateTableAsync<NavgrahMasterEntity>();
            await _db.CreateTableAsync<GrahanMasterEntity>();
            await _db.CreateTableAsync<RemedyMasterEntity>();
            await _db.CreateTableAsync<UsersRemedyEntity>();
            // NavgrahRemedyEntity table removed - RemedyMaster.NavgrahId replaces it.

            await SeedMasterDataAsync(_db);

            return _db;
        }

        private static async Task SeedMasterDataAsync(SQLiteAsyncConnection db)
        {
            var now = DateTime.Now;

            var navgrahCount = await db.Table<NavgrahMasterEntity>().CountAsync();
            if (navgrahCount == 0)
            {
                await db.InsertAllAsync(new List<NavgrahMasterEntity>
                {
                    new() { Name = "Surya",   SortOrder = 1, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Chandra", SortOrder = 2, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Mangal",  SortOrder = 3, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Budh",    SortOrder = 4, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Guru",    SortOrder = 5, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Shukra",  SortOrder = 6, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Shani",   SortOrder = 7, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Rahu",    SortOrder = 8, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Ketu",    SortOrder = 9, CreatedAt = now, UpdatedAt = now },
                });
            }

            var grahanCount = await db.Table<GrahanMasterEntity>().CountAsync();
            if (grahanCount == 0)
            {
                await db.InsertAllAsync(new List<GrahanMasterEntity>
                {
                    new() { Name = "None",           SortOrder = 0, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Surya Grahan",   SortOrder = 1, CreatedAt = now, UpdatedAt = now },
                    new() { Name = "Chandra Grahan", SortOrder = 2, CreatedAt = now, UpdatedAt = now },
                });
            }

            // RemedyMaster is no longer seeded generically - remedies are added
            // per-Navgrah via Hamburger -> Add Remedies.
        }
    }
}