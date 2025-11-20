using System.ComponentModel.DataAnnotations;

namespace Web.Models.DTOs
{
    /// <summary>
    /// Продукт без описания
    /// </summary>
    public record ProductShortDto(
    Guid Id,
    string Name,
    decimal Price,
    bool IsActive
    );

    /// <summary>
    /// Полный продукт
    /// </summary>
    public record ProductFullDto(
        Guid Id, 
        string Name, 
        string? Description, 
        decimal Price, 
        bool IsActive
    );

    /// <summary>
    /// DTO для создания продукта
    /// </summary>
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 999_999_999_999_999.99, ErrorMessage = "Price must be between 0 and 999,999,999,999,999.99")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = false;

        public ProductCreateDto() { }
        public ProductCreateDto(string name, string? description, decimal price, bool isActive = false)
        {
            Name = name;
            Description = description;
            Price = price;
            IsActive = isActive;
        }
    }

    /// <summary>
    /// DTO для обновления продукта
    /// </summary>
    public class ProductUpdateDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; init; }

        public string? Description { get; init; }

        [Range(0, 999_999_999_999_999.99, ErrorMessage = "Price must be between 0 and 999,999,999,999,999.99")]
        public decimal Price { get; init; }

        public bool IsActive { get; init; } = false;
        
        public ProductUpdateDto() { }

        public ProductUpdateDto(string name, string? description, decimal price, bool isActive)
        {
            Name = name;
            Description = description;
            Price = price;
            IsActive = isActive;
        }
    }

    /// <summary>
    /// DTO для смены статуса продукта
    /// </summary>
    public class ProductStatusUpdateDto
    {
        [Required(ErrorMessage = "IsActive is required")]
        public bool IsActive { get; set; } = false;
    }

    /// <summary>
    /// DTO для списка продуктов с общим количеством элементов
    /// </summary>
    public record ProductListDto(
        List<ProductShortDto> Products,
        int TotalCount
    );

    /// <summary>
    /// DTO для фильтрации продуктов
    /// </summary>
    public class ProductFilter
    {
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public bool Ascending { get; set; } = true;
        
        public bool? IsActive { get; set; } 
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
