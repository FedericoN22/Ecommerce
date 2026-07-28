using System.Net.Http.Json;
using E_commerceApi.Infrastructure.Data;
using E_commerceApi.Infrastructure.identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace E_commerceApi.Tests.Endpoints;

public abstract class BaseEndpointTest : IClassFixture<TestWebApplicationFactory>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly AppDbContext Context;
    private readonly TestWebApplicationFactory _factory;
    private bool _disposed;

    protected BaseEndpointTest(TestWebApplicationFactory factory)
    {
        _factory = factory;
        Client = factory.Client;
        var scope = factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Context.Dispose();
            _disposed = true;
        }
    }

    protected async Task<string> GetTokenAsync()
    {
        var email = $"test_{Guid.NewGuid():N}@test.com";
        var password = "Test1234!";
        var request = new { FullName = "Test User", Email = email, Password = password, ConfirmPassword = password };
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    protected async Task<string> GetAdminTokenAsync()
    {
        var email = $"admin_{Guid.NewGuid():N}@test.com";
        var password = "Admin1234!";
        var request = new { FullName = "Admin User", Email = email, Password = password, ConfirmPassword = password };

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUsers>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var user = await userManager.FindByEmailAsync(email);
        await userManager.AddToRoleAsync(user!, "Admin");

        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }
}
