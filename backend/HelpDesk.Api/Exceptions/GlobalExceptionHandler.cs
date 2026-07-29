using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{

    private readonly ILogger _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, exception.Message);

        var (statusCode, title, errors) = exception switch
        {
            ValidationException ex => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                ex.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage).ToArray())
            ),

            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized.",
                null
            ),

            KeyNotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource not found.",
                null
            ),

            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "Operation is not valid.",
                null
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                null
            )
        };

        httpContext.Response.StatusCode = statusCode;

        if (errors is not null)
        {
            await httpContext.Response.WriteAsJsonAsync(
                new ValidationProblemDetails(errors)
                {
                    Status = statusCode,
                    Title = title
                },
                cancellationToken);

            return true;
        }

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title
            },
            cancellationToken);

        return true;
    }
}