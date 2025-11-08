using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models.DTOs;
using Web.Models.Entities;
using Web.Models.Mappers;

namespace Web.Services;

/// <summary>
/// Сервис для операций с продуктами
/// </summary>
public interface IProductService
{
   
    /// <summary>
    /// Получает список продуктов с опциональной фильтрацией, сортировкой и пагинацией.
    /// </summary>
    /// <param name="name">Фильтр по имени продукта (необязательный).</param>
    /// <param name="minPrice">Минимальная цена для фильтрации (необязательная).</param>
    /// <param name="maxPrice">Максимальная цена для фильтрации (необязательная).</param>
    /// <param name="sortBy">Поле для сортировки ("name" или "price").</param>
    /// <param name="ascending">Порядок сортировки: true — по возрастанию, false — по убыванию.</param>
    /// <param name="page">Номер страницы для пагинации (по умолчанию 1).</param>
    /// <param name="pageSize">Размер страницы (по умолчанию 20).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Кортеж из списка продуктов и общего числа элементов.</returns>
    Task<(List<ProductShortDto>, int totalCount)> GetAllAsync(
        string? name = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? sortBy = null,
        bool ascending = true,
        int page = 1,
        int pageSize = 20,
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
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> UpdateAsync(ProductFullDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удалить продукт в БД
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Реализация 
/// </summary>
public class ProductService(
    AppDbContext dbContext,
    ILogger<ProductService> logger) : IProductService
{
    /// <inheritdoc />
    public async Task<(List<ProductShortDto>,  int totalCount)> GetAllAsync(string? name = null, 
        decimal? minPrice = null,
        decimal? maxPrice = null, 
        string? sortBy = null, 
        bool ascending = true,  
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Fetching products with filters: name={Name}, minPrice={Min}, maxPrice={Max}, page={Page}, pageSize={PageSize}",
            name, minPrice, maxPrice, page, pageSize);
        
        var query = dbContext.Products.AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);
        
        query = sortBy?.ToLower() switch
        {
            "price" => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name) // default
        };
        
        query = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        
        var products = await query.ToListAsync(cancellationToken);
        logger.LogInformation("Returning {Count} products", products.Count);

        return (products.Select(ProductMapper.ToShortDto).ToList(), totalCount);
    
    }

    /// <inheritdoc />
    public async Task<ProductFullDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fetching product by Id={Id}", id);
        var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id,  cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product with Id={Id} not found", id);
            return null;
        }

        return ProductMapper.ToFullDto(product);
    }
    /// <inheritdoc />
    public async Task<ProductFullDto?> AddAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(dto);
        try
        {
            await dbContext.Products.AddAsync(product, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Product created with Id={Id}", product.Id);
            return ProductMapper.ToFullDto(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding product");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(ProductFullDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == dto.Id, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Update failed: Product with Id={Id} not found", dto.Id);
                return false;
            }
            product.Update(dto.Name, dto.Description, dto.Price);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Product updated with Id={Id}", product.Id);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating product");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (product == null) return false;
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Product deleted with Id={Id}", id);
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting product");
            return false;
        }
    }
}
