using PizzaPlace.Models.Types;
using PizzaPlace.Models;
using PizzaPlace.Repositories;
using PizzaPlace.Services;
using Microsoft.AspNetCore.Mvc;

namespace PizzaPlace.Test.Services;

[TestClass]
public class StockServiceTests
{
    private static StockService GetService(Mock<IStockRepository> stockRepository) =>
        new(stockRepository.Object);

    [TestMethod]
    public async Task HasInsufficientStockReturnsFalseWhenSufficient()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 20),
        ]);
        
        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 100);
        ComparableList<PizzaRecipeDto> recipes = [rareRecipe, oddRecipe];

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.SetupSequence(x => x.GetStock(StockType.UnicornDust))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 21))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 20));
        stockRepository.Setup(x => x.GetStock(StockType.Sulphur)).
            ReturnsAsync(new StockDto(StockType.Sulphur, 22));

        var service = GetService(stockRepository);
        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsFalse(result);
        stockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task HasInsufficientStockReturnsTrueWhenInsufficient()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 20),
        ]);

        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 100);
        ComparableList<PizzaRecipeDto> recipes = [rareRecipe, oddRecipe];

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.SetupSequence(x => x.GetStock(StockType.UnicornDust))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 21))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 20));
        stockRepository.Setup(x => x.GetStock(StockType.Sulphur)).
            ReturnsAsync(new StockDto(StockType.Sulphur, 19));

        var service = GetService(stockRepository);
        // Act
        var result = await service.HasInsufficientStock(order, recipes);

        // Assert
        Assert.IsTrue(result);
        stockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task GetStockReturnsRequestedStock()
    {
        // Arrange
        var order = new PizzaOrder([
            new PizzaAmount(PizzaRecipeType.RarePizza, 1),
            new PizzaAmount(PizzaRecipeType.OddPizza, 2),
            new PizzaAmount(PizzaRecipeType.RarePizza, 20),
        ]);

        var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);
        var oddRecipe = new PizzaRecipeDto(PizzaRecipeType.OddPizza, [new StockDto(StockType.Sulphur, 10)], 100);
        ComparableList<PizzaRecipeDto> recipes = [rareRecipe, oddRecipe];

        var expected = new ComparableList<StockDto>
        {
            new StockDto(StockType.UnicornDust, 21),
            new StockDto(StockType.Sulphur, 20),
        };

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.TakeStock(StockType.UnicornDust, 21))
            .ReturnsAsync(new StockDto(StockType.UnicornDust, 21));
        stockRepository.Setup(x => x.TakeStock(StockType.Sulphur, 20)).
            ReturnsAsync(new StockDto(StockType.Sulphur, 20));


        var service = GetService(stockRepository);
        // Act
        var actual = await service.GetStock(order, recipes);

        // Assert
        Assert.AreEqual(expected, actual);
        stockRepository.VerifyAll();
    }

    [TestMethod]
    public async Task RestockAddsStock()
    {
        // Arrange
        var stock = new ComparableList<StockDto>();
        var resstock = new ComparableList<StockDto>();
        var stock1 = new StockDto(StockType.UnicornDust, 1);
        var stock2 = new StockDto(StockType.Bacon, 2);
        var stock3 = new StockDto(StockType.Chocolate, 11);
        var resstock1 = new StockDto(StockType.UnicornDust, 1);
        var resstock2 = new StockDto(StockType.Bacon, 12);
        var resstock3 = new StockDto(StockType.Chocolate, 16);
        stock.Add(stock1);
        stock.Add(stock2);
        stock.Add(stock3);
        resstock.Add(resstock1);
        resstock.Add(resstock2);
        resstock.Add(resstock3);

        var stockRepository = new Mock<IStockRepository>(MockBehavior.Strict);
        stockRepository.Setup(x => x.AddToStock(stock1))
            .ReturnsAsync(resstock1);
        stockRepository.Setup(x => x.AddToStock(stock2))
            .ReturnsAsync(resstock2);
        stockRepository.Setup(x => x.AddToStock(stock3))
            .ReturnsAsync(resstock3);

        var controller = GetService(stockRepository);

        // Act
        var actual = await controller.Restock(stock);

        // Assert
        Assert.AreEqual(resstock, actual);
        stockRepository.VerifyAll();
    }
}
