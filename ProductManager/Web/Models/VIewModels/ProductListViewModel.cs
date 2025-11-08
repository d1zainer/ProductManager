using Web.Models.DTOs;

namespace Web.Models;

/// <summary>
/// VM для таблицы
/// </summary>
public class ProductListViewModel
{
    public IEnumerable<ProductShortDto> Products { get; set; } = Enumerable.Empty<ProductShortDto>();

    public string? SortBy { get; set; }
    public bool Ascending { get; set; }

    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
