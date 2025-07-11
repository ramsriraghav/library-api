using FluentValidation;
using LMS.Domain.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LMS.Application.Queries
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetBookAvailabilityValidator).Assembly));
            services.AddValidatorsFromAssembly(typeof(GetBookAvailabilityValidator).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }

    }
}
