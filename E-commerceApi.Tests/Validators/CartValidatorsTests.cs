using Xunit;
using FluentValidation;
using E_commerceApi.Application.DTOs;

namespace E_commerceApi.Tests.Validators;

public class CartValidatorsTests
{
    private readonly IValidator<AddToCartRequest> _addToCartValidator = new AddToCartValidator();
    private readonly IValidator<UpdateCartItemRequest> _updateCartItemValidator = new UpdateCartItemRequestValidator();

    // AddToCartValidator tests

    [Fact]
    public void AddToCart_ValidRequest_ReturnsSuccess()
    {
        var request = new AddToCartRequest { ProductId = 1, Quantity = 1 };
        var result = _addToCartValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void AddToCart_InvalidProductId_ReturnsError()
    {
        var request = new AddToCartRequest { ProductId = 0, Quantity = 1 };
        var result = _addToCartValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ProductId");
    }

    [Fact]
    public void AddToCart_InvalidQuantity_ReturnsError()
    {
        var request = new AddToCartRequest { ProductId = 1, Quantity = 0 };
        var result = _addToCartValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
    }

    // UpdateCartItemRequestValidator tests

    [Fact]
    public void UpdateCartItem_ValidRequest_ReturnsSuccess()
    {
        var request = new UpdateCartItemRequest { Quantity = 5 };
        var result = _updateCartItemValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateCartItem_InvalidQuantity_ReturnsError()
    {
        var request = new UpdateCartItemRequest { Quantity = 0 };
        var result = _updateCartItemValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Quantity");
    }
}
