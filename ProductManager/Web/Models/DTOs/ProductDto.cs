using System.ComponentModel.DataAnnotations;
using Web.Models.Entities;

namespace Web.Models.DTOs;

/// <summary>
/// Продукт без описания
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Price"></param>
public record ProductShortDto(Guid Id, string Name, decimal Price);
    
/// <summary>
/// Продукт
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="Price"></param>
public record ProductFullDto(Guid Id, string Name, string? Description, decimal Price);
    
/// <summary>
/// Создать продукт
/// </summary>
/// <param name="Name"></param>
/// <param name="Description"></param>
/// <param name="Price"></param>
public class ProductCreateDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, 999_999_999_999_999.99, ErrorMessage = "Price must be between 0 and 999,999,999,999,999.99")]
    [Display(Name = "Price", Prompt = "0 - 999,999,999,999,999.99")]
    public decimal Price { get; set; }
    
    public ProductCreateDto() { }

    public ProductCreateDto(string name, string? description, decimal price)
    {
        Name = name;
        Description = description;
        Price = price;
    }
}


/// <summary>
/// DTO для списка продуктов с общим количеством элементов
/// </summary>
/// <param name="Products">Список краткой информации о продуктах</param>
/// <param name="TotalCount">Общее количество продуктов (без учёта пагинации)</param>
public record ProductListDto(List<ProductShortDto> Products, int TotalCount);


/// <summary>
/// DTO для обновления продукта
/// </summary>
public class ProductUpdateDto(string name, string? description, decimal price)
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; init; } = name;

    public string? Description { get; init; } = description;

    [Range(0, 999_999_999_999_999.99, ErrorMessage = "Price must be between 0 and 999,999,999,999,999.99")]
    [Display(Name = "Price", Prompt = "0 - 999,999,999,999,999.99")]
    public decimal Price { get; init; } = price;
    
}