using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_commerceApi.Application.Exceptions;

namespace E_commerceApi.Middleware;

public class ExceptionMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            KeyNotFoundException ex =>
                (StatusCodes.Status404NotFound, "Not Found", ex.Message),

            ArgumentException ex =>
                (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),

            InvalidOperationException ex =>
                (StatusCodes.Status400BadRequest, "Bad Request", ex.Message),

            InsufficientStockException ex =>
                (StatusCodes.Status409Conflict, "Conflict", ex.Message),

            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "Unauthorized", "You are not authorized to perform this action."),

            DbUpdateException =>
                (StatusCodes.Status409Conflict, "Conflict", "A database constraint was violated."),

            _ =>
                (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://tools.ietf.org/html/rfc9110#section-{statusCode switch
            {
                400 => "15.5.1",
                401 => "15.5.2",
                404 => "15.5.5",
                409 => "15.5.10",
                _ => "15.6.1"
            }}"
        };

        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
