using E_commerceApi.Application.Interfaces;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.category;
using Microsoft.EntityFrameworkCore;
using E_commerceApi.Application.DTOs.Category.CreateCategory;
// using System.Security.Cryptography.X509Certificates;




public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        return await _context.categories
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync();
    }

    public async Task<CategoryResponse> GetByIdAsync(int id)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null) return null!;

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name!,
            Description = category.Description!
        };
    }

    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request
    )
    {
        var category = new categoryETT
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name!,
            Description = category.Description
        };
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null) return null!;

        category.Name = request.Name;
        category.Description = request.Description;

        await _context.SaveChangesAsync();

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null) return false;

        _context.categories.Remove(category);
        await _context.SaveChangesAsync();

        return true;

    }
}