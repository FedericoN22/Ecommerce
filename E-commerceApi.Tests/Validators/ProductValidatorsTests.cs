using Xunit;
using FluentValidation;

namespace E_commerceApi.Tests.Validators;

public class ProductValidatorsTests
{
    private readonly IValidator<CreateProductRequest> _createValidator = new CreateProductRequestValidator();
    private readonly IValidator<UpdateProductRequest> _updateValidator = new UpdateProductRequestValidator();

    // CreateProductRequestValidator tests

    [Fact]
    public void CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new CreateProductRequest
        {
            Name = "Laptop",
            Description = "Gaming laptop",
            Price = 999.99m,
            Stock = 10,
            CategoryId = 1
        };
        var result = _createValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateAsync_EmptyName_ReturnsError()
    {
        var request = new CreateProductRequest { Name = "", Price = 10, Stock = 5, CategoryId = 1 };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateAsync_NameExceedsMaxLength_ReturnsError()
    {
        var request = new CreateProductRequest { Name = new string('A', 101), Price = 10, Stock = 5, CategoryId = 1 };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateAsync_InvalidPrice_ReturnsError()
    {
        var request = new CreateProductRequest { Name = "Laptop", Price = 0, Stock = 5, CategoryId = 1 };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price");
    }

    [Fact]
    public void CreateAsync_NegativeStock_ReturnsError()
    {
        var request = new CreateProductRequest { Name = "Laptop", Price = 100, Stock = -1, CategoryId = 1 };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Stock");
    }

    [Fact]
    public void CreateAsync_InvalidCategoryId_ReturnsError()
    {
        var request = new CreateProductRequest { Name = "Laptop", Price = 100, Stock = 5, CategoryId = 0 };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "CategoryId");
    }

    // UpdateProductRequestValidator tests

    [Fact]
    public void UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new UpdateProductRequest
        {
            Name = "Laptop Pro",
            Description = "Updated laptop",
            Price = 1299.99m,
            Stock = 15,
            CategoryId = 1
        };
        var result = _updateValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateAsync_EmptyName_ReturnsError()
    {
        var request = new UpdateProductRequest { Name = "", Price = 10, Stock = 5, CategoryId = 1 };
        var result = _updateValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }
}
