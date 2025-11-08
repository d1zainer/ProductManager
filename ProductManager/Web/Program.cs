using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        options.EnableSensitiveDataLogging();
    });
    
}

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddDistributedMemoryCache(); // кэш для сессий
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // время жизни сессии
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<AdminCredentials>(
    builder.Configuration.GetSection("AdminCredentials"));

builder.Services.AddScoped<IProductService, ProductService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";   
        options.AccessDeniedPath = "/Account/AccessDenied"; 
    });

builder.Services.AddAuthorization();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!builder.Environment.IsEnvironment("Test"))
    {
        try
        {
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Console.WriteLine("Применяем миграции...");
                await db.Database.MigrateAsync(); 
                await DbSeeder.SeedAsync(db, true);
            }
            else
            {
                Console.WriteLine("Все миграции уже применены.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при миграции: {ex.Message}");
        }
        

    }
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API V1");
    c.RoutePrefix = "swagger";
});


app.MapGet("/swagger", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();      
app.UseAuthentication();    
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

public partial class Program { }