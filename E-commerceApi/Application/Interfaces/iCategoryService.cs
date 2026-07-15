using E_commerceApi.Application.DTOs.Category.CreateCategory;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();

    Task<CategoryResponse> GetByIdAsync(int id);

    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);

    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request);

    Task<bool> DeleteAsync(int id);
}