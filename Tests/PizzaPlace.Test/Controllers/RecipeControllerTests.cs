using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Controllers;
using PizzaPlace.Models.Types;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Test.Controllers
{
    [TestClass]
    public class RecipeControllerTests
    {
        private static RecipeController GetController(Mock<IRecipeService> recipeService) =>
        new(recipeService.Object);

        [TestMethod]
        public async Task AddOrUpdateRecipe()
        {
            // Arrange
            var rareRecipe = new PizzaRecipeDto(PizzaRecipeType.RarePizza, [new StockDto(StockType.UnicornDust, 1)], 1);

            var recipeService = new Mock<IRecipeService>(MockBehavior.Strict);
            recipeService.Setup(x => x.AddOrUpdateRecipe(rareRecipe))
                .ReturnsAsync(rareRecipe);

            var controller = GetController(recipeService);

            // Act
            var actual = await controller.AddOrUpdateRecipe(rareRecipe);

            // Assert
            Assert.IsInstanceOfType<OkObjectResult>(actual);
            recipeService.VerifyAll();
        }
    }
}
