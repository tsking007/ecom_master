using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EcommerceApp.API.Filters;

/// <summary>
/// Global action filter that short-circuits requests with invalid ModelState.
///
/// WHY have this alongside FluentValidation:
///   FluentValidation runs via the MediatR pipeline behavior and validates
///   the MediatR command/query objects. ModelState validation runs earlier
///   in the ASP.NET Core pipeline and handles basic JSON binding errors
///   (e.g., required field missing in JSON, type mismatch like string for int).
///
///   These two layers complement each other:
///     ModelState: catches malformed / unreadable request bodies
///     FluentValidation: catches business rule violations on command objects
///
/// Response format matches the FluentValidation exception handler format
/// (from Part 6 ExceptionHandlingMiddleware) so the client sees consistent errors.
/// </summary>
public class ValidateModelFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(e => e.ErrorMessage)
                    .ToArray());

        context.Result = new BadRequestObjectResult(new
        {
            StatusCode = 400,
            Message = "One or more validation errors occurred.",
            Errors = errors
        });
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No post-execution logic needed
    }
}