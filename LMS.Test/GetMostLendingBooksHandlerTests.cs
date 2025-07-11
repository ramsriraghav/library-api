
using LMS.Application.Queries;
using LMS.Domain.Entities;
using LMS.Persistence.SQL;

namespace LMS.Test
{
    public class GetMostLendingBooksHandlerTests : IClassFixture<TestFixture>
    {
        private readonly LibraryDbContext _dbContext;

        private readonly GetMostLendingBooksQueryHandler _sut;


        public GetMostLendingBooksHandlerTests(TestFixture fixture)
        {
            _dbContext = fixture.DbContext;
            _sut = new GetMostLendingBooksQueryHandler(_dbContext);
        }
        [Theory, LibraryAutoData]
        public async Task Handle_ShouldReturnMostLendingBooks(UserBookLending lending)
        {
            // Arrange
            await _dbContext.AddAsync(lending);
            await _dbContext.SaveChangesAsync();

            var request = new MostLendingBooksQuery(5);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _sut.Handle(request,cancellationToken);

            //Assert 

            Assert.NotNull(result);
            var firstResult = result.First();
            Assert.Equal(firstResult.Title, lending.Book.Title);
        }
    }
}
