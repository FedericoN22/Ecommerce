using E_commerceApi.Application.Interfaces;
using E_commerceApi.Application.Exceptions;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        var checkout = app.MapGroup("/api/checkout")
            .RequireAuthorization();

        checkout.MapPost("/", async (
            IOrderService orderService,
            IConfiguration configuration,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var successUrl = configuration["Stripe:SuccessUrl"]!;
            var cancelUrl = configuration["Stripe:CancelUrl"]!;

            try
            {
                var result = await orderService.CreateCheckoutAsync(
                    userId, successUrl, cancelUrl);

                if (result == null)
                    return Results.BadRequest(
                        new { message = "Cart is empty or not found" });

                return Results.Ok(new
                {
                    sessionId = result.Value.order.Id,
                    checkoutUrl = result.Value.checkoutUrl
                });
            }
            catch (InsufficientStockException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        var webhooks = app.MapGroup("/api/webhooks");

        webhooks.MapPost("/stripe", async (
            HttpContext httpContext,
            IConfiguration configuration,
            IOrderService orderService) =>
        {
            var json = await new StreamReader(httpContext.Request.Body)
                .ReadToEndAsync();

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    httpContext.Request.Headers["Stripe-Signature"],
                    configuration["Stripe:WebhookSecret"]);
            }
            catch (StripeException)
            {
                return Results.BadRequest(new { message = "Invalid signature" });
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session?.Id != null)
                {
                    await orderService.ConfirmPaymentAsync(session.Id);
                }
            }

            return Results.Ok();
        });
    }
}
