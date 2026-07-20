using E_commerceApi.Domain.Entities.cart;
using E_commerceApi.Domain.Entities.product;

namespace E_commerceApi.Domain.Entities.cartItem
{
    public class cartItemETT
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public cartETT? Cart { get; set; }
        public productETT? Product { get; set; }
    }
}