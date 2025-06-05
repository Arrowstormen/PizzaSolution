using PizzaPlace.Models;

namespace PizzaPlace.Services;

public interface IRecipeService
{
    Task<ComparableList<PizzaRecipeDto>> GetPizzaRecipes(PizzaOrder order);

    Task<PizzaRecipeDto> AddOrUpdateRecipe(PizzaRecipeDto recipe);
}
