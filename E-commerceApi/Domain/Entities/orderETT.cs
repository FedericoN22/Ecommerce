
using E_commerceApi.Domain.Entities.orderItem;
using E_commerceApi.Infrastructure.identity;

namespace E_commerceApi.Domain.Entities.order
{
    public class OrderETT
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? StripeSessionId { get; set; }
        public enum OrderStatus
        {
            Pending = 0,
            Processing = 1,
            Shipped = 2,
            Delivered = 3,
            Cancelled = 4
        }
        public OrderStatus Status { get; set; }

        public List<orderItemETT> Items { get; set; } = [];
        public ApplicationUsers? User { get; set; }

    }
}