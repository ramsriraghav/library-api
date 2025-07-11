using LMS.Persistence.SQL;

namespace LMS.Web.Api
{
    public static class InitializerExtensions
    {
        public static async Task InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

            await initialiser.InitializeAsync();

            await initialiser.SeedAsync();
        }
    }



}
