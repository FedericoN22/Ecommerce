using E_commerceApi.Application.DTOs.Category.CreateCategory;
// using E_commerceApi.Application.Interfaces;
// using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this WebApplication app)
    {
        // =====================================================
        // CATEGORIES
        // =====================================================
        var categories = app.MapGroup("/api/admin/categories")
            .RequireAuthorization(p => p.RequireRole("Admin"));



        // GET /api/admin/categories
        // Retorna 200 con la lista de categorías
        categories.MapGet("/", async (ICategoryService categoryService) =>
        {
            var categories = await categoryService.GetAllAsync();
            return Results.Ok(categories);
        });




        // GET /api/admin/categories/{id}
        // Retorna 200 si existe, 404 si no
        categories.MapGet("/{id:int}", async (int id, ICategoryService categoryService) =>
        {
            var category = await categoryService.GetByIdAsync(id);
            if (category == null)
                return Results.NotFound(new { message = $"Category with id {id} not found" });
            return Results.Ok(category);
        });




        // POST /api/admin/categories
        // Retorna 201 con la categoría creada
        // Retorna 400 si el body es inválido
        categories.MapPost("/", async (CreateCategoryRequest request, ICategoryService categoryService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name is required" });
            var created = await categoryService.CreateAsync(request);
            return Results.Created($"/api/admin/categories/{created.Id}", created);
        });



        // PUT /api/admin/categories/{id}
        // Retorna 200 con la categoría actualizada, 404 si no existe
        // Retorna 400 si el body es inválido
        categories.MapPut("/{id:int}", async (
            int id,
            UpdateCategoryRequest request,
            ICategoryService categoryService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name is required" });
            var updated = await categoryService.UpdateAsync(id, request);
            if (updated == null)
                return Results.NotFound(new { message = $"Category with id {id} not found" });
            return Results.Ok(updated);
        });



        // DELETE /api/admin/categories/{id}
        // Retorna 204 si eliminó, 404 si no existe
        // Si tiene productos asociados, retorna 400 por FK constraint
        categories.MapDelete("/{id:int}", async (int id, ICategoryService categoryService) =>
        {
            try
            {
                var deleted = await categoryService.DeleteAsync(id);
                if (!deleted)
                    return Results.NotFound(new { message = $"Category with id {id} not found" });
                return Results.NoContent();
            }
            catch (DbUpdateException)
            {
                return Results.BadRequest(new { message = "Cannot delete category because it has associated products" });
            }
        });



        // =====================================================
        // PRODUCTS
        // =====================================================
        var products = app.MapGroup("/api/admin/products")
            .RequireAuthorization(p => p.RequireRole("Admin"));




        // GET /api/admin/products
        // Retorna 200 con la lista de productos
        products.MapGet("/", async (IProductService productService) =>
        {
            var products = await productService.GetAllAsync();
            return Results.Ok(products);
        });



        // GET /api/admin/products/{id}
        // Retorna 200 si existe, 404 si no
        products.MapGet("/{id:int}", async (int id, IProductService productService) =>
        {
            var product = await productService.GetByIdAsync(id);
            if (product == null)
                return Results.NotFound(new { message = $"Product with id {id} not found" });
            return Results.Ok(product);
        });



        // POST /api/admin/products
        // Retorna 201 con el producto creado
        // Retorna 400 si el body es inválido
        // Retorna 400 si la categoría no existe (KeyNotFoundException)
        products.MapPost("/", async (CreateProductRequest request, IProductService productService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name is required" });
            if (request.Price <= 0)
                return Results.BadRequest(new { message = "Price must be greater than 0" });
            if (request.Stock < 0)
                return Results.BadRequest(new { message = "Stock cannot be negative" });
            try
            {
                var created = await productService.CreateAsync(request);
                return Results.Created($"/api/admin/products/{created.Id}", created);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });



        // PUT /api/admin/products/{id}
        // Retorna 200 con el producto actualizado, 404 si no existe
        // Retorna 400 si el body es inválido o la categoría no existe
        products.MapPut("/{id:int}", async (
            int id,
            UpdateProductRequest request,
            IProductService productService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { message = "Name is required" });
            if (request.Price <= 0)
                return Results.BadRequest(new { message = "Price must be greater than 0" });
            if (request.Stock < 0)
                return Results.BadRequest(new { message = "Stock cannot be negative" });
            try
            {
                var updated = await productService.UpdateAsync(id, request);
                if (updated == null)
                    return Results.NotFound(new { message = $"Product with id {id} not found" });
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });



        // DELETE /api/admin/products/{id}
        // Retorna 204 si eliminó, 404 si no existe
        products.MapDelete("/{id:int}", async (int id, IProductService productService) =>
        {
            var deleted = await productService.DeleteAsync(id);
            if (!deleted)
                return Results.NotFound(new { message = $"Product with id {id} not found" });
            return Results.NoContent();
        });
    }
}