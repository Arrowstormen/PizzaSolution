using Microsoft.EntityFrameworkCore;
using PizzaPlace.Data;
using PizzaPlace.Models;
using PizzaPlace.Models.Entities;
using PizzaPlace.Models.Types;

namespace PizzaPlace.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly IPizzaContext _context;

    public RecipeRepository(IPizzaContext context)
    {
        _context = context;
    }

    // AddOrUpdate
    public async Task<long> AddRecipe(PizzaRecipeDto recipe)
    {
        var entity = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeType == recipe.RecipeType.ToString());
        if (entity != null)
        {
            _context.Recipes.Remove(entity);
        }

        var ingredients = new List<Ingredient>();
        foreach (StockDto stock in recipe.Ingredients)
        {
            var ingredient = new Ingredient
            {
                Amount = stock.Amount,
                StockType = stock.StockType.ToString()
            };
            ingredients.Add(ingredient);
        }

        var newEntity = new PizzaRecipe
        {
            RecipeType = recipe.RecipeType.ToString(),
            Ingredients = ingredients,
            CookingTimeMinutes = recipe.CookingTimeMinutes
        };

        _context.Recipes.Add(newEntity);
        await _context.SaveChangesAsync();

        var addedEntity = await _context.Recipes.FirstOrDefaultAsync(r => r.RecipeType == recipe.RecipeType.ToString());
        return addedEntity.Id;
    }
       
    
    public async Task<PizzaRecipeDto> GetRecipe(PizzaRecipeType recipeType)
    {
        var entity = await _context.Recipes.Include(r => r.Ingredients).FirstOrDefaultAsync(r => r.RecipeType == recipeType.ToString());

        if (entity == null)
        {
            throw new PizzaException("Recipe does not exists of type " + recipeType.ToString() + ".");
        }


        var ingredients = new ComparableList<StockDto>();
        foreach(Ingredient ingredient in entity.Ingredients)
        {
            StockType type;
            Enum.TryParse(ingredient.StockType, out type);

            var stockDto = new StockDto(type, ingredient.Amount);
            ingredients.Add(stockDto);
        }

        var recipeDto = new PizzaRecipeDto(recipeType, ingredients, entity.CookingTimeMinutes, Id:entity.Id);
        return recipeDto;
    }
}
