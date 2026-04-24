namespace RecipeApp.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public string PrepTimeDisplay => PrepTimeMinutes < 60
        ? $"{PrepTimeMinutes} min"
        : $"{PrepTimeMinutes / 60}h {PrepTimeMinutes % 60}m";
}
