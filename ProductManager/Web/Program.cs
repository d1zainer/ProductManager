using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;
using Web.Repositories;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

var useDapper = builder.Configuration.GetValue<bool>("UseDapper");

builder.Services.AddControllersWithViews();
if (builder.Environment.IsEnvironment("Test"))
{
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));
        builder.Services.AddScoped<IProductService, ProductService>();
}
else
{
    if (!useDapper)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging();
        });
        builder.Services.AddScoped<IProductService, ProductService>();
    }
    else
    {

        builder.Services.AddScoped<IDbConnection>(sp =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString); 
        });
        
        builder.Services.AddScoped<IProductRepository, ProductSqlRepository>();
        builder.Services.AddScoped<IProductService, ProductSqlService>();
    }
}

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.Configure<AdminCredentials>(
    builder.Configuration.GetSection("AdminCredentials"));

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
    if (!useDapper)
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
                    await DbSeeder.SeedAsync(db, false); //если false, то без сидирования
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
    else
    {
        if (!builder.Environment.IsEnvironment("Test"))
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            var builderConn = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            const int maxRetries = 10;
            var delay = TimeSpan.FromSeconds(2);
            
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using var masterConn = new SqlConnection(builderConn.ConnectionString);
                    await masterConn.OpenAsync();
                    
                    var createDbSql = @"
        IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ProductsDb')
        BEGIN
            CREATE DATABASE ProductsDb;
        END";
                    await masterConn.ExecuteAsync(createDbSql);
                    break;
                }
                catch (SqlException)
                {
                    Console.WriteLine("SQL Server ещё не готов, жду 2 сек...");
                    await Task.Delay(delay);
                }

            }
            
            builderConn.InitialCatalog = "ProductsDb";

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using var conn = new SqlConnection(builderConn.ConnectionString);
                    await conn.OpenAsync();
                    
                    // Создаём таблицу, если её нет
                    var createTableSql = @"
        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='product' AND xtype='U')
        BEGIN
            CREATE TABLE product (
                Id UNIQUEIDENTIFIER PRIMARY KEY,
                Name NVARCHAR(255) NOT NULL,
                Description NVARCHAR(MAX),
                Price DECIMAL(18,2) NOT NULL,
                IsActive BIT NOT NULL DEFAULT 0
            );
        END";
                    await conn.ExecuteAsync(createTableSql);
                    await DbSeeder.SeedAsync(conn, true);
                    break; 
                }
                catch (SqlException)
                {
                    Console.WriteLine("ProductsDb ещё не готова, жду 2 сек...");
                    await Task.Delay(delay);
                }

            }


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