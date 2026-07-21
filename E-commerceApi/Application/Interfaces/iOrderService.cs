using E_commerceApi.Application.DTOs.Order;

namespace E_commerceApi.Application.Interfaces;

public interface IOrderService
{
    Task<(OrderResponse order, string checkoutUrl)?> CreateCheckoutAsync(
        string userId, string successUrl, string cancelUrl);
    Task<bool> ConfirmPaymentAsync(string stripeSessionId);
}