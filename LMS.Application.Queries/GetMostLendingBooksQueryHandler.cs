using FluentValidation;
using LMS.Persistence.SQL;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Application.Queries
{

    public record MostLendingBooksQuery(int TopN) : IRequest<List<MostLendingBooksResponse>>;

    public class MostLendingBooksResponse
    {
        public string Code { get; set; }
        public string Title { get; set; } = default!;
        public int Count { get; set; }
    }

    public class GetMostLendingBooksValidator : AbstractValidator<MostLendingBooksQuery>
    {
        public GetMostLendingBooksValidator()
        {
            RuleFor(x => x.TopN)
            .GreaterThan(0).WithMessage("TopN must be greater than 0.");
        }
    }


    public class GetMostLendingBooksQueryHandler 
        : IRequestHandler<MostLendingBooksQuery, List<MostLendingBooksResponse>>
    {
        private readonly ILibraryDbContext _dbContext;

        public GetMostLendingBooksQueryHandler(ILibraryDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<MostLendingBooksResponse>> Handle(MostLendingBooksQuery request, CancellationToken cancellationToken)
        {
            //var result = await _dbContext.Books.OrderByDescending(x => x.TotalNumberOfLendings)
            //    .Take(request.TopN)
            //    .Select(x => new MostLendingBooksResponse
            //    {
            //        Code = x.Code,
            //        Title = x.Title,
            //        Count = x.TotalNumberOfLendings
            //    }).ToListAsync(cancellationToken);


            var result = await _dbContext.UserBookLendings
                .Include(x=>x.Book)
                .GroupBy(x=>x.Book.Code)
                .OrderByDescending(x => x.Count())
                .Take(request.TopN)
                .Select(x => new MostLendingBooksResponse
                {
                    Code = x.Key,
                    Title = x.First().Book.Title,
                    Count = x.Count()
                }).ToListAsync(cancellationToken);

            return result;
        }
    }
}
