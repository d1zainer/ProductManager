using Web.Models.DTOs;
using Web.Models.Entities;

namespace Web.Repositories;

/// <summary>
/// Репозиторий для работы с продуктами
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Получить все продукты с фильтром, сортировкой и пагинацией
    /// </summary>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(ProductFilter filter);

    /// <summary>
    /// Получить продукт по Id
    /// </summary>
    Task<Product?> GetByIdAsync(Guid id);

    /// <summary>
    /// Добавить новый продукт
    /// </summary>
    Task<Product?> AddAsync(Product product);

    /// <summary>
    /// Обновить продукт
    /// </summary>
    Task<bool> UpdateAsync(Product product);

    /// <summary>
    /// Удалить продукт
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Выставить товар
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    Task<bool> UpdateStatusAsync(Guid productId, bool isActive);

}