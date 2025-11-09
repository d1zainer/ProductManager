using Web.Models.DTOs;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Tests;

public class ProductsApiTests(CustomWebApplicationFactory  factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetAllProducts_ShouldReturnList()
    {
        var response = await _client.GetAsync("/api/products");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<ProductListDto>();
        products.Should().NotBeNull();
        products!.Products.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnProduct()
    {
        var allProductsResponse= await _client.GetFromJsonAsync<ProductListDto>("/api/products");
        var productId = allProductsResponse!.Products.First().Id;

        var response = await _client.GetAsync($"/api/products/{productId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductFullDto>();
        product!.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetProducts_WithPriceFilter_ShouldReturnFiltered()
    {
        var response = await _client.GetAsync("/api/products?minPrice=100&maxPrice=200");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<ProductListDto>();
        products!.Products.Should().OnlyContain(p => p.Price >= 100 && p.Price <= 200);
    }
    
    
    [Fact]
    public async Task GetProducts_WithNameFilter_ShouldReturnFiltered()
    {
        var response = await _client.GetAsync("/api/products?name=кофе");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<ProductListDto>();
        products!.Products.Should().OnlyContain(p => p.Name.Contains("кофе", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetProducts_WithPagination_ShouldReturnCorrectPage()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=2");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<ProductListDto>();
        products!.Products.Should().HaveCountLessThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetProducts_WithSorting_ShouldReturnSorted()
    {
        var response = await _client.GetAsync("/api/products?sortBy=price&ascending=false");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<ProductListDto>();
        products!.Products.Should().BeInDescendingOrder(p => p.Price);
    }
    
    
    
    [Fact]
    public async Task AddUpdateDeleteProduct_Api_ShouldWorkCorrectly()
    {
        // --- Создание продукта через API ---
        var newProduct = new ProductCreateDto("Test Product", "Test Desc", 999);
        var postResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdProduct = await postResponse.Content.ReadFromJsonAsync<ProductFullDto>();
        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be("Test Product");

        // Обновление продукта через API
        var updateDto = new ProductFullDto(createdProduct.Id, "Updated Product", "Updated Desc", 888);
        var putResponse = await _client.PutAsJsonAsync($"/api/products/{createdProduct.Id}", updateDto);
        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedProduct = await _client.GetFromJsonAsync<ProductFullDto>($"/api/products/{createdProduct.Id}");
        updatedProduct!.Name.Should().Be("Updated Product");
        updatedProduct.Price.Should().Be(888);

        // Удаление продукта через API
        var deleteResponse = await _client.DeleteAsync($"/api/products/{createdProduct.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        //  Проверка, что продукта больше нет
        var afterDelete = await _client.GetFromJsonAsync<ProductListDto>("/api/products");
        afterDelete!.Products.Should().NotContain(p => p.Id == createdProduct.Id);
    }
    
    
    [Fact]
    public async Task CreateProduct_ShouldReturnValidationErrors_ForInvalidData()
    {
        // Пустое имя
        var invalidProduct1 = new ProductCreateDto("", "", 100);
        var response1 = await _client.PostAsJsonAsync("/api/products", invalidProduct1);
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Отрицательная цена
        var invalidProduct2 = new ProductCreateDto("Name", "Desc", -50);
        var response2 = await _client.PostAsJsonAsync("/api/products", invalidProduct2);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Пустое имя и цена 0
        var invalidProduct3 = new ProductCreateDto("", null, 0);
        var response3 = await _client.PostAsJsonAsync("/api/products", invalidProduct3);
        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    
    [Fact]
    public async Task UpdateProduct_ShouldReturnValidationErrors_ForInvalidData()
    {
        // Предположим, что у нас есть тестовый продукт с Id
        var allProducts = await _client.GetFromJsonAsync<ProductListDto>("/api/products");
        var productId = allProducts!.Products.First().Id;

        // Пустое имя
        var invalidUpdate1 = new ProductUpdateDto("", "Some Desc", 100);
        var response1 = await _client.PutAsJsonAsync($"/api/products/{productId}", invalidUpdate1);
        response1.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Отрицательная цена
        var invalidUpdate2 = new ProductUpdateDto("Valid Name", "Desc", -50);
        var response2 = await _client.PutAsJsonAsync($"/api/products/{productId}", invalidUpdate2);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Пустое имя и цена 0
        var invalidUpdate3 = new ProductUpdateDto("", null, 0);
        var response3 = await _client.PutAsJsonAsync($"/api/products/{productId}", invalidUpdate3);
        response3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

}
