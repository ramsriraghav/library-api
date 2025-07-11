using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Persistence.SQL
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<LibraryDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("Default"));
            }).AddScoped(provider =>
                    (ILibraryDbContext)provider.GetRequiredService<LibraryDbContext>())
                .AddScoped(provider => (IUnitOfWork)provider.GetRequiredService<LibraryDbContext>());

            services.AddScoped<ApplicationDbContextInitializer>();

            return services;
        }
    }
}
