using Web.Models.DTOs;
using Web.Models.Entities;

namespace Web.Data;

public static class DbSeeder
{
    /// <summary>
    /// Заполнить БД тестовыми данными
    /// </summary>
    /// <param name="db"></param>
    /// <param name="isEnabled"></param>
    public static async Task SeedAsync(AppDbContext db, bool isEnabled = true)
    {
        if (isEnabled)
        {
            db.Products.AddRange(
                Product.Create(new ProductCreateDto("Кофе", null, 200)),
                Product.Create(new ProductCreateDto("Чай", null, 100)),
                Product.Create(new ProductCreateDto("Какао", null, 200)),
                Product.Create(new ProductCreateDto("Шоколад", null, 300)),
                Product.Create(new ProductCreateDto("Арабика", null, 250))
            );
        }
        await db.SaveChangesAsync();
    }
}
