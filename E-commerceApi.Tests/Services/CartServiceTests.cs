using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.product;
using E_commerceApi.Domain.Entities.cart;
using E_commerceApi.Domain.Entities.cartItem;
using E_commerceApi.Infrastructure.identity;
using E_commerceApi.Tests.Helpers;
using E_commerceApi.Application.DTOs;

namespace E_commerceApi.Tests.Services;

public class CartServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly CartService _service;
    private const string TestUserId = "test-user-id";

    public CartServiceTests()
    {
        (_context, _connection) = DbContextFactory.Create();
        _service = new CartService(_context);
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
    public async Task GetCartAsync_EmptyCart_ReturnsNull()
    {
        var result = await _service.GetCartAsync("non-existent-user");

        Assert.Null(result);
    }

    [Fact]
    public async Task AddToCartAsync_NewProduct_AddsItem()
    {
        var request = new AddToCartRequest { ProductId = 1, Quantity = 2 };

        var result = await _service.AddToCartAsync(TestUserId, request);

        Assert.NotNull(result);
        Assert.Equal(1, result.ProductId);
        Assert.Equal(2, result.Quantity);
    }

    [Fact]
    public async Task AddToCartAsync_ExistingProduct_IncrementsQuantity()
    {
        await _service.AddToCartAsync(TestUserId, new AddToCartRequest { ProductId = 1, Quantity = 1 });
        await _service.AddToCartAsync(TestUserId, new AddToCartRequest { ProductId = 1, Quantity = 2 });

        var cart = await _service.GetCartAsync(TestUserId);
        Assert.NotNull(cart);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == 1);
        Assert.NotNull(item);
        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public async Task UpdateCartItemAsync_ExistingItem_UpdatesQuantity()
    {
        var addedItem = await _service.AddToCartAsync(TestUserId, new AddToCartRequest { ProductId = 1, Quantity = 1 });
        var updateRequest = new UpdateCartItemRequest { Quantity = 5 };

        var result = await _service.UpdateCartItemAsync(TestUserId, addedItem.CartItemId, updateRequest);

        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public async Task UpdateCartItemAsync_NonExistingItem_ReturnsNull()
    {
        var updateRequest = new UpdateCartItemRequest { Quantity = 5 };

        var result = await _service.UpdateCartItemAsync(TestUserId, 999, updateRequest);

        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveFromCartAsync_ExistingItem_ReturnsTrue()
    {
        var addedItem = await _service.AddToCartAsync(TestUserId, new AddToCartRequest { ProductId = 1, Quantity = 1 });

        var result = await _service.RemoveFromCartAsync(TestUserId, addedItem.CartItemId);

        Assert.True(result);
        var cart = await _service.GetCartAsync(TestUserId);
        Assert.NotNull(cart);
        Assert.Empty(cart!.Items);
    }

    [Fact]
    public async Task ClearCartAsync_WithItems_ClearsCart()
    {
        await _service.AddToCartAsync(TestUserId, new AddToCartRequest { ProductId = 1, Quantity = 1 });

        var result = await _service.ClearCartAsync(TestUserId);

        Assert.True(result);
        var cart = await _service.GetCartAsync(TestUserId);
        Assert.NotNull(cart);
        Assert.Empty(cart!.Items);
    }
}
