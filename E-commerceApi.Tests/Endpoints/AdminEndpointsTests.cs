using System.Net;
using System.Net.Http.Json;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.product;

namespace E_commerceApi.Tests.Endpoints;

public class AdminEndpointsTests : BaseEndpointTest
{
    public AdminEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

    private async Task<string> GetTokenAndSeedCategoryAsync()
    {
        var token = await GetAdminTokenAsync();
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
        return token;
    }

    // --- Auth guard tests ---

    [Fact]
    public async Task GetCategories_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/admin/categories");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/admin/products");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- Category endpoints ---

    [Fact]
    public async Task GetCategories_Authenticated_ReturnsOk()
    {
        var token = await GetTokenAndSeedCategoryAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/categories");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_ValidRequest_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/categories")
        {
            Content = JsonContent.Create(new
            {
                Name = $"Cat_{Guid.NewGuid():N}",
                Description = "Test category"
            })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_InvalidRequest_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/categories")
        {
            Content = JsonContent.Create(new { Name = "", Description = "" })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_ExistingId_ReturnsOk()
    {
        var token = await GetTokenAndSeedCategoryAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/categories/1");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetCategoryById_NonExistingId_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/categories/999");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_ExistingId_ReturnsOk()
    {
        var token = await GetTokenAndSeedCategoryAsync();
        var request = new HttpRequestMessage(HttpMethod.Put, "/api/admin/categories/1")
        {
            Content = JsonContent.Create(new { Name = "Updated Electronics", Description = "Updated" })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCategory_ExistingId_ReturnsNoContent()
    {
        var token = await GetAdminTokenAsync();
        Context.categories.Add(new categoryETT { Name = $"ToDelete_{Guid.NewGuid():N}" });
        await Context.SaveChangesAsync();
        var catId = Context.categories.Max(c => c.Id);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/categories/{catId}");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    // --- Product endpoints ---

    [Fact]
    public async Task GetProducts_Authenticated_ReturnsOk()
    {
        var token = await GetTokenAndSeedCategoryAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/products");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_ReturnsCreated()
    {
        var token = await GetTokenAndSeedCategoryAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/products")
        {
            Content = JsonContent.Create(new
            {
                Name = $"Product_{Guid.NewGuid():N}",
                Price = 49.99m,
                Stock = 10,
                CategoryId = 1
            })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_InvalidCategoryId_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/products")
        {
            Content = JsonContent.Create(new
            {
                Name = "Orphan Product",
                Price = 10m,
                Stock = 5,
                CategoryId = 999
            })
        };
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProductById_NonExistingId_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/products/999");
        request.Headers.Authorization = new("Bearer", token);

        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
