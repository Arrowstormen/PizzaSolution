namespace PizzaPlace.Models;

public record Menu(string Title, ComparableList<MenuItem> Items);
