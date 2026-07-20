// using E_commerceApi.Application.DTOs;

public interface ICartService
{
    Task<CartResponse?> GetCartAsync(string userId);
    Task<CartItemResponse?> AddToCartAsync(string userId, AddToCartRequest request);
    Task<CartItemResponse?> UpdateCartItemAsync(string userId, int cartItemId, UpdateCartItemRequest request);
    Task<bool> RemoveFromCartAsync(string userId, int cartItemId);
    Task<bool> ClearCartAsync(string userId);



}