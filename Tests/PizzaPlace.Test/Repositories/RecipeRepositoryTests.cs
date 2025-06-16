using Moq.EntityFrameworkCore;
using PizzaPlace.Data;
using PizzaPlace.Models;
using PizzaPlace.Models.Entities;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;

namespace PizzaPlace.Test.Repositories;

[TestClass]
public class RecipeRepositoryTests
{
    private static IRecipeRepository GetRecipeRepository(IPizzaContext pizzaContext) => new RecipeRepository(pizzaContext);

    private static ComparableList<StockDto> GetStandardIngredients() =>
    [
        new StockDto(StockType.Dough, 3),
        new StockDto(StockType.Tomatoes, 10),
        new StockDto(StockType.Bacon, 2),
    ];

    private static ComparableList<Ingredient> GetStandardIngredientsEntities() =>
  [
      new Ingredient{
          StockType = StockType.Dough.ToString(),
          Amount = 3
      },
        new Ingredient{
            StockType = StockType.Tomatoes.ToString(),
            Amount = 10
        },
        new Ingredient{ 
            StockType = StockType.Bacon.ToString(),
            Amount = 2
        }

    ];

    private const int StandardCookingTime = 19;
    private static long StandardRecipeId { get; set; }

    [TestMethod]
    public async Task AddRecipe()
    {
        // Arrange
        if (StandardRecipeId > 0)
            return;

        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, GetStandardIngredients(), StandardCookingTime);
        var pizzaContext = new Mock<IPizzaContext>();
        var recipeEntity1 = new PizzaRecipe
        {
            Id = 1,
            RecipeType = PizzaRecipeType.StandardPizza.ToString(),
            Ingredients = GetStandardIngredientsEntities(),
            CookingTimeMinutes = StandardCookingTime

        };
        pizzaContext.Setup(x => x.Recipes).ReturnsDbSet([recipeEntity1]);
        var repository = GetRecipeRepository(pizzaContext.Object);

        // Act
        var actual = await repository.AddRecipe(recipe);

        // Assert
        Assert.IsTrue(actual > 0, "Recipe has an id.");
        StandardRecipeId = actual;
    }
    /*
    [TestMethod]
    public async Task AddRecipe_AlreadyAdded()
    {
        // Arrange
        await AddRecipe();
        var recipe = new PizzaRecipeDto(PizzaRecipeType.StandardPizza, [new StockDto(StockType.UnicornDust, 123), new StockDto(StockType.Anchovies, 1)], StandardCookingTime);
        var pizzaContext = new Mock<IPizzaContext>();
        pizzaContext.Setup(x => x.Recipes).ReturnsDbSet([]);
        var repository = GetRecipeRepository(pizzaContext.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.AddRecipe(recipe));

        // Assert
        Assert.AreEqual("Recipe already added for StandardPizza.", ex.Message);
    }
    */
    [TestMethod]
    public async Task GetRecipe()
    {
        // Arrange
        var pizzaType = PizzaRecipeType.StandardPizza;
        var recipeEntity1 = new PizzaRecipe
        {
            Id = (int)StandardRecipeId,
            RecipeType = PizzaRecipeType.StandardPizza.ToString(),
            Ingredients = GetStandardIngredientsEntities(),
            CookingTimeMinutes = StandardCookingTime
            
        };
        var expected = new PizzaRecipeDto(pizzaType, GetStandardIngredients(), StandardCookingTime, StandardRecipeId);
        var pizzaContext = new Mock<IPizzaContext>();
        pizzaContext.Setup(x => x.Recipes).ReturnsDbSet([recipeEntity1]);
        var repository = GetRecipeRepository(pizzaContext.Object);

        // Act
        var actual = await repository.GetRecipe(pizzaType);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task GetRecipe_DoesNotExist()
    {
        // Arrange
        var pizzaType = PizzaRecipeType.ExtremelyTastyPizza;
        var pizzaContext = new Mock<IPizzaContext>();
        pizzaContext.Setup(x => x.Recipes).ReturnsDbSet([]);
        var repository = GetRecipeRepository(pizzaContext.Object);

        // Act
        var ex = await Assert.ThrowsExceptionAsync<PizzaException>(() => repository.GetRecipe(pizzaType));

        // Assert
        Assert.AreEqual("Recipe does not exists of type ExtremelyTastyPizza.", ex.Message);
    }
}
