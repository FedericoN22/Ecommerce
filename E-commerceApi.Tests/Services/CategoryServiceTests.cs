using Xunit;
using Microsoft.Data.Sqlite;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Tests.Helpers;
using E_commerceApi.Application.DTOs.Category.CreateCategory;
using E_commerceApi.Application.DTOs.Category.CategoryUpdate;

namespace E_commerceApi.Tests.Services;

public class CategoryServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        (_context, _connection) = DbContextFactory.Create();
        _service = new CategoryService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        _context.categories.AddRange(
            new categoryETT { Name = "Electronics", Description = "Electronic devices" },
            new categoryETT { Name = "Books", Description = "All books" }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCategory()
    {
        var category = new categoryETT { Name = "Electronics", Description = "Electronic devices" };
        _context.categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(category.Id);

        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedCategory()
    {
        var request = new CreateCategoryRequest { Name = "Electronics", Description = "Electronic devices" };

        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal("Electronic devices", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsUpdatedCategory()
    {
        var category = new categoryETT { Name = "Electronics", Description = "Old description" };
        _context.categories.Add(category);
        await _context.SaveChangesAsync();

        var request = new UpdateCategoryRequest { Name = "Electronics Updated", Description = "New description" };
        var result = await _service.UpdateAsync(category.Id, request);

        Assert.NotNull(result);
        Assert.Equal("Electronics Updated", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsNull()
    {
        var request = new UpdateCategoryRequest { Name = "Test", Description = "Test" };
        var result = await _service.UpdateAsync(999, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        var category = new categoryETT { Name = "Electronics" };
        _context.categories.Add(category);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(category.Id);

        Assert.True(result);
        Assert.Null(await _context.categories.FindAsync(category.Id));
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(999);

        Assert.False(result);
    }
}
