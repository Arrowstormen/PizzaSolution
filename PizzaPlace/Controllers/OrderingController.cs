using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers;

[Route("api/order")]
public class OrderingController(
    IOrderingService orderingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlacePizzaOrder([FromBody] PizzaOrder pizzaOrder)
    {
        orderingService.HandlePizzaOrder(pizzaOrder);
        var result = (orderingService.GetOrderCookingTime(pizzaOrder), orderingService.GetOrderPrice(pizzaOrder, DateTimeOffset.Now));
        return Ok(result);
    }
}
