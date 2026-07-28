using Xunit;
using Moq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.product;
using E_commerceApi.Domain.Entities.cart;
using E_commerceApi.Domain.Entities.cartItem;
using E_commerceApi.Domain.Entities.order;
using E_commerceApi.Domain.Entities.orderItem;
using E_commerceApi.Application.Interfaces;
using E_commerceApi.Application.Services;
using E_commerceApi.Application.DTOs.Payment;
using E_commerceApi.Application.Exceptions;
using E_commerceApi.Infrastructure.identity;
using E_commerceApi.Tests.Helpers;

namespace E_commerceApi.Tests.Services;

public class OrderServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly Mock<IPaymentGateway> _paymentGatewayMock;
    private readonly OrderService _service;
    private const string TestUserId = "test-user-id";

    public OrderServiceTests()
    {
        (_context, _connection) = DbContextFactory.Create();
        _paymentGatewayMock = new Mock<IPaymentGateway>();
        _service = new OrderService(_context, _paymentGatewayMock.Object);
        SeedData();
    }

    private void SeedData()
    {
        _context.Users.Add(new ApplicationUsers
        {
            Id = TestUserId,
            UserName = "testuser",
            Email = "test@test.com",
            FullName = "Test User"
        });
        _context.categories.Add(new categoryETT { Id = 1, Name = "Electronics" });
        _context.products.Add(new productETT { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 10, CategoryId = 1 });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ExistingOrder_UpdatesStatusAndStock()
    {
        var order = new OrderETT
        {
            UserId = TestUserId,
            OrderDate = DateTime.UtcNow,
            Status = OrderETT.OrderStatus.Pending,
            TotalAmount = 999.99m,
            StripeSessionId = "session_123",
            Items = new List<orderItemETT>
            {
                new orderItemETT { ProductId = 1, Quantity = 2, UnitPrice = 999.99m }
            }
        };
        _context.orders.Add(order);
        await _context.SaveChangesAsync();

        var result = await _service.ConfirmPaymentAsync("session_123");

        Assert.True(result);
        var updatedOrder = await _context.orders.FindAsync(order.Id);
        Assert.Equal(OrderETT.OrderStatus.Processing, updatedOrder.Status);
        var product = await _context.products.FindAsync(1);
        Assert.Equal(8, product.Stock);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_NonExistingOrder_ReturnsFalse()
    {
        var result = await _service.ConfirmPaymentAsync("non_existent_session");

        Assert.False(result);
    }

    [Fact]
    public async Task CancelExpiredOrdersAsync_WithExpiredOrders_ReturnsCount()
    {
        _context.orders.Add(new OrderETT
        {
            UserId = TestUserId,
            OrderDate = DateTime.UtcNow.AddHours(-2),
            Status = OrderETT.OrderStatus.Pending,
            TotalAmount = 100m,
            StripeSessionId = "session_expired"
        });
        await _context.SaveChangesAsync();

        var result = await _service.CancelExpiredOrdersAsync(TimeSpan.FromHours(1));

        Assert.Equal(1, result);
        var order = await _context.orders.FirstAsync();
        Assert.Equal(OrderETT.OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task CancelExpiredOrdersAsync_NoExpiredOrders_ReturnsZero()
    {
        _context.orders.Add(new OrderETT
        {
            UserId = TestUserId,
            OrderDate = DateTime.UtcNow,
            Status = OrderETT.OrderStatus.Pending,
            TotalAmount = 100m,
            StripeSessionId = "session_recent"
        });
        await _context.SaveChangesAsync();

        var result = await _service.CancelExpiredOrdersAsync(TimeSpan.FromHours(1));

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task CreateCheckoutAsync_EmptyCart_ReturnsNull()
    {
        var result = await _service.CreateCheckoutAsync("empty-user", "https://success.com", "https://cancel.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCheckoutAsync_WithCart_ReturnsCheckoutUrl()
    {
        var cart = new cartETT { UserId = TestUserId };
        _context.carts.Add(cart);
        await _context.SaveChangesAsync();

        _context.cartItems.Add(new cartItemETT
        {
            CartId = cart.Id,
            ProductId = 1,
            Quantity = 1,
            UnitPrice = 999.99m
        });
        await _context.SaveChangesAsync();

        _paymentGatewayMock
            .Setup(g => g.CreateCheckoutSessionAsync(
                It.IsAny<IEnumerable<CheckoutLineItem>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(("session_abc", "https://checkout.stripe.com/session_abc"));

        var result = await _service.CreateCheckoutAsync(TestUserId, "https://success.com", "https://cancel.com");

        Assert.NotNull(result);
        Assert.Equal("https://checkout.stripe.com/session_abc", result.Value.checkoutUrl);
        _paymentGatewayMock.Verify(g => g.CreateCheckoutSessionAsync(
            It.IsAny<IEnumerable<CheckoutLineItem>>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()), Times.Once);
    }

    [Fact]
    public async Task CreateCheckoutAsync_InsufficientStock_ThrowsException()
    {
        var cart = new cartETT { UserId = TestUserId };
        _context.carts.Add(cart);
        await _context.SaveChangesAsync();

        _context.cartItems.Add(new cartItemETT
        {
            CartId = cart.Id,
            ProductId = 1,
            Quantity = 100,
            UnitPrice = 999.99m
        });
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            _service.CreateCheckoutAsync(TestUserId, "https://success.com", "https://cancel.com"));
    }
}
