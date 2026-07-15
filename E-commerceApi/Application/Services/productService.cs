using E_commerceApi.Application.Interfaces;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.product;
using Microsoft.EntityFrameworkCore;


public class ProductService : IProductService
{
    private readonly AppDbContext _context;


    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        return await _context.products
        .Include(p => p.Category)
        .Select(p => new ProductResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category!.Name
        })
        .ToListAsync();
    }

    public async Task<ProductResponse> GetByIdAsync(int id)
    {
        var product = await _context.products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return null!;

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name!,
            Description = product.Description!,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category!.Name!
        };
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var categoryExists = await _context.categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new KeyNotFoundException($"Category with id {request.CategoryId} not found");
        var product = new productETT
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId
        };
        _context.products.Add(product);
        await _context.SaveChangesAsync();
        // Recargar con Include para tener el nombre de categoría
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name!,
            Description = product.Description!,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category!.Name!
        };
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return null!;
        var categoryExists = await _context.categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
            throw new KeyNotFoundException($"Category with id {request.CategoryId} not found");
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        await _context.SaveChangesAsync();
        // Recargar categoría si cambió
        if (product.CategoryId != product.Category?.Id)
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name!,
            Description = product.Description!,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            CategoryName = product.Category!.Name!
        };
    }
    // DELETE
    // Retorna false si no existe
    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.products.FindAsync(id);
        if (product == null) return false;
        _context.products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

}