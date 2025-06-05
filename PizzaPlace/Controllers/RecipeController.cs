using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PizzaPlace.Models;
using PizzaPlace.Services;

namespace PizzaPlace.Controllers
{
    [Route("api/recipe")]
    public class RecipeController(IRecipeService recipeService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateRecipe([FromBody] PizzaRecipeDto recipe)
        {
            return Ok(new
            {
               addedRecipe = await recipeService.AddOrUpdateRecipe(recipe)
            });
        }
    }
}
