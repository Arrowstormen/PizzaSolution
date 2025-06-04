using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Repositories;

namespace PizzaPlace.Controllers;

[Route("api/restocking")]
public class RestockingController(IStockRepository stockRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Restock([FromBody] ComparableList<StockDto> stock)
    {
        var updatedstocks = new ComparableList<StockDto>();

            foreach (var item in stock)
        {
           var updatedstock = await stockRepository.AddToStock(item);
           updatedstocks.Add(updatedstock);
        }
        return Ok(updatedstocks);
    }
    }

