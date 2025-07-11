
using FluentValidation;
using LMS.Persistence.SQL;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Queries
{

    public record TopLendingUsersQuery(DateTime startDate, DateTime endDate, int topUserCount) : IRequest<List<TopLendingUsersResponse>>;

    public class TopLendingUsersResponse
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int LendingBooksCount { get; set; }
    }

    public class GetTopLendingUsersValidator : AbstractValidator<TopLendingUsersQuery>
    {
        public GetTopLendingUsersValidator()
        {
            RuleFor(x => x.topUserCount)
            .NotNull()
            .WithMessage("User count must be specified");

            RuleFor(x => x.startDate)
            .NotNull()
            .WithMessage("Start date must be specified");

            RuleFor(x => x.endDate)
            .NotNull()
            .WithMessage("End date must be specified");
        }
    }


    public class GetTopLendingUsersQueryHandler : IRequestHandler<TopLendingUsersQuery, List<TopLendingUsersResponse>>
    {
        private readonly ILibraryDbContext _dbContext;

        public GetTopLendingUsersQueryHandler(ILibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<TopLendingUsersResponse>> Handle(TopLendingUsersQuery request, CancellationToken cancellationToken)
        {
            var startdate = request.startDate == default(DateTime) ? DateTime.Now.AddDays(-30) : request.startDate;
            var endDate = request.endDate == default(DateTime) ? DateTime.Now : request.endDate;
            var top = request.topUserCount == 0 ? 10 : request.topUserCount;


            var result = await _dbContext.UserBookLendings
                .Where(x => x.LendingDate.Date >= startdate
                && x.LendingDate.Date <= endDate)
                .GroupBy(x => x.UserId)
                .OrderByDescending(g => g.Count())
                .Take(top)
                .Select(x => new TopLendingUsersResponse
                {
                    UserId = x.Key,
                    Email = x.First().User.Email,
                    Phone = x.First().User.PhoneNumber,
                    LendingBooksCount = x.Count(),
                    Name = $"{x.First().User.LastName} {x.First().User.FirstName}"
                }).ToListAsync(cancellationToken);

            return result;
        }
    }
}
