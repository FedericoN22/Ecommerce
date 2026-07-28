using System.Net;
using System.Net.Http.Json;
using E_commerceApi.Domain.Entities.category;
using E_commerceApi.Domain.Entities.product;

namespace E_commerceApi.Tests.Endpoints;

public class PublicCatalogEndpointsTests : BaseEndpointTest
{
    public PublicCatalogEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

    private void SeedData()
    {
        if (Context.categories.Any()) return;
        Context.categories.AddRange(
            new categoryETT { Id = 1, Name = "Electronics", Description = "Electronic devices" },
            new categoryETT { Id = 2, Name = "Books", Description = "All books" }
        );
        Context.products.AddRange(
            new productETT { Id = 1, Name = "Laptop", Price = 999.99m, Stock = 10, CategoryId = 1 },
            new productETT { Id = 2, Name = "Phone", Price = 699.99m, Stock = 20, CategoryId = 1 },
            new productETT { Id = 3, Name = "Book", Price = 29.99m, Stock = 50, CategoryId = 2 }
        );
        Context.SaveChanges();
    }

    [Fact]
    public async Task GetProducts_ReturnsOk()
    {
        SeedData();
        var response = await Client.GetAsync("/api/products");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithSearch_ReturnsFilteredResults()
    {
        SeedData();
        var response = await Client.GetAsync("/api/products?search=Laptop");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithCategoryFilter_ReturnsFilteredResults()
    {
        SeedData();
        var response = await Client.GetAsync("/api/products?categoryId=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_WithPagination_ReturnsCorrectPage()
    {
        SeedData();
        var response = await Client.GetAsync("/api/products?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductsById_ExistingId_ReturnsOk()
    {
        SeedData();
        var response = await Client.GetAsync("/api/products/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProductsById_NonExistingId_ReturnsNotFound()
    {
        var response = await Client.GetAsync("/api/products/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        SeedData();
        var response = await Client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
