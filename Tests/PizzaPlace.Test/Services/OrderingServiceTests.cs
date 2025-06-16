using PizzaPlace.Consoles;
using PizzaPlace.Factories;
using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Pizzas;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class OrderingServiceTests
{
    private static OrderingService GetService(IStockService stockService, IRecipeService recipeService, IMenuService menuService, IConsole console, IPizzaOven pizzaOven) =>
        new OrderingService(stockService, recipeService, menuService, console, pizzaOven);

    [TestMethod]
    public async Task HandlePizzaOrder()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 54),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 0),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 4),
        ];
        var order = new PizzaOrder(requests);
        var standardRecipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza,
            [
                new StockDto(StockType.Dough, 2),
                new StockDto(StockType.Tomatoes, 1),
            ], 10);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15);
        ComparableList<PizzaRecipeDto> recipes = [standardRecipe, tastyRecipe];
        ComparableList<StockDto> returnedStock = [new StockDto(StockType.Anchovies, 2)]; // Doesn't matter that it doesn't match recipes.
        ComparableList<PizzaPrepareOrder> prepareOrders =
        [
            new PizzaPrepareOrder(standardRecipe, 58),
            new PizzaPrepareOrder(tastyRecipe, 2),
        ];

        var pizzas = new List<Pizza>{
            new StandardPizza(),
            new ExtremelyTastyPizza(),
        };

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var console = new Mock<IConsole>();

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes))
            .ReturnsAsync(false);
        stockService.Setup(x => x.GetStock(order, recipes))
            .ReturnsAsync(returnedStock);
        pizzaOven.Setup(x => x.PreparePizzas(prepareOrders, returnedStock))
            .ReturnsAsync(pizzas);

        var service = GetService(stockService.Object, recipeService.Object, menuService.Object, console.Object, pizzaOven.Object);

        // Act
        var actual = await service.HandlePizzaOrder(order);

        // Assert
        CollectionAssert.AreEqual(pizzas, actual.ToList());
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }

    [TestMethod]
    public async Task HandlePizzaOrder_InsufficientStock()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 2),
        ];
        var order = new PizzaOrder(requests);
        var tastyRecipe = new PizzaRecipeDto(PizzaRecipeType.ExtremelyTastyPizza,
            [
                new StockDto(StockType.UnicornDust, 1),
                new StockDto(StockType.BellPeppers, 2),
            ], 15);
        ComparableList<PizzaRecipeDto> recipes = [tastyRecipe];

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var console = new Mock<IConsole>();
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes))
            .ReturnsAsync(true);

        var service = GetService(stockService.Object, recipeService.Object, menuService.Object, console.Object, pizzaOven.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(order));

        // Assert
        Assert.AreEqual("Unable to take in order. Insufficient stock.", ex.Message);
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }

    [TestMethod]
    public async Task HandlePizzaOrder_BrokenRecipeServiceDoesNotReturnRecipe()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 1),
        ];
        var order = new PizzaOrder(requests);
        ComparableList<PizzaRecipeDto> recipes = [];
        ComparableList<StockDto> returnedStock = [new StockDto(StockType.Anchovies, 2)]; // Doesn't matter that it doesn't match recipes.

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var console = new Mock<IConsole>();
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);

        recipeService.Setup(x => x.GetPizzaRecipes(order))
            .ReturnsAsync(recipes);
        stockService.Setup(x => x.HasInsufficientStock(order, recipes))
            .ReturnsAsync(false);
        stockService.Setup(x => x.GetStock(order, recipes))
            .ReturnsAsync(returnedStock);

        var service = GetService(stockService.Object, recipeService.Object, menuService.Object, console.Object, pizzaOven.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => service.HandlePizzaOrder(order));

        // Assert
        Assert.AreEqual("Missing recipe. Recipe service did not return a recipe for StandardPizza which was expected.", ex.Message);
        stockService.VerifyAll();
        recipeService.VerifyAll();
        pizzaOven.VerifyAll();
    }

    [TestMethod]
    public void FinishedOrderMessage_WithOnePizzaType()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 54),
        ];
        var order = new PizzaOrder(requests);

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var console = new Mock<IConsole>();

        var service = GetService(stockService.Object, recipeService.Object, menuService.Object, console.Object, pizzaOven.Object);
        var expected = "An order consisting of 54 StandardPizzas finished";
        // Act
        var actual = service.FinishedOrderMessage(order);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void FinishedOrderMessage_WithMultiplePizzaTypes()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 54),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 0),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 1),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 4),
        ];
        var order = new PizzaOrder(requests);


        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var console = new Mock<IConsole>();

        var service = GetService(stockService.Object, recipeService.Object, menuService.Object, console.Object, pizzaOven.Object);
        var expected = "An order consisting of 54 StandardPizzas, 0 StandardPizza, 1 ExtremelyTastyPizza, and 4 StandardPizzas finished";
        // Act
        var actual = service.FinishedOrderMessage(order);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GetOrderPrice_GetCorrectPrice()
    {
        // Arrange
        ComparableList<PizzaAmount> requests =
        [
            new PizzaAmount(PizzaRecipeType.StandardPizza, 54),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 0),
            new PizzaAmount(PizzaRecipeType.ExtremelyTastyPizza, 1),
            new PizzaAmount(PizzaRecipeType.StandardPizza, 4),
        ];
        var order = new PizzaOrder(requests);

        var standardMenuItems = new ComparableList<MenuItem>()
        {
             new MenuItem ("The Classic", Models.Types.PizzaRecipeType.StandardPizza, 100.00 ),
             new MenuItem ("The Vegan", Models.Types.PizzaRecipeType.StandardPizza, 100.00 ),
             new MenuItem ("The Freaky", Models.Types.PizzaRecipeType.OddPizza, 120.00 ),
             new MenuItem ("The Horse Radish", Models.Types.PizzaRecipeType.HorseRadishPizza, 105.00 ),
             new MenuItem ("The Tasty Extreme", Models.Types.PizzaRecipeType.ExtremelyTastyPizza, 150.00 ),
             new MenuItem ("The Tasty Vegan Extreme", Models.Types.PizzaRecipeType.ExtremelyTastyPizza, 150.00 ),
             new MenuItem ("The Desolation", Models.Types.PizzaRecipeType.EmptyPizza, 60.00 ),
             new MenuItem ("The Dinner Special", Models.Types.PizzaRecipeType.RarePizza, 110.00 ),
             new MenuItem ("The Vegan Dinner", Models.Types.PizzaRecipeType.RarePizza, 110.00 ),
             new MenuItem ("The Chicken Dinner", Models.Types.PizzaRecipeType.OddPizza, 120.00 ),
             new MenuItem ("The Kiddy Pool", Models.Types.PizzaRecipeType.StandardPizza, 100.00 ),
             new MenuItem ("The Vegan Kiddy Pool", Models.Types.PizzaRecipeType.StandardPizza, 100.00 ),

        };
       var standardMenu = new Menu("Standard Menu", standardMenuItems);

        var stockService = new Mock<IStockService>(MockBehavior.Strict);
        var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
        var menuService = new Mock<IMenuService>(MockBehavior.Strict);
        var pizzaOven = new Mock<IPizzaOven>(MockBehavior.Strict);
        var console = new Mock<IConsole>();
        var time  = DateTimeOffset.Now;
        menuService.Setup(x => x.GetMenu((time)))
            .Returns(standardMenu);

        var service = GetService(stockService.Object, recipeService.Object, menuService.Object, console.Object, pizzaOven.Object);
        var expected = 5950;
        // Act
        var actual = service.GetOrderPrice(order, time);

        // Assert
        Assert.AreEqual(expected, actual);
    }
}
