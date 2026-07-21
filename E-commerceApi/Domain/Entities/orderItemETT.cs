namespace E_commerceApi.Domain.Entities.orderItem
{
    public class orderItemETT
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public E_commerceApi.Domain.Entities.product.productETT? Product { get; set; }
    }
}