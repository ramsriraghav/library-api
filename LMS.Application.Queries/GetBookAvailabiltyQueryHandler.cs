using FluentValidation;
using LMS.Persistence.SQL;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Queries
{

    public record BookAvailabilityQuery(Guid bookId) : IRequest<BookAvailabilityResponse>;

    public class BookAvailabilityResponse
    {
        public string Code { get; set; }
        public bool IsAvalable { get; set; } = default!;
        public int AvailableCopiesCount { get; set; } = 0;
        public int TotalCopies { get; set; } = 0;
    }

    public class GetBookAvailabilityValidator : AbstractValidator<BookAvailabilityQuery>
    {
        public GetBookAvailabilityValidator()
        {
            RuleFor(x => x.bookId)
            .NotNull()
            .WithMessage("Book Id must be specified");
        }
    }


    public class GetBookAvailabilityQueryHandler
        : IRequestHandler<BookAvailabilityQuery, BookAvailabilityResponse>
    {
        private readonly ILibraryDbContext _dbContext;

        public GetBookAvailabilityQueryHandler(ILibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
       

        public async Task<BookAvailabilityResponse> Handle(BookAvailabilityQuery request, CancellationToken cancellationToken)
        {
            var result = await _dbContext.Books
                .Where(x => x.Id == request.bookId && x.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (result != null) {

                return new BookAvailabilityResponse
                {
                    AvailableCopiesCount = result.AvailableCopies,
                    Code = result.Code,
                    IsAvalable = result.AvailableCopies > 0,
                    TotalCopies = result.TotalCopies
                };
            }

            return default(BookAvailabilityResponse);
        }
    }
}
