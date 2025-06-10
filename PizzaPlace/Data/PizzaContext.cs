using Microsoft.EntityFrameworkCore;
using PizzaPlace.Models;
using PizzaPlace.Models.Entities;

namespace PizzaPlace.Data
{
    public class PizzaContext : DbContext, IPizzaContext
    {
        public DbSet<Stock> Stock { get; set; }
        public DbSet<PizzaRecipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=pizzadb.db");
        }
    }
}

