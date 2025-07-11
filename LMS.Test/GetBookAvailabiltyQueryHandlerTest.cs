using AutoFixture;
using LMS.Application.Queries;
using LMS.Domain.Entities;
using LMS.Persistence.SQL;

namespace LMS.Test
{
    public class GetBookAvailabiltyQueryHandlerTest : IClassFixture<TestFixture>
    {
        private readonly LibraryDbContext _dbContext;

        private readonly GetBookAvailabilityQueryHandler _sut;

        private readonly IFixture _fixture;

        public GetBookAvailabiltyQueryHandlerTest(TestFixture fixture)
        {
            _dbContext = fixture.DbContext;
            _fixture = new Fixture();
            _sut = new GetBookAvailabilityQueryHandler(_dbContext);


        }
        [Fact]
        public async Task Handle_ShouldReturnBooksAvailability()
        {
            // Arrange
            var bookId = Guid.NewGuid();
            var book = _fixture.Build<Book>().Create();
            typeof(Entity).GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                ?.SetValue(book, bookId);

            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            var request = new BookAvailabilityQuery(bookId);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _sut.Handle(request, cancellationToken);

            //Assert 
            Assert.NotNull(result);
            Assert.Equal(result.Code, book.Code);
        }
    }
}
