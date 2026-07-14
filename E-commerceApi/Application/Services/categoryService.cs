using E_commerceApi.Application.Interfaces;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.category;
using Microsoft.EntityFrameworkCore;




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

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _context.categories.FindAsync(id);
        if (category == null) return null;

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name!,
            Description = category.Description!
        };
    }

}