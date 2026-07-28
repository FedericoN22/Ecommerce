using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace E_commerceApi.Tests.Endpoints;

public class AuthEndpointsTests : BaseEndpointTest
{
    public AuthEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        var request = new
        {
            FullName = "John Doe",
            Email = $"john_{Guid.NewGuid():N}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidModel_ReturnsBadRequest()
    {
        var request = new
        {
            FullName = "",
            Email = "invalid-email",
            Password = "123",
            ConfirmPassword = "456"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_EmptyFields_ReturnsBadRequest()
    {
        var request = new
        {
            FullName = "",
            Email = "",
            Password = "",
            ConfirmPassword = ""
        };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        var registerRequest = new
        {
            FullName = "Login User",
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };
        await Client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new { Email = email, Password = "Password123!" };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.Equal(email, authResponse.Email);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsInternalServerError()
    {
        var request = new
        {
            Email = $"nonexistent_{Guid.NewGuid():N}@example.com",
            Password = "WrongPassword"
        };

        var response = await Client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Token_CanBeValidatedManually()
    {
        var email = $"debug_{Guid.NewGuid():N}@test.com";
        var registerRequest = new
        {
            FullName = "Debug User",
            Email = email,
            Password = "Debug123!",
            ConfirmPassword = "Debug123!"
        };
        var regResponse = await Client.PostAsJsonAsync("/api/auth/register", registerRequest);
        regResponse.EnsureSuccessStatusCode();
        var authResponse = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("UnaClaveSuperSecretaDeMasDe32Caracteres");
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "ECommerceApi",
            ValidAudience = "ECommerceClient",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };

        var principal = tokenHandler.ValidateToken(authResponse.Token, validationParameters, out var validatedToken);
        Assert.NotNull(principal);

        // Check if the JWT settings in configuration match
        var tokenStr = authResponse.Token;
        var jwtToken = tokenHandler.ReadJwtToken(tokenStr);
        Assert.Equal("ECommerceApi", jwtToken.Issuer);
        Assert.Equal("ECommerceClient", jwtToken.Audiences.First());

        // Try via DefaultRequestHeaders
        Client.DefaultRequestHeaders.Authorization = new("Bearer", tokenStr);
        var cartResponse = await Client.GetAsync("/api/cart");
        Client.DefaultRequestHeaders.Authorization = null;

        var body = await cartResponse.Content.ReadAsStringAsync();
        Assert.True(cartResponse.StatusCode == HttpStatusCode.OK, $"Expected OK but got {cartResponse.StatusCode}. Body: {body}");
    }
}
