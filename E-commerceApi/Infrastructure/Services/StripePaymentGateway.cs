using E_commerceApi.Application.DTOs.Payment;
using E_commerceApi.Application.Interfaces;
using Stripe.Checkout;

namespace E_commerceApi.Infrastructure.Services;

public class StripePaymentGateway : IPaymentGateway
{
    public async Task<(string sessionId, string checkoutUrl)> CreateCheckoutSessionAsync(
        IEnumerable<CheckoutLineItem> lineItems,
        string successUrl,
        string cancelUrl,
        Dictionary<string, string> metadata)
    {
        var sessionLineItems = lineItems.Select(item => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = "usd",
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = item.ProductName
                },
                UnitAmount = item.UnitAmountInCents,
            },
            Quantity = item.Quantity,
        }).ToList();

        var sessionOptions = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = sessionLineItems,
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = metadata
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions);

        return (session.Id, session.Url!);
    }
}
