namespace E_commerceApi.Application.DTOs.Payment;

public class CheckoutLineItem
{
    public string ProductName { get; set; } = string.Empty;
    public long UnitAmountInCents { get; set; }
    public int Quantity { get; set; }
}
