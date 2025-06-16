using Microsoft.AspNetCore.Mvc.ModelBinding;
using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Produces pizza on one giant revolving surface. Downside is, that all pizzas must have the same cooking time, when prepared at the same time.
/// </summary>
public class GiantRevolvingPizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int GiantRevolvingPizzaOvenCapacity = 120;
    private int QueuedCookingTime = 0;
    private ConcurrentQueue<(PizzaRecipeDto Recipe, Guid Guid)> _waitingQueue = new();

    protected override int Capacity => GiantRevolvingPizzaOvenCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        if (_pizzaQueue.IsEmpty)
        {
            QueuedCookingTime = 0;
        }

        recipeOrders = recipeOrders.OrderBy(x => x.Recipe.CookingTimeMinutes);
        Planning(recipeOrders);    
    }

    private void Planning(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            if (QueuedCookingTime == 0)
            {
                _pizzaQueue.Enqueue((MakePizza(recipe), orderGuid));
                QueuedCookingTime = recipe.CookingTimeMinutes;
            }
            else if (recipe.CookingTimeMinutes == QueuedCookingTime)
            {
                _pizzaQueue.Enqueue((MakePizza(recipe), orderGuid));
            }
            else
            {
                if (!_waitingQueue.Contains((recipe, orderGuid)))
                {
                    _waitingQueue.Enqueue((recipe, orderGuid));
                }
            }
        }

        if (!_waitingQueue.IsEmpty)
        {
            AddWaitingOrdersToPizzaQueue();
        }

    }

    private async Task AddWaitingOrdersToPizzaQueue()
    {
        await Task.Run(() =>
        {
            while (!_pizzaQueue.IsEmpty)
            {

            }
            QueuedCookingTime = 0;
            Planning(_waitingQueue);
        });
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe) => async () =>
    {
        await CookPizza(recipe.CookingTimeMinutes);

        return GetPizza(recipe.RecipeType);
    };
}
