using E_commerceApi.Application.DTOs.Queries;
using E_commerceApi.Application.Interfaces;
public static class PublicCatalogEndpoints
{
    public static void MapPublicCatalogEndpoints(this WebApplication app)
    {
        // GET /api/products — catálogo paginado con filtros
        var products = app.MapGroup("/api/products");

        products.MapGet("/", async (
            [AsParameters] ProductQueryParams queryParams,
            IProductService productService) =>
        {
            var result = await productService.GetPublicProductsAsync(queryParams);
            return Results.Ok(result);
        });


        // GET /api/products/{id} — detalle de producto
        products.MapGet("/{id:int}", async (
            int id,
            IProductService productService) =>
        {
            var product = await productService.GetByIdAsync(id);
            if (product == null)
                return Results.NotFound(new { message = $"Product with id {id} not found" });
            return Results.Ok(product);
        });


        // GET /api/categories — listar categorías (público)
        app.MapGet("/api/categories", async (
            ICategoryService categoryService) =>
        {
            var categories = await categoryService.GetAllAsync();
            return Results.Ok(categories);
        });


    }
}