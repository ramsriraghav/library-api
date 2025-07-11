using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace LMS.Persistence.SQL
{
    public interface ILibraryDbContext
    {
        DbSet<Book> Books { get; }
        DbSet<User> Users { get; }
        DbSet<UserBookLending> UserBookLendings { get; }
    }
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }


    public class LibraryDbContext([NotNull] DbContextOptions options) : DbContext(options),IUnitOfWork, ILibraryDbContext
    {
        private readonly ILogger<LibraryDbContext>? _logger;

        public DbSet<Book> Books { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserBookLending> UserBookLendings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasKey(b => b.Id);
            modelBuilder.Entity<Book>().Property(b=> b.Id).ValueGeneratedOnAdd();


            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedOnAdd();



            modelBuilder.Entity<UserBookLending>().HasKey(br => br.Id);
            modelBuilder.Entity<UserBookLending>().Property(ub => ub.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UserBookLending>()
                .HasOne(ub => ub.Book)
                .WithMany(b => b.UserBookLendings)
                .HasForeignKey(ub => ub.BookId);
            modelBuilder.Entity<UserBookLending>()
                .HasOne(ub => ub.User)
                .WithMany(u => u.UserBookLendings)
                .HasForeignKey(ub => ub.UserId);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                return await base.SaveChangesAsync(true, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger!.LogError($"Error occurred when commiting changes.{ex}");
                throw;
            }
        }
    }
}
