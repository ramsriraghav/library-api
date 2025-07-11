using LMS.Application.Queries;
using LMS.Domain.Entities;
using LMS.Persistence.SQL;
using System.Reflection;

namespace LMS.Test
{
    public class GetUserLendingBooksQueryHandlerTest : IClassFixture<TestFixture>
    {
        private readonly LibraryDbContext _dbContext;

        private readonly GetUserLendingBooksQueryHandler _sut;


        public GetUserLendingBooksQueryHandlerTest(TestFixture fixture)
        {
            _dbContext = fixture.DbContext;
            _sut = new GetUserLendingBooksQueryHandler(_dbContext);
        }
        [Theory, LibraryAutoData]
        public async Task Handle_ShouldReturnLendingBooksByUser(UserBookLending lending)
        {
            // Arrange
            typeof(UserBookLending).GetProperty("LendingDate", BindingFlags.Public | BindingFlags.Instance)
                     ?.SetValue(lending, DateTime.Now.AddDays(-10));
            typeof(UserBookLending).GetProperty("SubmittedDate", BindingFlags.Public | BindingFlags.Instance)
                    ?.SetValue(lending, DateTime.Now);

            await _dbContext.AddAsync(lending);
            await _dbContext.SaveChangesAsync();

            var request = new UserLendingBooksQuery(lending.UserId, DateTime.Now.AddDays(-30), DateTime.Now);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _sut.Handle(request, cancellationToken);

            //Assert 

            Assert.NotNull(result);
            var firstResult = result.First();
            Assert.Equal(firstResult.BookCode, lending.Book.Code);
        }

        [Theory, LibraryAutoData]
        public async Task Handle_ShouldReturnOtherLendingBooksByUser(UserBookLending lending)
        {
            // Arrange
            typeof(UserBookLending).GetProperty("LendingDate", BindingFlags.Public | BindingFlags.Instance)
                     ?.SetValue(lending, DateTime.Now.AddDays(-10));
            typeof(UserBookLending).GetProperty("SubmittedDate", BindingFlags.Public | BindingFlags.Instance)
                    ?.SetValue(lending, DateTime.Now);

            await _dbContext.AddAsync(lending);
            await _dbContext.SaveChangesAsync();

            var request = new LendingRelatedBooksQuery(lending.BookId);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _sut.Handle(request, cancellationToken);

            //Assert 

            Assert.True(result.Count == 0, $"It should fetch 0 results, but actual was {result.Count}");
        }
    }
}

