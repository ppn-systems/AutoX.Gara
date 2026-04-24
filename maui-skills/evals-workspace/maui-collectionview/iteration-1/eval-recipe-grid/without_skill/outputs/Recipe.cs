namespace RecipeApp.Models;

/// <summary>
/// Represents a recipe with its basic display properties.
/// </summary>
public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Formatted prep time string for display in the UI.
    /// </summary>
    public string PrepTimeDisplay =>
        PrepTimeMinutes < 60
            ? $"{PrepTimeMinutes} min"
            : $"{PrepTimeMinutes / 60}h {PrepTimeMinutes % 60}min";
}
