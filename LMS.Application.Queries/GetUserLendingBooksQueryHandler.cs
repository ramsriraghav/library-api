
using FluentValidation;
using LMS.Persistence.SQL;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Queries
{

    public record UserLendingBooksQuery(Guid userId, DateTime start,DateTime endDate) : IRequest<List<LendingBooksResponse>>;

    public record LendingRelatedBooksQuery(Guid BookId) : IRequest<List<LendingBooksResponse>>;

    public class LendingBooksResponse
    {
        public Guid BookId { get; set; }
        public string BookCode { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime LendingDate { get; set; }
        public DateTime? SubmittedDate { get; set; }

    }

    public class GetUserLendingBooksValidator : AbstractValidator<UserLendingBooksQuery>
    {
        public GetUserLendingBooksValidator()
        {
            RuleFor(x => x.userId)
            .NotNull()
            .WithMessage("User must be specified");

            RuleFor(x => x.start)
            .NotNull()
            .WithMessage("Start date must be specified");

            RuleFor(x => x.endDate)
            .NotNull()
            .WithMessage("End date must be specified");
        }
    }

    public class GetUserLendingOtherBooksValidator : AbstractValidator<LendingRelatedBooksQuery>
    {
        public GetUserLendingOtherBooksValidator()
        {
            RuleFor(x => x.BookId)
            .NotNull()
            .WithMessage("Book must be specified");
        }
    }

    public class GetUserLendingBooksQueryHandler :
        IRequestHandler<UserLendingBooksQuery, List<LendingBooksResponse>>,
        IRequestHandler<LendingRelatedBooksQuery, List<LendingBooksResponse>>
    {
        private readonly ILibraryDbContext _dbContext;

        public GetUserLendingBooksQueryHandler(ILibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<LendingBooksResponse>> Handle(UserLendingBooksQuery request, CancellationToken cancellationToken)
        {

            var startdate = request.start == default(DateTime) ? DateTime.Now.AddDays(-30) : request.start;
            var endDate = request.endDate == default(DateTime) ? DateTime.Now : request.endDate;


            var result = await _dbContext.UserBookLendings
                .Include(x => x.Book)
                .Where(x => x.UserId == request.userId && x.LendingDate.Date >= startdate.Date
                && x.LendingDate.Date <= endDate.Date)
                .Select(x => new LendingBooksResponse
                {
                    Title = x.Book.Title,
                    Author = x.Book.Author,
                    BookCode = x.Book.Code,
                    BookId = x.Book.Id,
                    LendingDate = x.LendingDate,
                    SubmittedDate = x.SubmittedDate
                }).ToListAsync(cancellationToken);
            return result;
        }

        public async Task<List<LendingBooksResponse>> Handle(LendingRelatedBooksQuery request, CancellationToken cancellationToken)
        {
            var userList = await _dbContext.UserBookLendings.Where(x => x.BookId == request.BookId)
                .Select(x => x.UserId).Distinct()
                .ToListAsync(cancellationToken);

            var result = await _dbContext.UserBookLendings
                .Include(x => x.Book)
                .Where(x => userList.Contains(x.UserId) && x.BookId != request.BookId)
                .Select(x => new LendingBooksResponse
                {

                    Title = x.Book.Title,
                    Author = x.Book.Author,
                    BookCode = x.Book.Code,
                    BookId = x.Book.Id,
                    LendingDate = x.LendingDate,
                    SubmittedDate = x.SubmittedDate
                }).ToListAsync(cancellationToken);

            return result;
        }
    }
}
