using System.ComponentModel.DataAnnotations;

namespace PizzaPlace.Models.Entities
{
    public class Stock
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string StockType { get; set; }

        public int Amount { get; set; }
    }
}
