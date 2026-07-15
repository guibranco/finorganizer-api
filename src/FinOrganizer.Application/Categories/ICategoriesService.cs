namespace FinOrganizer.Application.Categories;

public interface ICategoriesService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
