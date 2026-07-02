using Microsoft.AspNetCore.Diagnostics;
using StockPoint.Business;
using StockPoint.Business.Interfaces;
using StockPoint.Data;
using StockPoint.Data.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddScoped<ConexionBD>(sp =>
    new ConexionBD(builder.Configuration.GetConnectionString("StockPointDB")));

builder.Services.AddScoped<IProductoData, ProductoData>();
builder.Services.AddScoped<IClienteData, ClienteData>();
builder.Services.AddScoped<IEmpleadoData, EmpleadoData>();
builder.Services.AddScoped<ICategoriaData, CategoriaData>();
builder.Services.AddScoped<IOrdenData, OrdenData>();

builder.Services.AddScoped<IProductoBusiness, ProductoBusiness>();
builder.Services.AddScoped<IClienteBusiness, ClienteBusiness>();
builder.Services.AddScoped<IEmpleadoBusiness, EmpleadoBusiness>();
builder.Services.AddScoped<IOrdenBusiness, OrdenBusiness>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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