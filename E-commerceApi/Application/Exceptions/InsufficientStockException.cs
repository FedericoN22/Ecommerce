namespace E_commerceApi.Application.Exceptions;

public class InsufficientStockException : Exception
{
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }

    public InsufficientStockException(
        string productName, int requestedQuantity, int availableStock)
        : base($"Insufficient stock for '{productName}': " +
               $"requested {requestedQuantity}, available {availableStock}.")
    {
        ProductName = productName;
        RequestedQuantity = requestedQuantity;
        AvailableStock = availableStock;
    }
}
