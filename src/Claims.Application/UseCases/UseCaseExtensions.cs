using Claims.Application.UseCases.Claims.Create;
using Claims.Application.UseCases.Claims.Delete;
using Claims.Application.UseCases.Claims.Get;
using Claims.Application.UseCases.Claims.GetById;
using Claims.Application.UseCases.Covers.ComputePremium;
using Claims.Application.UseCases.Covers.Create;
using Claims.Application.UseCases.Covers.Delete;
using Claims.Application.UseCases.Covers.Get;
using Claims.Application.UseCases.Covers.GetById;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Application;

public static class UseCaseExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IGetClaimsUseCase, GetClaimsUseCase>();
        services.AddScoped<IGetClaimUseCase, GetClaimUseCase>();
        services.AddScoped<ICreateClaimUseCase, CreateClaimUseCase>();
        services.AddScoped<IDeleteClaimUseCase, DeleteClaimUseCase>();

        services.AddScoped<IGetCoversUseCase, GetCoversUseCase>();
        services.AddScoped<IGetCoverUseCase, GetCoverUseCase>();
        services.AddScoped<ICreateCoverUseCase, CreateCoverUseCase>();
        services.AddScoped<IComputePremiumUseCase, ComputePremiumUseCase>();
        services.AddScoped<IDeleteCoverUseCase, DeleteCoverUseCase>();

        return services;
    }
}
