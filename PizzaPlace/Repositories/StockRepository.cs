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
        var entity = await _context.Stock.Include(s => s.StockType == stock.StockType.ToString()).FirstOrDefaultAsync();

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
        var entity = await _context.Stock.Include(s => s.StockType == stockType.ToString()).FirstOrDefaultAsync();
        if (entity == null)
        {
            return new StockDto(stockType, 0);
        }

        return new StockDto(stockType, entity.Amount);
        
    }
    public Task<StockDto> TakeStock(StockType stockType, int amount) => throw new NotImplementedException("A real repository must be implemented.");
}
