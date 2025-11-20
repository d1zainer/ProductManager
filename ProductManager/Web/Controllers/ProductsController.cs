using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Models.DTOs;
using Web.Services;

namespace Web.Controllers;

/// <summary>
/// Контроллер, доступный только для авторизованных пользователей
/// </summary>
/// <param name="productService"></param>
[Authorize]
public class ProductsController(IProductService productService) : Controller
{

    /// Страница отображения списка продуктов
    /// <param name="sortBy"></param>
    /// <param name="ascending"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<IActionResult> Index(
        string? sortBy, 
        bool ascending = true, 
        int page = 1, 
        int pageSize = 10)
    {
        var cancellationToken = HttpContext.RequestAborted;

        var (products, totalCount) = await productService.GetAllAsync( new ProductFilter()
        {
            Ascending = ascending,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
        },cancellationToken);
        
        var vm = new ProductListViewModel
        {
            Products = products,
            SortBy = sortBy,
            Ascending = ascending,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return View(vm);
    }
    
    /// <summary>
    /// Страница создания продукта
    /// </summary>
    /// <returns></returns>
    public IActionResult Create() => View(new ProductCreateDto(string.Empty, string.Empty,0 ));
    
    /// <summary>
    /// Кнопка создания продукта
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var cancellationToken = HttpContext.RequestAborted;
        await productService.AddAsync(dto, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Страница редактирования
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Edit(Guid id)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var dto = await productService.GetByIdAsync(id, cancellationToken);
        if (dto == null) return NotFound();
        return View(dto);
    }
    
    /// <summary>
    /// Кнопка редактирования
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFullDto dto)
    {
        var updateDto = new ProductUpdateDto(dto.Name, dto.Description, dto.Price,  dto.IsActive);
        if(!TryValidateModel(updateDto)) return View(dto);
        var cancellationToken =  HttpContext.RequestAborted;
        await productService.UpdateAsync(dto.Id, updateDto, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Кнопка удаления продукта
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var cancellationToken = HttpContext.RequestAborted;
        await productService.DeleteAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
    
    
    /// <summary>
    /// Смена статуса продукта (выставить на продажу / снять с продажи)
    /// </summary>
    /// <param name="id">Id продукта</param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var cancellationToken = HttpContext.RequestAborted;
        var product = await productService.GetByIdAsync(id, cancellationToken);
        if (product == null) return NotFound();
        bool newStatus = !product.IsActive;
        await productService.UpdateStatusAsync(id, newStatus, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
