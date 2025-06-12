using Microsoft.EntityFrameworkCore;
using PizzaPlace.Data;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Models.Entities;
using Microsoft.AspNetCore.Http.HttpResults;

namespace PizzaPlace.Repositories;

public class StockRepository : IStockRepository
{
    private readonly IPizzaContext _context;

    public StockRepository(IPizzaContext context)
    {
        _context = context;
    }

    public async Task<StockDto> AddToStock(StockDto stock)
    {
        var entity = await _context.Stock.FirstOrDefaultAsync(s => s.StockType == stock.StockType.ToString());

        if (entity == null)
        {
            entity = new Stock
            {
                StockType = stock.StockType.ToString(),
                Amount = stock.Amount
            };

            _context.Stock.Add(entity);
            await _context.SaveChangesAsync();
            return stock;
        }

        entity.Amount = entity.Amount + stock.Amount;
        await _context.SaveChangesAsync();

        var updatedEntity = new StockDto(stock.StockType, entity.Amount);
        return updatedEntity;
    }
    public async Task<StockDto> GetStock(StockType stockType)
    {
        var entity = await _context.Stock.FirstOrDefaultAsync(s => s.StockType == stockType.ToString());
        if (entity == null)
        {
            return new StockDto(stockType, 0);
        }

        return new StockDto(stockType, entity.Amount);

    }
    public async Task<StockDto> TakeStock(StockType stockType, int amount)
    {
        var entity = await _context.Stock.FirstOrDefaultAsync(s => s.StockType == stockType.ToString());

        if (entity == null)
        {
            throw new Exception("StockType does not exist");
        }

        if (entity.Amount < amount)
        {
            throw new Exception("Not enough stock");
        }

        entity.Amount = entity.Amount - amount;
        await _context.SaveChangesAsync();

        var takenStock = new StockDto(stockType, amount);
        return takenStock;
    }
}
