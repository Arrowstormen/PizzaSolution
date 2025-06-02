using PizzaPlace.Models;
using PizzaPlace.Models.Types;
using PizzaPlace.Repositories;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Services;

[TestClass]
public class MenuServiceTests
{

    [TestMethod]
    public void ReturnStandardMenuOutsideLunchTime()
    {
        // Arrange
        var standardMenuItems = new ComparableList<MenuItem>()
        {
             new MenuItem ("The Classic", Models.Types.PizzaRecipeType.StandardPizza, 100.00 ),
             new MenuItem ("The Vegan", Models.Types.PizzaRecipeType.StandardPizza, 90.00 ),
             new MenuItem ("The Freaky", Models.Types.PizzaRecipeType.OddPizza, 120.00 ),
             new MenuItem ("The Horse Radish", Models.Types.PizzaRecipeType.HorseRadishPizza, 105.00 ),
             new MenuItem ("The Tasty Extreme", Models.Types.PizzaRecipeType.ExtremelyTastyPizza, 150.00 ),
             new MenuItem ("The Tasty Vegan Extreme", Models.Types.PizzaRecipeType.ExtremelyTastyPizza, 140.00 ),
             new MenuItem ("The Desolation", Models.Types.PizzaRecipeType.EmptyPizza, 60.00 ),
             new MenuItem ("The Dinner Special", Models.Types.PizzaRecipeType.RarePizza, 110.00 ),
             new MenuItem ("The Vegan Dinner", Models.Types.PizzaRecipeType.RarePizza, 105.00 ),
             new MenuItem ("The Chicken Dinner", Models.Types.PizzaRecipeType.OddPizza, 110.00 ),
             new MenuItem ("The Kiddy Pool", Models.Types.PizzaRecipeType.StandardPizza, 80.00 ),
             new MenuItem ("The Vegan Kiddy Pool", Models.Types.PizzaRecipeType.StandardPizza, 75.00 ),

        };
        var expected = new Menu("Standard Menu", standardMenuItems);

        var time = DateTimeOffset.Parse("18:00:00");
        var service = new MenuService();

        // Act
        var actual = service.GetMenu(time);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ReturnLunchMenuDuringLunchTime()
    {
        // Arrange
        var lunchMenuItems = new ComparableList<MenuItem>
        {
             new MenuItem ("The Lunch Classic", Models.Types.PizzaRecipeType.StandardPizza, 90.00 ),
             new MenuItem ("The Lunch Vegan", Models.Types.PizzaRecipeType.StandardPizza, 80.00 ),
             new MenuItem ("The Freaky Lunch", Models.Types.PizzaRecipeType.OddPizza, 110.00 ),
             new MenuItem ("The Horse Radish Lunch", Models.Types.PizzaRecipeType.HorseRadishPizza, 100.00 ),
             new MenuItem ("The Tasty Extreme", Models.Types.PizzaRecipeType.ExtremelyTastyPizza, 140.00 ),
             new MenuItem ("The Tasty Vegan Extreme", Models.Types.PizzaRecipeType.ExtremelyTastyPizza, 130.00 ),
             new MenuItem ("The Desert", Models.Types.PizzaRecipeType.EmptyPizza, 55.00 ),
             new MenuItem ("The Lunch Special", Models.Types.PizzaRecipeType.RarePizza, 100.00 ),
             new MenuItem ("The Vegan Lunch", Models.Types.PizzaRecipeType.RarePizza, 95.00 ),
             new MenuItem ("The Munch Lunch", Models.Types.PizzaRecipeType.OddPizza, 120.00 ),
             new MenuItem ("The Kiddy Pool", Models.Types.PizzaRecipeType.StandardPizza, 75.00 ),
             new MenuItem ("The Vegan Kiddy Pool", Models.Types.PizzaRecipeType.StandardPizza, 70.00 ),
        };
        var expected = new Menu("Lunch Menu", lunchMenuItems);

        var time = DateTimeOffset.Parse("12:00:00");
        var service = new MenuService();

        // Act
        var actual = service.GetMenu(time);

        // Assert
        Assert.AreEqual(expected, actual);
    }
}