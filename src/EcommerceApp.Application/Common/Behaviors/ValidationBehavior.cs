using FluentValidation;
using MediatR;

namespace EcommerceApp.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs all FluentValidation validators
/// registered for the incoming request type before the handler executes.
///
/// Execution order: LoggingBehavior → PerformanceBehavior → ValidationBehavior → Handler
///
/// If any validator produces failures, a ValidationException is thrown
/// immediately and the handler never runs. All validators run concurrently
/// so every field's errors are collected in one pass.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // No validators registered for this request type — skip
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        // Run all validators concurrently for best performance
        var validationResults = await Task.WhenAll(
            _validators.Select(v =>
                v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new Exceptions.ValidationException(failures);

        return await next();
    }
}