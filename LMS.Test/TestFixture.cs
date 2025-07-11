using LMS.Persistence.SQL;
using Microsoft.EntityFrameworkCore;

namespace LMS.Test
{
    public class TestFixture : IDisposable
    {
        public LibraryDbContext DbContext { get; }

        public TestFixture()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;
            DbContext = new LibraryDbContext(options);
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Dispose();
        }
    }
}
