using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using E_commerceApi.Middleware;
using E_commerceApi.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace E_commerceApi.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionMiddleware>> _loggerMock = new();
    private readonly ExceptionMiddleware _middleware;

    public ExceptionMiddlewareTests()
    {
        _middleware = new ExceptionMiddleware(_loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_Returns404()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = _ => throw new KeyNotFoundException("Product not found");

        await _middleware.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_Returns400()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = _ => throw new ArgumentException("Invalid argument");

        await _middleware.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InvalidOperationException_Returns400()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = _ => throw new InvalidOperationException("Invalid operation");

        await _middleware.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InsufficientStockException_Returns409()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = _ => throw new InsufficientStockException("Laptop", 5, 2);

        await _middleware.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_Returns401()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = _ => throw new UnauthorizedAccessException();

        await _middleware.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DbUpdateException_Returns409()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = _ => throw new DbUpdateException("DB error", new Exception());

        await _middleware.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_Returns500()
    {
        var context = new DefaultHttpContext();
        RequestDelegate next = _ => throw new Exception("Unexpected error");

        await _middleware.InvokeAsync(context, next);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        var context = new DefaultHttpContext();
        bool nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        await _middleware.InvokeAsync(context, next);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }
}
