using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Persistence.SQL
{
    public enum DatabaseSize
    {
        Small,
        Medium,
        Large,
        XLarge
    }


    public class ApplicationDbContextInitializer
    {
        private readonly ILogger<ApplicationDbContextInitializer> _logger;
        private readonly LibraryDbContext _context;

        public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger,
            LibraryDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync(DatabaseSize dbSize = DatabaseSize.Medium)
        {
            try
            {
                await TrySeedAsync(dbSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public async Task TrySeedAsync(DatabaseSize dbSize)
        {
            var defaultRows = 20;

            switch (dbSize)
            {
                case DatabaseSize.Small:
                    defaultRows = 20;
                    break;
                case DatabaseSize.Medium:
                    defaultRows = 100;
                    break;
                case DatabaseSize.Large:
                    defaultRows = 500;
                    break;
                case DatabaseSize.XLarge:
                    defaultRows = 2000;
                    break;

            }


            // Ensure database is created
            _context.Database.EnsureCreated();

            if (!_context.Books.Any())
            {

                // Seed Books
                var books = GenerateBooks(defaultRows);
                _context.Books.AddRange(books);
                _context.SaveChanges();
            }

            if (!_context.Users.Any())
            {
                // Seed Users
                var users = GenerateUsers(defaultRows / 10);
                _context.Users.AddRange(users);
                _context.SaveChanges();
            }


            if (!_context.UserBookLendings.Any())
            {
                var dbBooks = await _context.Books.ToListAsync();
                var dbUsers = await _context.Users.ToListAsync();
                // Seed UserBookLendings
                var lendings = GenerateUserBookLendings(dbBooks, dbUsers, defaultRows);
                _context.UserBookLendings.AddRange(lendings);
                _context.SaveChanges();
            }
        }


        private static List<Book> GenerateBooks(int count)
        {
            var genres = new[] { "Fiction", "Non-Fiction", "Science Fiction", "Fantasy", "Mystery", "Thriller", "Romance", "Biography", "History", "Self-Help" };
            var publishers = new[] { "Penguin Books", "Random House", "HarperCollins", "Simon & Schuster", "Macmillan", "Hachette" };
            var titles = new[] { "The Silent Echo", "Shadows of Time", "Beyond the Horizon", "Whispers in the Dark", "The Last Voyage", "Dreams of Eternity", "Hidden Truths", "Echoes of the Past" };
            var authors = new[] { "John Smith", "Jane Doe", "Michael Brown", "Emily Davis", "Robert Wilson", "Sarah Johnson", "David Lee", "Laura Martin" };
            var random = new Random();
            var books = new List<Book>();

            for (int i = 0; i < count; i++)
            {
                var title = $"{titles[random.Next(titles.Length)]} {i + 1}";
                var author = authors[random.Next(authors.Length)];
                var code = $"ISBN-978-0-{random.Next(100, 999)}-{random.Next(10000, 99999)}-{random.Next(0, 9)}";
                var publisher = publishers[random.Next(publishers.Length)];
                var genre = genres[random.Next(genres.Length)];
                var pages = random.Next(100, 600);
                var releasedYear = random.Next(1980, 2025);
                var totalCopies = random.Next(1, 10);
                var book = new Book(title, author, code, publisher, genre, pages, releasedYear, totalCopies);
                books.Add(book);
            }

            return books;
        }

        private static List<User> GenerateUsers(int count)
        {
            var firstNames = new[] { "Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona", "George", "Hannah" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis" };
            var domains = new[] { "gmail.com", "yahoo.com", "outlook.com", "example.com" };
            var random = new Random();
            var users = new List<User>();

            for (int i = 0; i < count; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var birthDate = DateTime.Now.AddYears(-random.Next(18, 70)).AddDays(-random.Next(0, 365)).Date;
                var phoneNumber = $"555-{random.Next(100, 999)}-{random.Next(1000, 9999)}";
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}{random.Next(1, 100)}@{domains[random.Next(domains.Length)]}";
                var address = $"{random.Next(100, 9999)} Main St, City {random.Next(1, 100)}, Sweden";
                var user = new User(firstName, lastName, birthDate, phoneNumber, email, address);
                users.Add(user);
            }

            return users;
        }

        private static List<UserBookLending> GenerateUserBookLendings(List<Book> books, List<User> users, int count)
        {
            var random = new Random();
            var lendings = new List<UserBookLending>();
            var remarksOptions = new[] { null, "Returned in good condition", "Slight wear on cover", "Missing dust jacket", "Returned late" };

            for (int i = 0; i < count; i++)
            {
                var book = books[random.Next(books.Count)];
                var user = users[random.Next(users.Count)];
                var lendingDate = DateTime.Now.AddDays(-random.Next(1, 365));
                var lending = new UserBookLending(book.Id, user.Id, lendingDate);

                // Randomly decide if the book has been returned (70% chance)
                if (random.NextDouble() < 0.7)
                {
                    var submittedDate = lendingDate.AddDays(random.Next(1, 30));
                    var remark = remarksOptions[random.Next(remarksOptions.Length)];
                    lending.UpdateSubmittedDate(submittedDate, remark);
                    book.IncrementAvailableCopies(); // Adjust available copies
                    user.IncreamentLendingBookCount();
                }
                else
                {
                    book.DecrementAvailableCopies();
                    user.IncreamentLendingBookCount();
                }

                lendings.Add(lending);
            }

            return lendings;
        }
    }
}
