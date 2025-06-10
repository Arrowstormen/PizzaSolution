using System.ComponentModel.DataAnnotations;

namespace PizzaPlace.Models.Entities
{
    public class PizzaRecipe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RecipeType { get; set; }

        public ICollection<Ingredient> Ingredients { get; set; }

        public int CookingTimeMinutes { get; set; }
    }
}
