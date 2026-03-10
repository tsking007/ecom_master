using EcommerceApp.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EcommerceApp.Application;

/// <summary>
/// Registers all Application layer services into the DI container.
/// Called once from Program.cs: services.AddApplication();
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // ── AutoMapper ────────────────────────────────────────────────────────
        // Scans this assembly for all classes that inherit from Profile.
        // This picks up:
        //   - Common/Mappings/MappingProfile.cs
        //   - Features/Auth/Mappings/AuthMappingProfile.cs
        //   - Features/Products/Mappings/ProductMappingProfile.cs
        //   - ... all other feature mapping profiles added in later parts
        services.AddAutoMapper(assembly);

        // ── MediatR ───────────────────────────────────────────────────────────
        // Scans this assembly for all:
        //   - IRequestHandler<TRequest, TResponse> (command + query handlers)
        //   - INotificationHandler<TNotification>  (domain event handlers)
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        // ── Pipeline behaviors ────────────────────────────────────────────────
        // Registered in outermost-first order:
        //
        // Request  →  LoggingBehavior
        //             →  PerformanceBehavior
        //                 →  ValidationBehavior
        //                     →  Handler
        //                 ←  ValidationBehavior
        //             ←  PerformanceBehavior (records elapsed time)
        //         ←  LoggingBehavior (logs success or failure)
        // Response ←

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(PerformanceBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        // ── FluentValidation ──────────────────────────────────────────────────
        // Scans this assembly for all IValidator<T> implementations.
        // includeInternalTypes: true — picks up validators in nested classes
        // and internal validators used as child validators.
        services.AddValidatorsFromAssembly(
            assembly,
            includeInternalTypes: true);

        return services;
    }
}