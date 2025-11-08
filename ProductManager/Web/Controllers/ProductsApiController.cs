using Microsoft.AspNetCore.Mvc;
using Web.Models.DTOs;
using Web.Models.Mappers;
using Web.Services;

namespace Web.Controllers;

/// <summary>
/// Контроллер для работы с продуктами через JSON API.
/// </summary>
[Route("api/products")]
[ApiController]
public class ProductsApiController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Получить все продукты с пагинацией + сортировка
    /// </summary>
    /// <param name="name"></param>
    /// <param name="minPrice"></param>
    /// <param name="maxPrice"></param>
    /// <param name="sortBy"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="ascending"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] string? name,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy, 
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var products = await productService.GetAllAsync(name, 
            minPrice,
            maxPrice,  
            sortBy, 
            ascending,
            page,
            pageSize,
            cancellationToken);
        return Ok(new ProductListDto(products.Item1,  products.Item2));
    }

    /// <summary>
    /// Получить продукт по Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id,  CancellationToken cancellationToken = default)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (product == null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    /// Создать продукт
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto dto,  CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var product = await productService.AddAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Удалить продукт
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id,  CancellationToken cancellationToken = default)
    {
       var deleteResult = await productService.DeleteAsync(id, cancellationToken);
       if(deleteResult) return NoContent();
       return NotFound();
    }
    
    /// <summary>
    /// Обновить продукт
    /// </summary>
    /// <param name="id">Id продукта</param>
    /// <param name="dto">Данные для обновления</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductUpdateDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) 
            return BadRequest(ModelState);
        
        var updateResult = await productService.UpdateAsync(new ProductFullDto(id, dto.Name, dto.Description, dto.Price), cancellationToken);
        if (!updateResult)
            return NotFound();

        return Ok(dto);
    }

}