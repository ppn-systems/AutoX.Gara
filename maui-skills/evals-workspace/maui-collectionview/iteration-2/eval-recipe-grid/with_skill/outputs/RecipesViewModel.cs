using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RecipeApp.Models;

namespace RecipeApp.ViewModels;

public partial class RecipesViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Recipe> _recipes = [];

    [ObservableProperty]
    private Recipe? _selectedRecipe;

    public RecipesViewModel()
    {
        LoadRecipes();
    }

    private void LoadRecipes()
    {
        Recipes =
        [
            new Recipe { Name = "Spaghetti Carbonara", PhotoUrl = "https://picsum.photos/seed/carbonara/400/300", PrepTime = "20 min" },
            new Recipe { Name = "Avocado Toast", PhotoUrl = "https://picsum.photos/seed/avocado/400/300", PrepTime = "10 min" },
            new Recipe { Name = "Chicken Tikka Masala", PhotoUrl = "https://picsum.photos/seed/tikka/400/300", PrepTime = "45 min" },
            new Recipe { Name = "Greek Salad", PhotoUrl = "https://picsum.photos/seed/greek/400/300", PrepTime = "15 min" },
            new Recipe { Name = "Beef Tacos", PhotoUrl = "https://picsum.photos/seed/tacos/400/300", PrepTime = "30 min" },
            new Recipe { Name = "Banana Pancakes", PhotoUrl = "https://picsum.photos/seed/pancakes/400/300", PrepTime = "25 min" },
            new Recipe { Name = "Tomato Soup", PhotoUrl = "https://picsum.photos/seed/tomato/400/300", PrepTime = "35 min" },
            new Recipe { Name = "Caesar Salad", PhotoUrl = "https://picsum.photos/seed/caesar/400/300", PrepTime = "12 min" },
        ];
    }

    [RelayCommand]
    private void SelectRecipe(Recipe? recipe)
    {
        SelectedRecipe = recipe;
    }
}
