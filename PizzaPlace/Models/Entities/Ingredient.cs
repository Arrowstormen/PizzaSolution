using System.ComponentModel.DataAnnotations;

namespace PizzaPlace.Models.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }

        [Required]
        public PizzaRecipe Recipe { get; set; }

        [Required]
        [StringLength(50)]
        public string StockType { get; set; }

        public int Amount { get; set; }
    }
}
