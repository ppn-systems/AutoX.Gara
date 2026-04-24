using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RecipeApp.Models;

namespace RecipeApp.ViewModels;

/// <summary>
/// ViewModel for the recipe grid page. Manages a collection of recipes,
/// tracks the currently selected recipe, and exposes commands for user interaction.
/// </summary>
public class RecipesViewModel : INotifyPropertyChanged
{
    private Recipe? _selectedRecipe;

    public ObservableCollection<Recipe> Recipes { get; } = new();

    public Recipe? SelectedRecipe
    {
        get => _selectedRecipe;
        set
        {
            if (_selectedRecipe == value)
                return;
            _selectedRecipe = value;
            OnPropertyChanged();
        }
    }

    public ICommand SelectRecipeCommand { get; }

    public RecipesViewModel()
    {
        SelectRecipeCommand = new Command<Recipe>(OnSelectRecipe);
        LoadRecipes();
    }

    private void OnSelectRecipe(Recipe? recipe)
    {
        // Toggle selection: tapping the already-selected card deselects it.
        SelectedRecipe = SelectedRecipe == recipe ? null : recipe;
    }

    private void LoadRecipes()
    {
        // Sample data. In a real app these would come from a service or repository.
        var sampleRecipes = new[]
        {
            new Recipe { Id = 1, Name = "Spaghetti Carbonara", PhotoUrl = "https://picsum.photos/seed/carbonara/400/300",  PrepTimeMinutes = 25, Category = "Pasta" },
            new Recipe { Id = 2, Name = "Avocado Toast",       PhotoUrl = "https://picsum.photos/seed/avocado/400/300",   PrepTimeMinutes = 10, Category = "Breakfast" },
            new Recipe { Id = 3, Name = "Chicken Tikka",       PhotoUrl = "https://picsum.photos/seed/tikka/400/300",     PrepTimeMinutes = 45, Category = "Indian" },
            new Recipe { Id = 4, Name = "Caesar Salad",        PhotoUrl = "https://picsum.photos/seed/caesar/400/300",    PrepTimeMinutes = 15, Category = "Salad" },
            new Recipe { Id = 5, Name = "Beef Tacos",          PhotoUrl = "https://picsum.photos/seed/tacos/400/300",     PrepTimeMinutes = 30, Category = "Mexican" },
            new Recipe { Id = 6, Name = "Blueberry Pancakes",  PhotoUrl = "https://picsum.photos/seed/pancakes/400/300",  PrepTimeMinutes = 20, Category = "Breakfast" },
            new Recipe { Id = 7, Name = "Margherita Pizza",    PhotoUrl = "https://picsum.photos/seed/pizza/400/300",     PrepTimeMinutes = 35, Category = "Italian" },
            new Recipe { Id = 8, Name = "Miso Ramen",          PhotoUrl = "https://picsum.photos/seed/ramen/400/300",     PrepTimeMinutes = 50, Category = "Japanese" },
            new Recipe { Id = 9, Name = "Greek Salad",         PhotoUrl = "https://picsum.photos/seed/greek/400/300",     PrepTimeMinutes = 12, Category = "Salad" },
            new Recipe { Id = 10, Name = "Chocolate Lava Cake",PhotoUrl = "https://picsum.photos/seed/lavacake/400/300",  PrepTimeMinutes = 22, Category = "Dessert" },
        };

        foreach (var recipe in sampleRecipes)
            Recipes.Add(recipe);
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
