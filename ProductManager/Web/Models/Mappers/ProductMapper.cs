using Web.Models.DTOs;
using Web.Models.Entities;

namespace Web.Models.Mappers;

public static class ProductMapper
{
    public static ProductShortDto ToShortDto(Product product) =>
        new ProductShortDto(
            product.Id,
            product.Name,
            product.Price,
            product.IsActive
        );

    public static ProductFullDto ToFullDto(Product product) =>
        new ProductFullDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.IsActive
        );

}