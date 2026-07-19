public class CartItemResponse
{
    public int CartItemId { get; set; }
    public int ProdcutId { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal SubTotal { get; set; }
}