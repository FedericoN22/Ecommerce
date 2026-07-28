using Xunit;
using FluentValidation;

namespace E_commerceApi.Tests.Validators;

public class AuthValidatorsTests
{
    private readonly IValidator<RegisterRequest> _registerValidator = new RegisterRequestValidator();
    private readonly IValidator<LoginRequest> _loginValidator = new LoginRequestValidator();

    // RegisterRequestValidator tests

    [Fact]
    public void RegisterAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };
        var result = _registerValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterAsync_EmptyFullName_ReturnsError()
    {
        var request = new RegisterRequest { FullName = "", Email = "test@test.com", Password = "Password1", ConfirmPassword = "Password1" };
        var result = _registerValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "FullName");
    }

    [Fact]
    public void RegisterAsync_InvalidEmail_ReturnsError()
    {
        var request = new RegisterRequest { FullName = "Test", Email = "invalid-email", Password = "Password1", ConfirmPassword = "Password1" };
        var result = _registerValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterAsync_ShortPassword_ReturnsError()
    {
        var request = new RegisterRequest { FullName = "Test", Email = "test@test.com", Password = "123", ConfirmPassword = "123" };
        var result = _registerValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void RegisterAsync_PasswordsMismatch_ReturnsError()
    {
        var request = new RegisterRequest { FullName = "Test", Email = "test@test.com", Password = "Password1", ConfirmPassword = "DifferentPassword1" };
        var result = _registerValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ConfirmPassword");
    }

    // LoginRequestValidator tests

    [Fact]
    public void LoginAsync_ValidRequest_ReturnsSuccess()
    {
        var request = new LoginRequest { Email = "john@example.com", Password = "Password123" };
        var result = _loginValidator.Validate(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void LoginAsync_EmptyEmail_ReturnsError()
    {
        var request = new LoginRequest { Email = "", Password = "Password123" };
        var result = _loginValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginAsync_InvalidEmail_ReturnsError()
    {
        var request = new LoginRequest { Email = "not-an-email", Password = "Password123" };
        var result = _loginValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void LoginAsync_EmptyPassword_ReturnsError()
    {
        var request = new LoginRequest { Email = "john@example.com", Password = "" };
        var result = _loginValidator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }
}
