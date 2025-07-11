using LMS.Domain.Entities;
using LMS.Persistence.SQL;
using Microsoft.EntityFrameworkCore;

namespace LMS.Test
{
    public static class DataInitializer
    {
        public static void SeedData(LibraryDbContext context)
        {
            var random = new Random();

            // Create Books
            var book1 = new Book
            (
                 "The Great Gatsby",
                 "F. Scott Fitzgerald",
                 $"ISBN-978-0-111111111",
                 "Penguin Books",
                "Fiction",
                450,
                2022, 5);

            var book2 = new Book
            (
                 "The Silent Echo",
                 "Robert Wilson",
                 $"ISBN-978-0-2222222222",
                 "Random House",
                "Fantasy",
                500,
                2012, 15);

            var book3 = new Book
            (
                 "Shadows of Time",
                 "Emily Davis",
                 $"ISBN-978-0-33333333333",
                 "HarperCollins",
                "Biography",
                850,
                2002, 8);



            // Create Users
            var user1 = new User("Alice", "Bob", DateTime.Now.AddYears(-40),"123567 89","alice@gmail.com","1st street, Stockholm");
            
            var user2 = new User("Charlie", "Diana", DateTime.Now.AddYears(-28), "123587 89", "Charlie@gmail.com", "2nd street, Stockholm");

            var user3 = new User("Edward", "Fiona", DateTime.Now.AddYears(-35), "123597 89", "Edward@gmail.com", "3rd street, Stockholm");


            // Add entities to the context
            context.Books.AddRange(book1, book2, book3);
            context.Users.AddRange(user1, user2, user3);
            // Save changes
            context.SaveChanges();

            var lendings = GenerateUserBookLendings(context.Books.ToList(), context.Users.ToList(), 5);
            context.UserBookLendings.AddRange(lendings);
            context.SaveChanges();
            VerifySeededData(context);
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


        private static void VerifySeededData(LibraryDbContext _context)
        {
            var books =  _context.Books.Include(b => b.UserBookLendings).ToList();
            var users =  _context.Users.Include(u => u.UserBookLendings).ToList();
            var borrowRecords =  _context.UserBookLendings.Include(br => br.Book).Include(br => br.User).ToList();

            Assert.Equal(3, books.Count);
            Assert.Equal(3, users.Count);
            Assert.Equal(5, borrowRecords.Count);
            Assert.All(borrowRecords, br => Assert.NotNull(br.Book));
            Assert.All(borrowRecords, br => Assert.NotNull(br.User));
        }
    }
}
