using E_commerceApi.Application.DTOs;
using E_commerceApi.Application.Interfaces;
using System.Security.Claims;
public static class CartEndpoints
{
    public static void MapCartEndpoints(this WebApplication app)
    {
        var cart = app.MapGroup("/api/cart")
            .RequireAuthorization();



        // GET /api/cart — Obtener carrito del usuario
        cart.MapGet("/", async (
            ICartService cartService,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await cartService.GetCartAsync(userId);
            return result == null
                ? Results.Ok(new CartResponse { Items = [], Total = 0 })
                : Results.Ok(result);
        });



        // POST /api/cart — Agregar producto al carrito
        cart.MapPost("/", async (
            AddToCartRequest request,
            ICartService cartService,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var item = await cartService.AddToCartAsync(userId, request);
            Results.Created($"/api/cart", item);
        });



        // PUT /api/cart/{cartItemId} — Actualizar cantidad
        cart.MapPut("/{cartItemId:int}", async (
            int cartItemId,
            UpdateCartItemRequest request,
            ICartService cartService,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var item = await cartService.UpdateCartItemAsync(userId, cartItemId, request);
            return item == null
                ? Results.NotFound(new { message = "Cart item not found" })
                : Results.Ok(item);
        });



        // DELETE /api/cart/{cartItemId} — Quitar un producto
        cart.MapDelete("/{cartItemId:int}", async (
            int cartItemId,
            ICartService cartService,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var removed = await cartService.RemoveFromCartAsync(userId, cartItemId);
            return removed
                ? Results.NoContent()
                : Results.NotFound(new { message = "Cart item not found" });
        });



        // DELETE /api/cart — Vaciar carrito
        cart.MapDelete("/", async (
            ICartService cartService,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var cleared = await cartService.ClearCartAsync(userId);
            return cleared
                ? Results.NoContent()
                : Results.NoContent(); // 204 aunque esté vacío
        });
    }
}