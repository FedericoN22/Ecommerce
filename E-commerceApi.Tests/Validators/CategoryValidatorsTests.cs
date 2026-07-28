using Xunit;
using FluentValidation;
using E_commerceApi.Application.DTOs.Category.CreateCategory;
using E_commerceApi.Application.DTOs.Category.CategoryUpdate;

namespace E_commerceApi.Tests.Validators;

public class CategoryValidatorsTests
{
    private readonly IValidator<CreateCategoryRequest> _createValidator = new CreateCategoryRequestValidator();
    private readonly IValidator<UpdateCategoryRequest> _updateValidator = new UpdateCategoryRequestValidator();

    // CreateCategoryRequestValidator tests

    [Fact]
    public void CreateAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new CreateCategoryRequest { Name = "Electronics", Description = "Electronic devices" };
        var result = _createValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateAsync_EmptyName_ReturnsError()
    {
        var request = new CreateCategoryRequest { Name = "" };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateAsync_NullName_ReturnsError()
    {
        var request = new CreateCategoryRequest { Name = null };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateAsync_NameExceedsMaxLength_ReturnsError()
    {
        var request = new CreateCategoryRequest { Name = new string('A', 101) };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void CreateAsync_DescriptionExceedsMaxLength_ReturnsError()
    {
        var request = new CreateCategoryRequest { Name = "Test", Description = new string('A', 501) };
        var result = _createValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    // UpdateCategoryRequestValidator tests

    [Fact]
    public void UpdateAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new UpdateCategoryRequest { Name = "Electronics", Description = "Updated description" };
        var result = _updateValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateAsync_EmptyName_ReturnsError()
    {
        var request = new UpdateCategoryRequest { Name = "" };
        var result = _updateValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }
}
