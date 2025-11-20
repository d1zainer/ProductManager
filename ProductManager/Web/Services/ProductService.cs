using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models.DTOs;
using Web.Models.Entities;
using Web.Models.Mappers;

namespace Web.Services;

/// <summary>
/// Реализация c EF
/// </summary>
public class ProductService(
    AppDbContext dbContext,
    ILogger<ProductService> logger) : IProductService
{
    
    /// <inheritdoc />
    public async Task<(List<ProductShortDto>, int totalCount)> GetAllAsync(ProductFilter filter, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Fetching products with filters: name={Name}, minPrice={Min}, maxPrice={Max}, page={Page}, pageSize={PageSize}",
            filter.Name, filter.MinPrice, filter.MaxPrice, filter.Page, filter.PageSize);

        
        var query = dbContext.Products.AsQueryable();
        
        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(p => p.Name.Contains(filter.Name));

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);
        
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        query = filter.SortBy?.ToLower() switch
        {
            "price" => filter.Ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            "name" => filter.Ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name) // default
        };
        
        query = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize);
        
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
    public async Task<bool> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Update failed: Product with Id={Id} not found", id);
                return false;
            }
            product.Update(dto.Name, dto.Description, dto.Price, dto.IsActive);
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

    public async Task<bool> UpdateStatusAsync(Guid productId, bool isActive, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await dbContext.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product == null)
            {
                logger.LogWarning("Update failed: Product with Id={Id} not found", productId);
                return false;
            }
            product.UpdateStutus(isActive);
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
}
