using System.Data;
using Dapper;
using Web.Models.DTOs;
using Web.Models.Entities;

namespace Web.Repositories;

/// <summary>
/// Реализация с Dapper
/// </summary>
/// <param name="db"></param>
/// <param name="logger"></param>
public class ProductSqlRepository(IDbConnection db,
    ILogger<ProductSqlRepository> logger) : IProductRepository
{
    /// <inheritdoc />
    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetAllAsync(ProductFilter filter)
    {
        try
        {
            var sql = "SELECT * FROM product WHERE 1=1";
            var parameters = new DynamicParameters();


            if (filter.IsActive.HasValue)
            {
                sql += " AND IsActive = @IsActive"; 
                parameters.Add("IsActive", filter.IsActive);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                sql += " AND Name LIKE @Name";
                parameters.Add("Name", $"%{filter.Name}%");
            }

            if (filter.MinPrice.HasValue)
            {
                sql += " AND Price >= @MinPrice";
                parameters.Add("MinPrice", filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                sql += " AND Price <= @MaxPrice";
                parameters.Add("MaxPrice", filter.MaxPrice.Value);
            }
            
            var orderBy = filter.SortBy?.ToLower() switch
            {
                "price" => $"Price {(filter.Ascending ? "ASC" : "DESC")}",
                "name" => $"Name {(filter.Ascending ? "ASC" : "DESC")}",
                _ => "Name ASC"
            };
            sql += $" ORDER BY {orderBy}";

            var allProducts = (await db.QueryAsync<Product>(sql, parameters)).ToList();

            var totalCount = allProducts.Count;

            var pagedProducts = allProducts
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return (pagedProducts, totalCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products with filters {@Filter}", filter);
            return (Enumerable.Empty<Product>(), 0);
        }

    }


    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            var sql = "SELECT * FROM product WHERE Id = @Id";
            return await db.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching product by Id {Id}", id);
            return null; // Возвращаем null
        }
    }

    /// <inheritdoc />
    public async Task<Product?> AddAsync(Product product)
    {
        try
        {
            var sql = @"INSERT INTO product (Id, Name, Description, Price)
                        VALUES (@Id, @Name, @Description, @Price)";
            await db.ExecuteAsync(sql, product);
            logger.LogInformation("Added product {Id}", product.Id);
            return product;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding product {@Product}", product);
            return null!;
        }
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Product product)
    {
        try
        {
            var sql = @"UPDATE product
                        SET Name = @Name,
                            Description = @Description,
                            Price = @Price
                        WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, product);
            logger.LogInformation("Updated product {Id}, affected rows: {Affected}", product.Id, affected);
            return affected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product {@Product}", product);
            return false;
        }
    }


    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var sql = "DELETE FROM product WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = id });
            logger.LogInformation("Deleted product {Id}, affected rows: {Affected}", id, affected);
            return affected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid productId, bool isActive)
    {
        try
        {
            var sql = @"UPDATE product
SET IsActive = @IsActive
WHERE Id = @Id";
            var affected = await db.ExecuteAsync(sql, new { Id = productId, IsActive = isActive });
            logger.LogInformation("Updated IsActive for product {Id} to {IsActive}, affected rows: {Affected}", productId, isActive, affected);
            return affected > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating IsActive for product {Id}", productId);
            return false;
        }
    }

}
