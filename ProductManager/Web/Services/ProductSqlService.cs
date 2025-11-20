using Web.Models.DTOs;
using Web.Repositories;
using Web.Models.Entities;
using Web.Models.Mappers;

namespace Web.Services;

/// <summary>
/// Реализация с помощью репозитория
/// </summary>
public class ProductSqlService(IProductRepository productRepository) : IProductService
{
    /// <inheritdoc />
    public async Task<(List<ProductShortDto>, int totalCount)> GetAllAsync(ProductFilter filter, CancellationToken cancellationToken = default)
    {
        var (products, totalCount) = await productRepository.GetAllAsync(filter);
        return (products.Select(ProductMapper.ToShortDto).ToList(), totalCount);
    }

    /// <inheritdoc />
    public async Task<ProductFullDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(id);
        if (product == null) return null;
        return ProductMapper.ToFullDto(product);
    }

    /// <inheritdoc />
    public async Task<ProductFullDto?> AddAsync(ProductCreateDto dto, CancellationToken cancellationToken = default)
    {
        var product = Product.Create(dto);
        var addedProduct = await productRepository.AddAsync(product);
        if (addedProduct == null) return null;
        return ProductMapper.ToFullDto(addedProduct);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(Guid id, ProductUpdateDto dto, CancellationToken cancellationToken = default)
    {
        return await productRepository.UpdateAsync(Product.MapFromDto(id, dto));
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await productRepository.DeleteAsync(id);
    }

    public async Task<bool>  UpdateStatusAsync(Guid productId, bool isActive, CancellationToken cancellationToken = default)
    {
        return await productRepository.UpdateStatusAsync(productId, isActive);
    }
}