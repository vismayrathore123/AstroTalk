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
            await _db.CreateTableAsync<RemedyMasterEntity>();
            await _db.CreateTableAsync<NavgrahRemedyEntity>();

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
                    new() { Name = "Surya",   SortOrder = 1 },
                    new() { Name = "Chandra", SortOrder = 2 },
                    new() { Name = "Mangal",  SortOrder = 3 },
                    new() { Name = "Budh",    SortOrder = 4 },
                    new() { Name = "Guru",    SortOrder = 5 },
                    new() { Name = "Shukra",  SortOrder = 6 },
                    new() { Name = "Shani",   SortOrder = 7 },
                    new() { Name = "Rahu",    SortOrder = 8 },
                    new() { Name = "Ketu",    SortOrder = 9 },
                });
            }

            var grahanCount = await db.Table<GrahanMasterEntity>().CountAsync();
            if (grahanCount == 0)
            {
                await db.InsertAllAsync(new List<GrahanMasterEntity>
                {
                    new() { Name = "None",           Symbol = "",   SortOrder = 0 },
                    new() { Name = "Surya Grahan",   Symbol = "🌞", SortOrder = 1 },
                    new() { Name = "Chandra Grahan", Symbol = "🌘", SortOrder = 2 },
                });
            }

            var remedyCount = await db.Table<RemedyMasterEntity>().CountAsync();
            if (remedyCount == 0)
            {
                await db.InsertAllAsync(new List<RemedyMasterEntity>
                {
                    new() { Name = "Chant Beej Mantra",     SortOrder = 1 },
                    new() { Name = "Fast on Related Day",   SortOrder = 2 },
                    new() { Name = "Wear Gemstone",         SortOrder = 3 },
                    new() { Name = "Donate on Related Day", SortOrder = 4 },
                    new() { Name = "Worship Related Deity", SortOrder = 5 },
                    new() { Name = "Visit Temple",          SortOrder = 6 },
                    new() { Name = "Recite Stotram",        SortOrder = 7 },
                    new() { Name = "Feed Animals / Birds",  SortOrder = 8 },
                });
            }
        }
    }
}