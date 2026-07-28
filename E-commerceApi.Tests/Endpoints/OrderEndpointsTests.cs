using System.Net;
using System.Net.Http.Json;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.product;

namespace E_commerceApi.Tests.Endpoints;

public class OrderEndpointsTests : BaseEndpointTest
{
    public OrderEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

    private async Task<(string token, int productId)> GetTokenAndSeedProductAsync()
    {
        var token = await GetTokenAsync();
        if (!Context.categories.Any())
        {
            Context.categories.Add(new categoryETT { Id = 1, Name = "Electronics" });
            Context.products.Add(new productETT
            {
                Id = 1,
                Name = "Laptop",
                Price = 999.99m,
                Stock = 10,
                CategoryId = 1
            });
            await Context.SaveChangesAsync();
        }
        return (token, 1);
    }

    [Fact]
    public async Task Checkout_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/checkout", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_EmptyCart_ReturnsBadRequest()
    {
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/checkout");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Checkout_WithCart_ReturnsOkWithSessionUrl()
    {
        var (token, productId) = await GetTokenAndSeedProductAsync();

        // Add item to cart first
        var addRequest = new HttpRequestMessage(HttpMethod.Post, "/api/cart")
        {
            Content = JsonContent.Create(new { ProductId = productId, Quantity = 1 })
        };
        addRequest.Headers.Authorization = new("Bearer", token);
        var addResponse = await Client.SendAsync(addRequest);
        addResponse.EnsureSuccessStatusCode();

        // Checkout
        var checkoutRequest = new HttpRequestMessage(HttpMethod.Post, "/api/checkout");
        checkoutRequest.Headers.Authorization = new("Bearer", token);
        var response = await Client.SendAsync(checkoutRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.Contains("checkoutUrl", body!.Keys);
    }
}
