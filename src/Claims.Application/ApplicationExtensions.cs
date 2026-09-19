using Claims.Application.UseCases.Claims.Create;
using Claims.Application.UseCases.Claims.Delete;
using Claims.Application.UseCases.Claims.Get;
using Claims.Application.UseCases.Claims.GetById;
using Claims.Application.UseCases.Covers.ComputePremium;
using Claims.Application.UseCases.Covers.Create;
using Claims.Application.UseCases.Covers.Delete;
using Claims.Application.UseCases.Covers.Get;
using Claims.Application.UseCases.Covers.GetById;
using Claims.Domain.Cover;
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

        services.AddSingleton<IPremiumCalculator, PremiumCalculator>();

        services.AddUseCases();

        return services;
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddClaimsUseCases();
        services.AddCoversUseCases();

        return services;
    }

    public static IServiceCollection AddClaimsUseCases(this IServiceCollection services)
    {
        services.AddScoped<IGetClaimsUseCase, GetClaimsUseCase>();
        services.AddScoped<IGetClaimUseCase, GetClaimUseCase>();
        services.AddScoped<ICreateClaimUseCase, CreateClaimUseCase>();
        services.AddScoped<IDeleteClaimUseCase, DeleteClaimUseCase>();
        return services;
    }

    public static IServiceCollection AddCoversUseCases(this IServiceCollection services)
    {
        services.AddScoped<IGetCoversUseCase, GetCoversUseCase>();
        services.AddScoped<IGetCoverUseCase, GetCoverUseCase>();
        services.AddScoped<ICreateCoverUseCase, CreateCoverUseCase>();
        services.AddScoped<IComputePremiumUseCase, ComputePremiumUseCase>();
        services.AddScoped<IDeleteCoverUseCase, DeleteCoverUseCase>();
        return services;
    }
}
