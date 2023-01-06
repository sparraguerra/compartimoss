using Demo.Products.Api.Services;
using Demo.Products.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Demo.Products.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApiContext>(options => options.UseInMemoryDatabase("api"));
builder.Services.AddScoped<IProductsService, ProductsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/products", async (IProductsService productService) =>
       await productService.GetProducts())
.Produces<IEnumerable<ProductResponse>>(StatusCodes.Status200OK)
.WithName("GetProducts");

app.MapGet("/api/products/{id}", async (int id, IProductsService productService) =>
{
    var customer = await productService.GetProductById(id);
    if (customer is null) return Results.NotFound();
    return Results.Ok(customer);
})
.Produces<ProductResponse>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
.WithName("GetProductById");

app.MapPost("/api/products", async (ProductRequest request, IProductsService productService)
    => await productService.CreateProduct(request))
.Produces<ProductResponse>(StatusCodes.Status200OK)
.WithName("CreateProduct");

app.MapPut("/api/products/{id}", async (int id, ProductRequest request, IProductsService productService) =>
{
    var customer = await productService.UpdateProduct(id, request);
    if (customer is null) return Results.NotFound();
    return Results.Ok(customer);
})
.Produces<ProductResponse>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
.WithName("UpdateProduct");

app.MapDelete("/api/products/{id}", async (int id, IProductsService productService) =>
{
    await productService.DeleteProduct(id);
    return Results.Ok();
})
.Produces<ProductResponse>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
.WithName("DeleteProduct");

app.Run();
