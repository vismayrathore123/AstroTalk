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
            return _db;
        }
    }
}
