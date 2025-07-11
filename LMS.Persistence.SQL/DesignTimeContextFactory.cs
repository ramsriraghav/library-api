using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LMS.Persistence.SQL
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
    {
        public LibraryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=LibraryDesignTime;Trusted_Connection=True;TrustServerCertificate=True;");
            return new LibraryDbContext(optionsBuilder.Options);
        }

    }
}
