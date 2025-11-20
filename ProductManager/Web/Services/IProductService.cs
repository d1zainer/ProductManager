using Web.Models.DTOs;

namespace Web.Services;

/// <summary>
/// Сервис для операций с продуктами
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Получает список продуктов с опциональной фильтрацией, сортировкой и пагинацией.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Кортеж из списка продуктов и общего числа элементов.</returns>
    Task<(List<ProductShortDto>, int totalCount)> GetAllAsync(
        ProductFilter filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить продукт по айди
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProductFullDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Добавить продукт в БД
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProductFullDto?> AddAsync(ProductCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить продукт в БД
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удалить продукт в БД
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    

    /// <summary>
    /// Обновить статус
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="isActive"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool>UpdateStatusAsync(Guid productId, bool isActive, CancellationToken cancellationToken = default);
}
