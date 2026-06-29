using StockPoint.Data;
using StockPoint.Data.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ===== Inyección de Dependencias - Capa de Datos (Jared) =====
builder.Services.AddScoped<ConexionBD>(sp =>
    new ConexionBD(builder.Configuration.GetConnectionString("StockPointDB")));

builder.Services.AddScoped<IProductoData, ProductoData>();
builder.Services.AddScoped<IClienteData, ClienteData>();
builder.Services.AddScoped<IEmpleadoData, EmpleadoData>();
builder.Services.AddScoped<ICategoriaData, CategoriaData>();
builder.Services.AddScoped<IOrdenData, OrdenData>();

// ===== Inyección de Dependencias - Capa de Negocios (Joel) =====
// builder.Services.AddScoped<IProductoBusiness, ProductoBusiness>();
// builder.Services.AddScoped<IOrdenBusiness, OrdenBusiness>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();