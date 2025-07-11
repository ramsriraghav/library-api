
using FluentValidation;
using LMS.Persistence.SQL;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Queries
{
    public record BookReadingRateQuery(Guid bookId) :IRequest<BookReadingRateResponse>;

    public class BookReadingRateResponse
    {
        public string Code { get; set; }
        public string  Title { get; set; }
        public double Average { get; set; }
    }

    public class BookReadingRateQueryValidator : AbstractValidator<BookReadingRateQuery>
    {
        public BookReadingRateQueryValidator()
        {
            RuleFor(x => x.bookId)
           .NotNull()
           .WithMessage("Book Id must be specified");
        }
    }

    public class GetBookReadingRateQueryHandler : IRequestHandler<BookReadingRateQuery, BookReadingRateResponse>
    {
        private readonly ILibraryDbContext _dbContext;
        public GetBookReadingRateQueryHandler(ILibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<BookReadingRateResponse> Handle(BookReadingRateQuery request, CancellationToken cancellationToken)
        {
            var lendingBookRecords = await _dbContext.UserBookLendings
                .Include(x=>x.Book)
                .Where(x=> x.BookId == request.bookId && x.SubmittedDate.HasValue)
                .ToListAsync(cancellationToken);

            if(!lendingBookRecords.Any()) 
                return default(BookReadingRateResponse);

            var readingRate = lendingBookRecords
                .Select(x => x.Book.Pages / (x.SubmittedDate.Value.Date - x.LendingDate.Date).TotalDays)
                .Average();
                ;

            return new BookReadingRateResponse
            {

                Average = Math.Round(readingRate,2),
                Code = lendingBookRecords.First().Book.Code,
                Title = lendingBookRecords.First().Book.Title,
            };
        }
    }
}
