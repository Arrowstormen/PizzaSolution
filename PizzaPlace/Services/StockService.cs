using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Services;

public class StockService(IStockRepository stockRepository) : IStockService
{
    public async Task<bool> HasInsufficientStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var requiredStock = new Dictionary<StockType, int>();

        foreach (PizzaAmount pizzaAmount in order.RequestedOrder)
        {
            //Only one recipe per recipe type?
            var ingredients = recipeDtos.Where(x => x.RecipeType == pizzaAmount.PizzaType).Select(x => x.Ingredients).First();

            foreach (var ingredient in ingredients)
            {
                if (requiredStock.ContainsKey(ingredient.StockType))
                {
                    var amount = ingredient.Amount * pizzaAmount.Amount;
                    requiredStock[ingredient.StockType] = requiredStock[ingredient.StockType] + amount;
                }
                else
                {
                    var amount = ingredient.Amount * pizzaAmount.Amount;
                    requiredStock[ingredient.StockType] = amount;
                }
            }
        }

        foreach (var pair in requiredStock)
        {
            if (pair.Value > (await stockRepository.GetStock(pair.Key)).Amount)
                return true;
        }

        return false;
    }

    public async Task<ComparableList<StockDto>> GetStock(PizzaOrder order, ComparableList<PizzaRecipeDto> recipeDtos)
    {
        var requiredStock = new Dictionary<StockType, int>();

        foreach (PizzaAmount pizzaAmount in order.RequestedOrder)
        {
            //Only one recipe per recipe type?
            var ingredients = recipeDtos.Where(x => x.RecipeType == pizzaAmount.PizzaType).Select(x => x.Ingredients).First();

            foreach (var ingredient in ingredients)
            {
                if (requiredStock.ContainsKey(ingredient.StockType))
                {
                    var amount = ingredient.Amount * pizzaAmount.Amount;
                    requiredStock[ingredient.StockType] = requiredStock[ingredient.StockType] + amount;
                }
                else
                {
                    var amount = ingredient.Amount * pizzaAmount.Amount;
                    requiredStock[ingredient.StockType] = amount;
                }
            }
        }

        var stock = new ComparableList<StockDto>();
        foreach (var pair in requiredStock)
        {
            stock.Add(await stockRepository.TakeStock(pair.Key, pair.Value));
        }

        return stock;
    }
}
