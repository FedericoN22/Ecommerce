using Xunit;
using Microsoft.Data.Sqlite;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.product;
using E_commerceApi.Tests.Helpers;
using E_commerceApi.Application.DTOs.Queries;

namespace E_commerceApi.Tests.Services;

public class ProductServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        (_context, _connection) = DbContextFactory.Create();
        _service = new ProductService(_context);
        SeedData();
    }

    private void SeedData()
    {
        _context.categories.Add(new categoryETT { Id = 1, Name = "Electronics", Description = "Electronic devices" });
        _context.categories.Add(new categoryETT { Id = 2, Name = "Books", Description = "All books" });
        _context.products.AddRange(
            new productETT { Name = "Laptop", Description = "Gaming laptop", Price = 999.99m, Stock = 10, CategoryId = 1 },
            new productETT { Name = "Phone", Description = "Smartphone", Price = 699.99m, Stock = 20, CategoryId = 1 },
            new productETT { Name = "Book", Description = "Programming book", Price = 29.99m, Stock = 50, CategoryId = 2 }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsProductsWithCategory()
    {
        var result = await _service.GetAllAsync();

        Assert.Equal(3, result.Count());
        Assert.All(result, p => Assert.NotNull(p.CategoryName));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal("Electronics", result.CategoryName);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedProduct()
    {
        var request = new CreateProductRequest
        {
            Name = "Tablet",
            Description = "Android tablet",
            Price = 299.99m,
            Stock = 15,
            CategoryId = 1
        };

        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Tablet", result.Name);
        Assert.Equal("Electronics", result.CategoryName);
    }

    [Fact]
    public async Task CreateAsync_InvalidCategoryId_ThrowsKeyNotFoundException()
    {
        var request = new CreateProductRequest
        {
            Name = "Tablet",
            Price = 299.99m,
            Stock = 15,
            CategoryId = 999
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsUpdatedProduct()
    {
        var request = new UpdateProductRequest
        {
            Name = "Laptop Pro",
            Description = "Updated laptop",
            Price = 1299.99m,
            Stock = 15,
            CategoryId = 1
        };

        var result = await _service.UpdateAsync(1, request);

        Assert.NotNull(result);
        Assert.Equal("Laptop Pro", result.Name);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        var result = await _service.DeleteAsync(1);

        Assert.True(result);
        Assert.Null(await _context.products.FindAsync(1));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task GetPublicProductsAsync_WithSearch_FiltersByName()
    {
        var queryParams = new ProductQueryParams { Search = "Laptop" };

        var result = await _service.GetPublicProductsAsync(queryParams);

        Assert.Single(result.Items);
        Assert.Contains("Laptop", result.Items.First().Name);
    }

    [Fact]
    public async Task GetPublicProductsAsync_WithCategory_FiltersByCategory()
    {
        var queryParams = new ProductQueryParams { CategoryId = 2 };

        var result = await _service.GetPublicProductsAsync(queryParams);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items.First().CategoryId);
    }

    [Fact]
    public async Task GetPublicProductsAsync_Pagination_ReturnsCorrectPage()
    {
        var queryParams = new ProductQueryParams { Page = 1, PageSize = 2 };

        var result = await _service.GetPublicProductsAsync(queryParams);

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
    }
}
