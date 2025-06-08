using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Produces pizza on one giant revolving surface. Downside is, that all pizzas must have the same cooking time, when prepared at the same time.
/// </summary>
public class GiantRevolvingPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int GiantRevolvingPizzaOvenCapacity = 120;

    private ConcurrentQueue<int> CookTimeQueue = new ConcurrentQueue<int>();

    protected override int Capacity => GiantRevolvingPizzaOvenCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        recipeOrders = recipeOrders.OrderBy(x => x.Recipe.CookingTimeMinutes);
        if (_pizzaQueue.IsEmpty)
        {
            CookTimeQueue = [];
        } // Slim down queue if there's any pizzas queued up? 

        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            _pizzaQueue.Enqueue((MakePizza(recipe), orderGuid));
        }
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe) => async () =>
    {
        if (CookTimeQueue.Count == 0)
        {
            CookTimeQueue.Enqueue(recipe.CookingTimeMinutes);
            await CookPizza(recipe.CookingTimeMinutes);
      
        } 
        else
        {
            var sumCookTime = CookTimeQueue.Sum();
            if (CookTimeQueue.Last() == recipe.CookingTimeMinutes)
            {
                await CookPizza(sumCookTime);
            } 
            else
            {
                CookTimeQueue.Enqueue(recipe.CookingTimeMinutes);
                await CookPizza(sumCookTime + recipe.CookingTimeMinutes);
            }

        }
        return GetPizza(recipe.RecipeType);
    };
}
