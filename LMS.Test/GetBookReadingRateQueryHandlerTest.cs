using LMS.Application.Queries;
using LMS.Domain.Entities;
using LMS.Persistence.SQL;
using System.Reflection;

namespace LMS.Test
{
    public class GetBookReadingRateQueryHandlerTest : IClassFixture<TestFixture>
    {
        private readonly LibraryDbContext _dbContext;

        private readonly GetBookReadingRateQueryHandler _sut;


        public GetBookReadingRateQueryHandlerTest(TestFixture fixture)
        {
            _dbContext = fixture.DbContext;
            _sut = new GetBookReadingRateQueryHandler(_dbContext);
        }
        [Theory, LibraryAutoData]
        public async Task Handle_ShouldReturnBookReadingRate(UserBookLending lending)
        {
            // Arrange
            typeof(Book).GetProperty("Pages", BindingFlags.Public | BindingFlags.Instance)
                    ?.SetValue(lending.Book, 400);

            typeof(UserBookLending).GetProperty("LendingDate", BindingFlags.Public | BindingFlags.Instance)
                     ?.SetValue(lending, DateTime.Now.AddDays(-10));
            typeof(UserBookLending).GetProperty("SubmittedDate", BindingFlags.Public | BindingFlags.Instance)
                    ?.SetValue(lending, DateTime.Now);

            await _dbContext.AddAsync(lending);
            await _dbContext.SaveChangesAsync();

            var request = new BookReadingRateQuery(lending.BookId);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _sut.Handle(request, cancellationToken);

            //Assert 

            Assert.NotNull(result);
            Assert.True(result.Average > 0,$"{lending.Book.Title} average must be greater than 0" );
            Assert.Equal(result.Title, lending.Book.Title);
            Assert.Equal(result.Code, lending.Book.Code);
        }
    }
}
