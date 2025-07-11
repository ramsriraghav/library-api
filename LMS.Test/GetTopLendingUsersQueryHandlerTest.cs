using LMS.Application.Queries;
using LMS.Domain.Entities;
using LMS.Persistence.SQL;
using System.Reflection;

namespace LMS.Test
{
    public class GetTopLendingUsersQueryHandlerTest : IClassFixture<TestFixture>
    {
        private readonly LibraryDbContext _dbContext;

        private readonly GetTopLendingUsersQueryHandler _sut;


        public GetTopLendingUsersQueryHandlerTest(TestFixture fixture)
        {
            _dbContext = fixture.DbContext;
            _sut = new GetTopLendingUsersQueryHandler(_dbContext);
        }
        [Theory, LibraryAutoData]
        public async Task Handle_ShouldReturnTopLendingUserBooks(UserBookLending lending)
        {
            // Arrange
            typeof(UserBookLending).GetProperty("LendingDate", BindingFlags.Public | BindingFlags.Instance)
                     ?.SetValue(lending, DateTime.Now.AddDays(-10));
            typeof(UserBookLending).GetProperty("SubmittedDate", BindingFlags.Public | BindingFlags.Instance)
                    ?.SetValue(lending, DateTime.Now);

            await _dbContext.AddAsync(lending);
            await _dbContext.SaveChangesAsync();

            var request = new TopLendingUsersQuery(DateTime.Now.AddDays(-30), DateTime.Now,10);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await _sut.Handle(request, cancellationToken);

            //Assert 

            Assert.NotNull(result);
            var firstResult = result.First();
            Assert.Equal(firstResult.UserId, lending.UserId);
        }
    }
}

