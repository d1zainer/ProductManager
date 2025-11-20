using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.DTOs;

namespace Web.Models.Entities;

public class Product
{
    [Key]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    [Column(TypeName = "nvarchar(200)")]
    public string Name { get; private set; } = string.Empty;

    [Column(TypeName = "nvarchar(max)")]
    public string? Description { get; private set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; private set; }
    
    
    /// <summary>
    /// Показывает, выставлен ли товар
    /// </summary>
    [Required]
    public bool IsActive { get; private set; } = false; 

    private Product() { }  

    /// <summary>
    /// Фабричный метод для создания продукта
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public static Product Create(ProductCreateDto dto)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsActive = dto.IsActive 
        };
    }

    public void Update(string name, string? description, decimal price, bool isActive)
    {
        Name = name;
        Description = description;
        Price = price;
        IsActive = isActive;
    }

    public static Product MapFromDto(Guid id, ProductUpdateDto dto)
    {
        return new Product
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsActive = dto.IsActive
        };
    }

    public void UpdateStutus(bool status)
    {
        IsActive = status;
    }
}