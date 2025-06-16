using PizzaPlace.Models;
using PizzaPlace.Pizzas;

namespace PizzaPlace.Services;

public interface IOrderingService
{
    Task<IEnumerable<Pizza>> HandlePizzaOrder(PizzaOrder order);

    Task<int> GetOrderCookingTime(PizzaOrder order);
    double GetOrderPrice(PizzaOrder order, DateTimeOffset time);
}
