using Microsoft.Data.Sqlite;
using PizzaPlace;
using PizzaPlace.Data;
using PizzaPlace.Factories;
using PizzaPlace.Repositories;
using PizzaPlace.Services;
using Microsoft.EntityFrameworkCore;
using PizzaPlace.Consoles;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o =>
{
    o.AddPolicy("allowAll",
        policy =>
        {
            policy.WithOrigins(
            // "whatEver.homepage.com"
            );
            policy.AllowAnyHeader();
            policy.AllowCredentials();
            policy.AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddOpenApiDocument(d =>
{
    d.Title = "Pizza Place";
    d.Version = "v1";
});

builder.Services.AddDbContext<PizzaContext>();
using (var context = new PizzaContext())
{
    context.Database.EnsureCreated();
}

// Register services:
var services = builder.Services;
services.AddSingleton(TimeProvider.System);

services.AddTransient<IPizzaContext, PizzaContext>();
services.AddTransient<IStockRepository, StockRepository>();
services.AddTransient<IRecipeRepository, RecipeRepository>();

services.AddTransient<IPizzaOven, NormalPizzaOven>();
services.AddTransient<IPizzaOven, AssemblyLinePizzaOven>();
services.AddTransient<IPizzaOven, GiantRevolvingPizzaOven>();

services.AddTransient<IStockService, StockService>();
services.AddTransient<IRecipeService, RecipeService>();
services.AddTransient<IOrderingService, OrderingService>();
services.AddTransient<IMenuService, MenuService>();

services.AddTransient<IConsole, StandardConsole>();

var app = builder.Build();

app.UseOpenApi();
app.UseSwaggerUi();

app.UseCors();

app.MapControllers();

app.Run();
