using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using E_commerceApi.Infrastructure.identity;

namespace E_commerceApi.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUsers>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var userStore = new Mock<IUserStore<ApplicationUsers>>();
        _userManagerMock = new Mock<UserManager<ApplicationUsers>>(
            userStore.Object, null, null, null, null, null, null, null, null);

        var roleStore = new Mock<IRoleStore<IdentityRole>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            roleStore.Object, null, null, null, null);

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("UnaClaveSuperSecretaDeMasDe32Caracteres");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configurationMock.Setup(c => c["Jwt:DurationInMinutes"]).Returns("60");

        _service = new AuthService(_userManagerMock.Object, _roleManagerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsToken()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync((ApplicationUsers)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUsers>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUsers>(), "User"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUsers>()))
            .ReturnsAsync(new List<string>());

        var result = await _service.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(request.Email, result.Email);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsException()
    {
        var request = new RegisterRequest
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123"
        };

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync(new ApplicationUsers { Email = request.Email });

        await Assert.ThrowsAsync<Exception>(() => _service.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var request = new LoginRequest { Email = "john@example.com", Password = "Password123" };
        var user = new ApplicationUsers { Email = request.Email, UserName = request.Email };

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, request.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var result = await _service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal(request.Email, result.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsException()
    {
        var request = new LoginRequest { Email = "john@example.com", Password = "WrongPassword" };

        _userManagerMock.Setup(m => m.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUsers)null);

        await Assert.ThrowsAsync<Exception>(() => _service.LoginAsync(request));
    }
}
