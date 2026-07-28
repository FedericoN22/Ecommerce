using E_commerceApi.Application.DTOs.Payment;

namespace E_commerceApi.Application.Interfaces;

public interface IPaymentGateway
{
    Task<(string sessionId, string checkoutUrl)> CreateCheckoutSessionAsync(
        IEnumerable<CheckoutLineItem> lineItems,
        string successUrl,
        string cancelUrl,
        Dictionary<string, string> metadata);
}
