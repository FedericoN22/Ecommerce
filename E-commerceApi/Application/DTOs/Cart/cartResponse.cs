public class CartResponse
{
    public int CartId { get; set; }
    public string? UserId { get; set; }
    public List<CartItemResponse>? Items { get; set; }

    public decimal Total { get; set; }
}