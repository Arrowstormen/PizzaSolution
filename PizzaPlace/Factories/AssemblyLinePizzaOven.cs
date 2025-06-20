﻿using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Factories;

/// <summary>
/// Producing one line of pizza. 
/// Taking 7 minutes to setup - and then 5 minutes less for every subsequent pizza of the same recipe type to a minimum of 4 minutes.
/// </summary>
public class AssemblyLinePizzaOven(TimeProvider timeProvider) : PizzaOven(timeProvider)
{
    private const int AssemblyLineCapacity = 1;
    public const int SetupTimeMinutes = 7;
    public const int SubsequentPizzaTimeSavingsInMinutes = 5;
    public const int MinimumCookingTimeMinutes = 4;

    public PizzaRecipeType? currentPizzaType = null;
    public int previouslyAssembled = 0;

    protected override int Capacity => AssemblyLineCapacity;

    protected override void PlanPizzaMaking(IEnumerable<(PizzaRecipeDto Recipe, Guid Guid)> recipeOrders)
    {
        recipeOrders = recipeOrders.OrderBy(x => x.Recipe.RecipeType);
        foreach (var (recipe, orderGuid) in recipeOrders)
        {
            _pizzaQueue.Enqueue((MakePizza(recipe), orderGuid));
        }
    }

    private Func<Task<Pizza?>> MakePizza(PizzaRecipeDto recipe) => async () =>
    {
        if (currentPizzaType == recipe.RecipeType)
        {
            var savings = (recipe.CookingTimeMinutes + SetupTimeMinutes) - (SubsequentPizzaTimeSavingsInMinutes * previouslyAssembled);
            if (MinimumCookingTimeMinutes > savings)
            {
                await CookPizza(MinimumCookingTimeMinutes);
            }
            else
            {
                await CookPizza(savings);
            }
            previouslyAssembled++;
            return GetPizza(recipe.RecipeType);
        }
        else
        {
            await CookPizza(recipe.CookingTimeMinutes + SetupTimeMinutes);
            currentPizzaType = recipe.RecipeType;
            previouslyAssembled = 1;
            return GetPizza(recipe.RecipeType);
        }
    };
}
