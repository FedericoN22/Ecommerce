using E_commerceApi.Application.DTOs.Common;
using E_commerceApi.Application.DTOs.Queries;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetAllAsync();

    Task<ProductResponse> GetByIdAsync(int id);

    Task<ProductResponse> CreateAsync(CreateProductRequest request);

    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);

    Task<bool> DeleteAsync(int id);

    Task<PagedResult<ProductResponse>> GetPublicProductsAsync(ProductQueryParams queryParams);

}