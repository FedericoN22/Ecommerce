using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.cart;
using E_commerceApi.Domain.Entities.cartItem;
// using E_commerceApi.Application.DTOs;
using Microsoft.EntityFrameworkCore;



public class CartService : ICartService
{
    private readonly AppDbContext _context;
    public CartService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<CartResponse?> GetCartAsync(string userId)
    {
        var cart = await _context.carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null) return null;
        var items = (cart.Items ?? new List<cartItemETT>())
            .Select(ci => new CartItemResponse
            {
                CartItemId = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? string.Empty,
                UnitPrice = ci.UnitPrice,
                Quantity = ci.Quantity,
                Subtotal = ci.UnitPrice * ci.Quantity
            }).ToList();
        return new CartResponse
        {
            CartId = cart.Id,
            UserId = cart.UserId ?? string.Empty,
            Items = items,
            Total = items.Sum(i => i.Subtotal)
        };
    }
    public async Task<CartItemResponse?> AddToCartAsync(string userId, AddToCartRequest request)
    {
        var cart = await _context.carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new cartETT { UserId = userId };
            _context.carts.Add(cart);
            await _context.SaveChangesAsync();
        }
        var existingItem = cart.Items?
            .FirstOrDefault(ci => ci.ProductId == request.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UnitPrice = await GetProductPriceAsync(request.ProductId);
            await _context.SaveChangesAsync();
            // Cargar producto para el response
            await _context.Entry(existingItem).Reference(ci => ci.Product).LoadAsync();
            return MapToCartItemResponse(existingItem);
        }
        var productPrice = await GetProductPriceAsync(request.ProductId);
        var cartItem = new cartItemETT
        {
            CartId = cart.Id,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = productPrice
        };
        _context.cartItems.Add(cartItem);
        await _context.SaveChangesAsync();
        await _context.Entry(cartItem).Reference(ci => ci.Product).LoadAsync();
        return MapToCartItemResponse(cartItem);
    }
    public async Task<CartItemResponse?> UpdateCartItemAsync(
        string userId, int cartItemId, UpdateCartItemRequest request)
    {
        var cartItem = await _context.cartItems
            .Include(ci => ci.Cart)
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId
                                     && ci.Cart!.UserId == userId);
        if (cartItem == null) return null;
        cartItem.Quantity = request.Quantity;
        await _context.SaveChangesAsync();
        return MapToCartItemResponse(cartItem); // Product ya viene incluido
    }
    public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId)
    {
        var cartItem = await _context.cartItems
            .Include(ci => ci.Cart)       // Solo necesito Cart para verificar userId
            .FirstOrDefaultAsync(ci => ci.Id == cartItemId
                                     && ci.Cart!.UserId == userId);
        if (cartItem == null) return false;
        _context.cartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> ClearCartAsync(string userId)
    {
        var cart = await _context.carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart?.Items == null || !cart.Items.Any())
            return false;
        _context.cartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync();
        return true;
    }
    private async Task<decimal> GetProductPriceAsync(int productId)
    {
        var product = await _context.products.FindAsync(productId);

        return product?.Price ?? 0;
    }
    // ÚNICO mapper privado — sin queries extra
    private static CartItemResponse MapToCartItemResponse(cartItemETT item)
    {
        return new CartItemResponse
        {
            CartItemId = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name ?? string.Empty,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            Subtotal = item.UnitPrice * item.Quantity
        };
    }
}