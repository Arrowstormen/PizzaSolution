using Microsoft.EntityFrameworkCore;
using PizzaPlace.Models.Entities;

namespace PizzaPlace.Data
{
    public interface IPizzaContext
    {
        public DbSet<Stock> Stock { get; set; }
        public DbSet<PizzaRecipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
