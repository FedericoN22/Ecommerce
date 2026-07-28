using System.Net;
using System.Net.Http.Json;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.product;

namespace E_commerceApi.Tests.Endpoints;

public class CartEndpointsTests : BaseEndpointTest
{
    public CartEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

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

    // --- Auth guard tests ---

    [Fact]
    public async Task GetCart_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/cart");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithoutAuth_ReturnsUnauthorized()
    {
        var request = new { ProductId = 0, Quantity = 0 };
        var response = await Client.PostAsJsonAsync("/api/cart", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- Authenticated cart tests ---

    [Fact]
    public async Task GetCart_Authenticated_ReturnsOkWithEmptyCart()
    {
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/cart");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_ValidRequest_ReturnsCreated()
    {
        var (token, productId) = await GetTokenAndSeedProductAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/cart")
        {
            Content = JsonContent.Create(new { ProductId = productId, Quantity = 2 })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_InvalidProductId_ReturnsBadRequest()
    {
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/cart")
        {
            Content = JsonContent.Create(new { ProductId = 0, Quantity = 1 })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItem_ExistingItem_ReturnsOk()
    {
        var (token, productId) = await GetTokenAndSeedProductAsync();

        // Add item first
        var addRequest = new HttpRequestMessage(HttpMethod.Post, "/api/cart")
        {
            Content = JsonContent.Create(new { ProductId = productId, Quantity = 1 })
        };
        addRequest.Headers.Authorization = new("Bearer", token);
        var addResponse = await Client.SendAsync(addRequest);
        var cartItem = await addResponse.Content.ReadFromJsonAsync<CartItemResponse>();

        // Update quantity
        var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/cart/{cartItem!.CartItemId}")
        {
            Content = JsonContent.Create(new { Quantity = 5 })
        };
        updateRequest.Headers.Authorization = new("Bearer", token);
        var response = await Client.SendAsync(updateRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItem_NonExistingItem_ReturnsNotFound()
    {
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/cart/999")
        {
            Content = JsonContent.Create(new { Quantity = 3 })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveFromCart_ExistingItem_ReturnsNoContent()
    {
        var (token, productId) = await GetTokenAndSeedProductAsync();

        var addRequest = new HttpRequestMessage(HttpMethod.Post, "/api/cart")
        {
            Content = JsonContent.Create(new { ProductId = productId, Quantity = 1 })
        };
        addRequest.Headers.Authorization = new("Bearer", token);
        var addResponse = await Client.SendAsync(addRequest);
        var cartItem = await addResponse.Content.ReadFromJsonAsync<CartItemResponse>();

        var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/cart/{cartItem!.CartItemId}");
        deleteRequest.Headers.Authorization = new("Bearer", token);
        var response = await Client.SendAsync(deleteRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ClearCart_Authenticated_ReturnsNoContent()
    {
        var token = await GetTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/cart");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
