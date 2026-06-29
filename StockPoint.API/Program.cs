using Microsoft.AspNetCore.Diagnostics;
using StockPoint.Business;
using StockPoint.Business.Interfaces;
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
builder.Services.AddScoped<IProductoBusiness, ProductoBusiness>();
builder.Services.AddScoped<IClienteBusiness, ClienteBusiness>();
builder.Services.AddScoped<IEmpleadoBusiness, EmpleadoBusiness>();
builder.Services.AddScoped<IOrdenBusiness, OrdenBusiness>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ===== Red de seguridad: excepciones no contempladas por los controllers =====
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        if (feature?.Error is not null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(feature.Error, "Excepción no controlada: {Mensaje}", feature.Error.Message);
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { mensaje = "Error interno del servidor" });
    });
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();