using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(typeof(ApplicationExtensions),
            ServiceLifetime.Transient,
            includeInternalTypes: true);

        return services.AddUseCases();
    }
}
