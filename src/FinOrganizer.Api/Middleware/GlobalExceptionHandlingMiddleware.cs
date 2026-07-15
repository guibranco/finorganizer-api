using System.Text.Json;
using FinOrganizer.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FinOrganizer.Api.Middleware;

public sealed class GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, "Not Found", ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(problem, options: (JsonSerializerOptions?)null, contentType: "application/problem+json");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        var problem = new ProblemDetails { Status = statusCode, Title = title, Detail = detail };
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(problem, options: (JsonSerializerOptions?)null, contentType: "application/problem+json");
    }
}
