using Microsoft.EntityFrameworkCore;
using Moq.EntityFrameworkCore;
using PizzaPlace.Data;
using PizzaPlace.Models;
using PizzaPlace.Models.Entities;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Repositories;

[TestClass]
public class StockRepositoryTests
{
    private static IStockRepository GetStockRepository(IPizzaContext pizzaContext) => new StockRepository(pizzaContext);

    [TestMethod]
    public async Task AddToStock()
    {
        // Arrange
        var addedAmount = 10;
        var stock = new StockDto(StockType.TrippleBacon, addedAmount);
        var pizzaContext = new Mock<IPizzaContext>();
        pizzaContext.Setup(x => x.Stock).ReturnsDbSet([]);

        var repository = GetStockRepository(pizzaContext.Object);

        // Act
        var actual = await repository.AddToStock(stock);

        // Assert
        Assert.IsTrue(actual.Amount >= addedAmount);
    }

    [TestMethod]
    public async Task AddToStock_MoreThanOnce()
    {
        // Arrange
        var addedAmount = 10;
        var secondAddedAmount = 13;
        var expectedLeastAmount = addedAmount + secondAddedAmount;
        var stock1 = new StockDto(StockType.UnicornDust, addedAmount);
        var stock2 = new StockDto(StockType.UnicornDust, secondAddedAmount);
        var stockEntity1 = new Stock{
            StockType = StockType.UnicornDust.ToString(),
            Amount = 0};
        var pizzaContext = new Mock<IPizzaContext>();
        pizzaContext.Setup(x => x.Stock).ReturnsDbSet([stockEntity1]);
        var repository = GetStockRepository(pizzaContext.Object);

        // Act
        await repository.AddToStock(stock1);
        var actual = await repository.AddToStock(stock2);

        // Assert
        Assert.IsTrue(actual.Amount >= expectedLeastAmount);
    }

    [TestMethod]
    public async Task AddToStock_NegativeAmount()
    {
        // Arrange
        var addedAmount = -10;
        var stock = new StockDto(StockType.TrippleBacon, addedAmount);
        var pizzaContext = new Mock<IPizzaContext>();
        var repository = GetStockRepository(pizzaContext.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.AddToStock(stock));

        // Assert
        Assert.AreEqual("Stock cannot have negative amount.", ex.Message);
    }

    [TestMethod]
    public async Task GetStock()
    {
        // Arrange
        var stockType = StockType.UngenericSpices;
        var pizzaContext = new Mock<IPizzaContext>();
        pizzaContext.Setup(x => x.Stock).ReturnsDbSet([]);
        var repository = GetStockRepository(pizzaContext.Object);

        // Act
        var actual = await repository.GetStock(stockType);

        // Assert
        Assert.AreEqual(stockType, actual.StockType);
    }

    [TestMethod]
    public async Task GetStock_WithAddToStock()
    {
        // Arrange
        var addedAmount = 233;
        var stockType = StockType.GenericSpices;
        var pizzaContext = new Mock<IPizzaContext>();
        var stockEntity1 = new Stock
        {
            StockType = StockType.GenericSpices.ToString(),
            Amount = 123
        };
        pizzaContext.Setup(x => x.Stock).ReturnsDbSet([stockEntity1]);
        var repository = GetStockRepository(pizzaContext.Object);
        var startStock = await repository.GetStock(stockType);
        var expected = startStock with { Amount = startStock.Amount + addedAmount };

        // Act
        await repository.AddToStock(new StockDto(stockType, addedAmount));
        var actual = await repository.GetStock(stockType);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task TakeStock()
    {
        // Arrange
        var stockType = StockType.FermentedDough;
        var amount = 7;
        var pizzaContext = new Mock<IPizzaContext>();
        var stockEntity1 = new Stock
        {
            StockType = StockType.FermentedDough.ToString(),
            Amount = amount +5
        };
        pizzaContext.Setup(x => x.Stock).ReturnsDbSet([stockEntity1]);
        var repository = GetStockRepository(pizzaContext.Object);
        var expected = new StockDto(stockType, amount);

        // Act
        var actual = await repository.TakeStock(stockType, amount);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task TakeStock_NegativeAmount()
    {
        // Arrange
        var amount = -1;
        var stockType = StockType.FermentedDough;
        var pizzaContext = new Mock<IPizzaContext>();
        var stockEntity1 = new Stock
        {
            StockType = StockType.FermentedDough.ToString(),
            Amount = 5
        };
        var repository = GetStockRepository(pizzaContext.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.TakeStock(stockType, amount));

        // Assert
        Assert.AreEqual("Unable to take zero or negative amount. (Parameter 'amount')", ex.Message);
    }

    [TestMethod]
    public async Task TakeStock_NotEnoughStock()
    {
        // Arrange
        var stockType = StockType.FermentedDough;
        var pizzaContext = new Mock<IPizzaContext>();
        var stockEntity1 = new Stock
        {
            StockType = StockType.FermentedDough.ToString(),
            Amount = 5
        };
        pizzaContext.Setup(x => x.Stock).ReturnsDbSet([stockEntity1]);
        var repository = GetStockRepository(pizzaContext.Object);
        var startStock = await repository.GetStock(stockType);
        var amount = startStock.Amount + 1;

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.TakeStock(stockType, amount));

        // Assert
        Assert.AreEqual("Not enough stock to take the given amount.", ex.Message);
    }

    [TestMethod]
    public async Task TakeStock_GetStock()
    {
        // Arrange
        var stockType = StockType.FermentedDough;
        var amount = 7;
        var pizzaContext = new Mock<IPizzaContext>();
        var stockEntity1 = new Stock
        {
            StockType = StockType.FermentedDough.ToString(),
            Amount = 8 + amount
        };
        pizzaContext.Setup(x => x.Stock).ReturnsDbSet([stockEntity1]);
        var repository = GetStockRepository(pizzaContext.Object);
        var startStock = await repository.GetStock(stockType);
        var expected = startStock with { Amount = startStock.Amount - amount };

        // Act
        await repository.TakeStock(stockType, amount);
        var actual = await repository.GetStock(stockType);

        // Assert
        Assert.AreEqual(expected, actual);
    }
}
