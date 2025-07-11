using Grpc.Core;
using LMS.Application.Queries;
using MediatR;

namespace LMS_grpc.Services;

public class LibraryService : LMS_grpc.LibraryService.LibraryServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LibraryService> _logger;

    public LibraryService(IMediator mediator, ILogger<LibraryService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<MostBorrowedBooksResponse> GetMostBorrowedBooks(MostBorrowedBooksRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching top {TopN} borrowed books", request.TopN);
        var result = await _mediator.Send(new MostLendingBooksQuery(request.TopN));
        var response = new MostBorrowedBooksResponse();
        response.Books.AddRange(result.Select(b => new MostBorrowedBook
        {
            Title = b.Title,
            BorrowCount = b.Count
        }));
        return response;
    }

    public override async Task<BookAvailabilityResponse> GetBookAvailability(BookAvailabilityRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching availability for book ID {BookId}", request.BookId);
        var bookId = Guid.Parse(request.BookId);
        var result = await _mediator.Send(new BookAvailabilityQuery(bookId));
        return new BookAvailabilityResponse
        {
            Code = result.Code,
            TotalCopies = result.TotalCopies,
            AvailableCopies = result.AvailableCopiesCount
        };
    }

    public override async Task<ReadingRateResponse> GetReadingRate(ReadingRateRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching reading rate for book ID {BookId}", request.BookId);
        var bookId = Guid.Parse(request.BookId);
        var result = await _mediator.Send(new BookReadingRateQuery(bookId));
        return new ReadingRateResponse { Rate = result.Average };
    }

    public override async Task<TopBorrowersResponse> GetTopBorrowers(TopBorrowersRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching top {TopN} borrowers", request.TopN);
        var result = await _mediator.Send(new TopLendingUsersQuery(DateTime.Now.AddDays(-30), DateTime.Now,request.TopN));
        var response = new TopBorrowersResponse();
        response.Borrowers.AddRange(result.Select(b => new TopBorrower
        {
            FullName = b.Name,
            BorrowCount = b.LendingBooksCount
        }));
        return response;
    }

    public override async Task<UserBorrowHistoryResponse> GetUserBorrowHistory(UserBorrowHistoryRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching borrow history for user ID {UserId}", request.UserId);
        var userId = Guid.Parse(request.UserId);
        var result = await _mediator.Send(new UserLendingBooksQuery(userId, DateTime.Now.AddDays(-30), DateTime.Now));
        var response = new UserBorrowHistoryResponse();
        response.History.AddRange(result.Select(h => new UserBorrowHistory
        {
            BookTitle = h.Title,
            BorrowedAt = h.LendingDate.ToString("o"),
            ReturnedAt = h.SubmittedDate?.ToString("o")
        }));
        return response;
    }

    public override async Task<RelatedBooksResponse> GetRelatedBooks(RelatedBooksRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching related books for book ID {BookId}", request.BookId);
        var bookId = Guid.Parse(request.BookId);
        var result = await _mediator.Send(new LendingRelatedBooksQuery(bookId));
        var response = new RelatedBooksResponse();
        response.Books.AddRange(result.Select(b => new RelatedBook
        {
            Id = b.BookId.ToString(),
            Title = b.Title,
            Author = b.Author
        }));
        return response;
    }
}
