using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RecipeApp.Models;

namespace RecipeApp.ViewModels;

public class RecipesViewModel : INotifyPropertyChanged
{
    private Recipe? _selectedRecipe;

    public ObservableCollection<Recipe> Recipes { get; } = new();

    public Recipe? SelectedRecipe
    {
        get => _selectedRecipe;
        set
        {
            if (_selectedRecipe == value) return;
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
        // Toggle off if tapping the already-selected item
        SelectedRecipe = SelectedRecipe == recipe ? null : recipe;
    }

    private void LoadRecipes()
    {
        var recipes = new[]
        {
            new Recipe { Id = 1, Name = "Spaghetti Bolognese",   PhotoUrl = "https://picsum.photos/seed/recipe1/400/300", PrepTimeMinutes = 45 },
            new Recipe { Id = 2, Name = "Chicken Tikka Masala",  PhotoUrl = "https://picsum.photos/seed/recipe2/400/300", PrepTimeMinutes = 60 },
            new Recipe { Id = 3, Name = "Caesar Salad",          PhotoUrl = "https://picsum.photos/seed/recipe3/400/300", PrepTimeMinutes = 15 },
            new Recipe { Id = 4, Name = "Beef Tacos",            PhotoUrl = "https://picsum.photos/seed/recipe4/400/300", PrepTimeMinutes = 30 },
            new Recipe { Id = 5, Name = "Mushroom Risotto",      PhotoUrl = "https://picsum.photos/seed/recipe5/400/300", PrepTimeMinutes = 40 },
            new Recipe { Id = 6, Name = "Pancakes",              PhotoUrl = "https://picsum.photos/seed/recipe6/400/300", PrepTimeMinutes = 20 },
            new Recipe { Id = 7, Name = "Greek Salad",           PhotoUrl = "https://picsum.photos/seed/recipe7/400/300", PrepTimeMinutes = 10 },
            new Recipe { Id = 8, Name = "Grilled Salmon",        PhotoUrl = "https://picsum.photos/seed/recipe8/400/300", PrepTimeMinutes = 25 },
            new Recipe { Id = 9, Name = "Vegetable Stir Fry",    PhotoUrl = "https://picsum.photos/seed/recipe9/400/300", PrepTimeMinutes = 20 },
            new Recipe { Id = 10, Name = "Chocolate Lava Cake",  PhotoUrl = "https://picsum.photos/seed/recipe10/400/300", PrepTimeMinutes = 35 },
        };

        foreach (var recipe in recipes)
            Recipes.Add(recipe);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
