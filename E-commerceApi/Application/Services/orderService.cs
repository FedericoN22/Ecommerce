using E_commerceApi.Application.Interfaces;
using E_commerceApi.Application.Exceptions;
using E_commerceApi.Application.DTOs.Order;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Domain.Entities.order;
using E_commerceApi.Domain.Entities.orderItem;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;

namespace E_commerceApi.Application.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(OrderResponse order, string checkoutUrl)?> CreateCheckoutAsync(
        string userId, string successUrl, string cancelUrl)
    {
        var cart = await _context.carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || cart.Items == null || !cart.Items.Any())
            return null;

        foreach (var item in cart.Items)
        {
            if (item.Product == null)
                return null;

            if (item.Quantity > item.Product.Stock)
            {
                throw new InsufficientStockException(
                    item.Product.Name ?? "Unknown",
                    item.Quantity,
                    item.Product.Stock);
            }
        }

        var totalAmount = cart.Items.Sum(item =>
            item.Product!.Price * item.Quantity);

        var order = new OrderETT
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderETT.OrderStatus.Pending,
            TotalAmount = totalAmount,
            Items = cart.Items.Select(ci => new orderItemETT
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product!.Price
            }).ToList()
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.Items)
            {
                item.Product!.Stock -= item.Quantity;
                _context.products.Update(item.Product);
            }
            await _context.SaveChangesAsync();

            _context.cartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var lineItems = order.Items.Select(orderItem =>
        {
            var product = cart.Items
                .First(ci => ci.ProductId == orderItem.ProductId)
                .Product!;

            return new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Name ?? "Product"
                    },
                    UnitAmount = (long)(orderItem.UnitPrice * 100),
                },
                Quantity = orderItem.Quantity,
            };
        }).ToList();

        var sessionOptions = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = lineItems,
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string>
            {
                { "order_id", order.Id.ToString() }
            }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(sessionOptions);

        order.StripeSessionId = session.Id;
        await _context.SaveChangesAsync();

        var orderResponse = await GetOrderByIdAsync(order.Id);
        return (orderResponse!, session.Url!);
    }

    public async Task<bool> ConfirmPaymentAsync(string stripeSessionId)
    {
        var order = await _context.orders
            .FirstOrDefaultAsync(o => o.StripeSessionId == stripeSessionId);

        if (order == null)
            return false;

        if (order.Status != OrderETT.OrderStatus.Pending)
            return true;

        order.Status = OrderETT.OrderStatus.Processing;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<OrderResponse?> GetOrderByIdAsync(int orderId)
    {
        var order = await _context.orders
            .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return null;

        return MapToOrderResponse(order);
    }

    private static OrderResponse MapToOrderResponse(OrderETT order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId ?? string.Empty,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
            Items = order.Items?.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Subtotal = oi.UnitPrice * oi.Quantity
            }).ToList() ?? new List<OrderItemResponse>()
        };
    }
}
