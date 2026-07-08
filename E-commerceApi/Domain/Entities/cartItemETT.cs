namespace E_commerceApi.Domain.Entities.cartItem
{
    public class cartItemETT
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}