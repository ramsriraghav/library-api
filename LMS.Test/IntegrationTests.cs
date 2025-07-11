
using FluentValidation;
using LMS.Application.Queries;
using LMS.Domain.Behaviours;
using LMS.Persistence.SQL;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LMS.Test
{
    public class IntegrationTests : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly LibraryDbContext _context;
        private bool _disposed;

        public IntegrationTests()
        {
            var services = new ServiceCollection();
            //services.AddApplicationServices();

            services.AddLogging(builder =>
            {
                builder.AddConsole(); // Use console logger
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Explicitly register ILoggerFactory
            services.AddSingleton<ILoggerFactory, LoggerFactory>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetBookAvailabilityValidator).Assembly));
            services.AddValidatorsFromAssembly(typeof(GetBookAvailabilityValidator).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SafeHandleExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            var connString = $"Server=.;Database=IntegrationTest_Library_{Guid.NewGuid().ToString()};Trusted_Connection=True;TrustServerCertificate=True;";

            var inMemoryOptions = new DbContextOptionsBuilder<LibraryDbContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
               .Options;

            services.AddDbContext<LibraryDbContext>((serviceProvider, options) =>
            {
                //options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

                options.UseSqlServer(connString);
            }).AddScoped(provider =>
                    (ILibraryDbContext)provider.GetRequiredService<LibraryDbContext>())
                .AddScoped(provider => (IUnitOfWork)provider.GetRequiredService<LibraryDbContext>());

            

            _serviceProvider = services.BuildServiceProvider();

            // Seed data
            using var scope = _serviceProvider.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            _context.Database.EnsureCreated();
            DataInitializer.SeedData(_context);

        }

        [Fact]
        public async Task GetMostLentBooks_ShouldReturnResults()
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new MostLendingBooksQuery(2));

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetBookAvailability_ShouldIncludeRelatedData()
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var book = await context.Books.FirstAsync();

            var result = await mediator.Send(new BookAvailabilityQuery(book.Id));

            Assert.NotNull(result);
           
        }

        [Fact]
        public async Task GetReadingRate_ShouldCalculateCorrectly()
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var book = await context.Books.FirstAsync(b => b.Title == "The Great Gatsby");

            var result = await mediator.Send(new BookReadingRateQuery(book.Id));

            Assert.True(result.Average > 0);
        }

        [Fact]
        public async Task GetMostLendingBooks_ShouldReturnCorrectResults()
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.Send(new MostLendingBooksQuery(2));

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            
        }

        [Fact]
        public async Task GetUserBorrowHistory_ShouldReturnCorrectHistory()
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var user = await context.Users.FirstAsync();

            var result = await mediator.Send(new UserLendingBooksQuery(user.Id, DateTime.Now.AddDays(-30), DateTime.Now));

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetRelatedBooks_ShouldReturnBooks()
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
            var book = await context.Books.FirstAsync(b => b.Title == "The Great Gatsby");

            var result = await mediator.Send(new LendingRelatedBooksQuery(book.Id));

            Assert.NotNull(result); 
        }

        public void Dispose()
        {
            if(!_disposed)
            {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
                    context.Database.CloseConnection();
                    context.Database.EnsureDeleted();
                    context.Dispose();
                    _disposed = true;
            }
        }
    }
}
