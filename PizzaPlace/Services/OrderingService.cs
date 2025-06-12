using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.BearerToken;
using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public class OrderingService(
    IStockService stockService,
    IRecipeService recipeService,
    IMenuService menuService,
    IPizzaOven pizzaOven) : IOrderingService
{
    public async Task<IEnumerable<Pizza>> HandlePizzaOrder(PizzaOrder order)
    {
        var recipes = await recipeService.GetPizzaRecipes(order);
        if (await stockService.HasInsufficientStock(order, recipes))
            throw new PizzaException("Unable to take in order. Insufficient stock.");

        var stock = await stockService.GetStock(order, recipes);

        var prepareOrder = order.RequestedOrder
            .GroupBy(x => x.PizzaType)
            .Select(x => new PizzaPrepareOrder(GetPizzaRecipe(x.Key, recipes), x.Aggregate(0, (total, request) => total + request.Amount)))
            .ToComparableList();

        var pizzas = await pizzaOven.PreparePizzas(prepareOrder, stock);
        Console.WriteLine(FinishedOrderMessage(order)); 
        return pizzas;

        PizzaRecipeDto GetPizzaRecipe(PizzaRecipeType pizzaType, ComparableList<PizzaRecipeDto> recipes) =>
            recipes.FirstOrDefault(x => x.RecipeType == pizzaType) ??
            throw new PizzaException($"Missing recipe. Recipe service did not return a recipe for {pizzaType} which was expected.");
    }

    public string FinishedOrderMessage(PizzaOrder order)
    {
        var message = "An order consisting of ";
        var multiples = "";
        foreach (PizzaAmount pizzaAmount in order.RequestedOrder)
        {
            if (pizzaAmount.Amount > 1)
            {
                multiples = "s";
            } else
            {
                multiples = "";
            }
                message += pizzaAmount.Amount + " " + pizzaAmount.PizzaType.ToString() + multiples + ", ";
        }
        message.Remove(message.Length - 2);
        message += "finished";
        return message;
    }

   public async Task<int> GetOrderCookingTime(PizzaOrder order)
   {
        var recipes = await recipeService.GetPizzaRecipes(order);
        var time = 0;
        foreach (PizzaRecipeDto recipe in recipes)
        {
            time = time + recipe.CookingTimeMinutes;
        }
        return time;
   }

    public double GetOrderPrice(PizzaOrder order)
    {
        var totalPrice = 0.0;
        var menu = menuService.GetMenu(DateTimeOffset.Now);
        foreach (PizzaAmount pizzaAmount in order.RequestedOrder)
        {
            var menuItem = menu.Items.Where(p => p.PizzaType == pizzaAmount.PizzaType).FirstOrDefault();
            if (menuItem == null)
            {
                throw new Exception("MenuItem of an ordered PizzaType does not exist");
            }
            var typePrice = menuItem.Price;
            totalPrice = totalPrice + (typePrice * pizzaAmount.Amount);
        }
        return totalPrice;
    }
}

