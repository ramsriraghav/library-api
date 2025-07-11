Library Management Web API
The Library Management Web API is a modern, scalable API built with .NET 8, designed to manage library operations such as tracking books, users, and borrowing activities. It supports both REST and gRPC endpoints for high-performance communication, uses MediatR for command/query handling, FluentValidation for input validation, and SQLite as the database for development and testing. The project includes comprehensive functional and integration tests to ensure reliability.
Features

REST and gRPC Endpoints: Expose library operations via REST (HTTP/1.1) and gRPC (HTTP/2) for flexibility and performance.
Core Functionality:
GetMostBorrowedBooks: Retrieves the top N most borrowed books.
GetBookAvailability: Returns a book's total, borrowed, and available copies.
GetReadingRate: Calculates the average pages per day for a book based on returned borrows.
GetTopBorrowers: Lists the top N users by borrow count.
GetUserBorrowHistory: Returns a user’s borrowing history.
GetRelatedBooks: Finds books by the same author as a given book.


MediatR: Implements the CQRS pattern for clean separation of concerns.
FluentValidation: Validates input for all queries to ensure data integrity.
SQLite Database: Uses an in-memory or file-based SQLite database for development and testing.
Functional and Integration Tests: Includes comprehensive tests to verify business logic and API behavior.
Data Seeding: Provides a SeedData method to populate the database with sample data for testing and development.
Dependency Injection: Leverages ASP.NET Core’s DI for services like LibraryDbContext and IMediator.
Logging: Configured with Microsoft.Extensions.Logging for debugging and monitoring.

Technologies

.NET 9.0: Latest .NET framework for performance and modern features.
ASP.NET Core: Web framework for REST and gRPC APIs.
Entity Framework Core: ORM for database operations with SQLite.
MediatR: Implements CQRS for command and query handling.
FluentValidation: Validates query inputs.
gRPC: High-performance RPC framework using HTTP/2 and Protocol Buffers.
xUnit: Testing framework for functional and integration tests.
SQL: SQL database for development and testing.

Project Structure
Library.Solution/
├── LMS.Domain/                    # Domain entities (Book, User, BorrowRecord)
├── LMS.Persistence.SQL/           # EF Core context, SeedData
├── Library.Application.Queries/   # MediatR queries, handlers, validators
├── Library.WebApi/                # REST and gRPC APIs, controllers, services
├── Library.Tests/                 # Functional and integration tests

Prerequisites

.NET 9.0 SDK: Install from https://dotnet.microsoft.com/download.
Visual Studio 2022 or VS Code: For development and debugging.
Postman: For testing REST and gRPC endpoints.
Node.js (optional): For gRPC-Web client development.
Docker (optional): For running tests with a SQL Server container.

Setup Instructions
1. Clone the Repository
git clone https://github.com/your-repo/library-management-api.git
cd LibraryManagementSystem

2. Restore Dependencies
Navigate to the solution directory and restore NuGet packages:
dotnet restore

3. Configure the Database
The project uses SQL with migrations enabled, once application configured with a proper connection string and run.It creates database automatically


Run migrations to create the database (if not using EnsureCreated):dotnet ef migrations add InitialCreate --project Library.Persistence.SQL


4. Run the Web API
cd Library.WebApi
dotnet run


REST Endpoints: Available at https://localhost:5000 
gRPC Endpoints: Available at https://localhost:5001 (e.g., LibraryService.GetMostBorrowedBooks).

5. Trust HTTPS Certificate
For gRPC and HTTPS, trust the development certificate:
dotnet dev-certs https --trust

Usage
REST API
Use tools like Postman or cURL to call REST endpoints. Example:
curl -X GET "https://localhost:5000/api/books/most-borrowed?topN=2" -H "accept: application/json"

Expected response:
[
  { "title": "The Great Gatsby", "borrowCount": 2 },
  { "title": "1984", "borrowCount": 2 }
]

gRPC API
.NET Client

Create a console project:dotnet new console -o Library.GrpcClient


Update Library.GrpcClient.csproj:<ItemGroup>
  <PackageReference Include="Grpc.Net.Client" Version="2.66.0" />
  <PackageReference Include="Grpc.Tools" Version="2.66.0" />
  <Protobuf Include="..\Library.WebApi\Protos\library.proto" GrpcServices="Client" />
</ItemGroup>


Add Program.cs:using Grpc.Net.Client;
using Library.WebApi.Protos;

var channel = GrpcChannel.ForAddress("https://localhost:5001");
var client = new LibraryService.LibraryServiceClient(channel);

var request = new MostBorrowedBooksRequest { TopN = 2 };
var response = await client.GetMostBorrowedBooksAsync(request);

foreach (var book in response.Books)
{
    Console.WriteLine($"Title: {book.Title}, BorrowCount: {book.BorrowCount}");
}


Run the client:dotnet run --project Library.GrpcClient

Test cases:
GetMostBorrowedBooks: Verifies top 2 books by borrow count.
GetBookAvailability: Checks book copies for "The Great Gatsby".
GetReadingRate: Validates pages/day calculation.
GetTopBorrowers: Ensures top 2 users are returned.
GetUserBorrowHistory: Confirms user borrowing history.
GetRelatedBooks: Verifies related books by author.
VerifySeededData: Checks seed data integrity.



Notes

Tests use an in-memory and SQL database, seeded with SeedData.
gRPC tests require the Web API running at https://localhost:5001.

Seeding the Database
The SeedData method in Library.Infrastructure/LibraryDbContext.cs populates the database with sample data for development and testing:

Books: 3 books with titles, authors, total copies, and pages.
Users: 3 users with full names.
BorrowRecords: 6 records (3 active, 3 returned) linking books and users.

To seed the database:

Manually:using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
LibraryDbContext.SeedData(context);


During Tests: Automatically called in IntegrationTests.cs constructor.
Via Endpoint: Add a seeding endpoint in Library.WebApi:app.MapPost("/api/seed", async (ILibraryDbContext context) =>
{
    LibraryDbContext.SeedData((LibraryDbContext)context);
    return Results.Ok("Database seeded");
});

Call with:curl -X POST "https://localhost:5000/api/seed"



Troubleshooting

ObjectDisposedException: Ensure fresh IServiceScope instances are used in tests and services to avoid disposing LibraryDbContext prematurely.
UserBookLendings Error: Verify BorrowRecord maps to BorrowRecords in LibraryDbContext.OnModelCreating.
ILoggerFactory Issues: Confirm AddLogging and ILoggerFactory are registered in Program.cs and IntegrationTests.cs.
gRPC Errors:
Ensure HTTPS is enabled and the certificate is trusted.
Use Grpc.AspNetCore.Web for browser clients.
Check appsettings.json for correct Kestrel endpoints (Http2 for gRPC, Http1 for REST).



Deployment

Development: Run locally with dotnet run.
Production:
Use HTTPS with a valid certificate.
Deploy to Azure App Service (Windows-based for HTTP/2).

