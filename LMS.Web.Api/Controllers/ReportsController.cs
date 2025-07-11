using LMS.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : Controller
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("most-lending")]
        public async Task<IActionResult> GetMostLendingBooks([FromQuery] int topN)
        {
            var result = await _mediator.Send(new MostLendingBooksQuery(topN));
            return Ok(result);
        }

        [HttpGet("book-availability/{bookId}")]
        public async Task<IActionResult> GetBookAvailability(Guid bookId)
        {
            var result = await _mediator.Send(new BookAvailabilityQuery(bookId));
            return Ok(result);
        }

        [HttpGet("top-lenders")]
        public async Task<IActionResult> GetTopLenders([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int topN)
        {
            var result = await _mediator.Send(new TopLendingUsersQuery(startDate, endDate, topN));
            return Ok(result);
        }

        [HttpGet("user-lending-history/{userId}")]
        public async Task<IActionResult> GetUserLendingHistory(Guid userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _mediator.Send(new UserLendingBooksQuery(userId, startDate, endDate));
            return Ok(result);
        }

        [HttpGet("related-books/{bookId}")]
        public async Task<IActionResult> GetRelatedBooks(Guid bookId)
        {
            var result = await _mediator.Send(new LendingRelatedBooksQuery(bookId));
            return Ok(result);
        }

        [HttpGet("reading-rate/{bookId}")]
        public async Task<IActionResult> GetReadingRate(Guid bookId)
        {
            var result = await _mediator.Send(new BookReadingRateQuery(bookId));
            return Ok(result);
        }
    }
}
