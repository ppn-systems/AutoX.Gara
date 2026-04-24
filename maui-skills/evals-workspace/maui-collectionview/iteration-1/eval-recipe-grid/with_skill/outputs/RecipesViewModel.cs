using System.Collections.ObjectModel;
using System.Windows.Input;
using RecipeApp.Models;

namespace RecipeApp.ViewModels;

public class RecipesViewModel : BindableObject
{
    private Recipe? _selectedRecipe;

    public ObservableCollection<Recipe> Recipes { get; } = new ObservableCollection<Recipe>
    {
        new Recipe { Name = "Spaghetti Carbonara", PhotoUrl = "https://picsum.photos/seed/carbonara/400/300", PrepTime = "20 min" },
        new Recipe { Name = "Avocado Toast", PhotoUrl = "https://picsum.photos/seed/avocado/400/300", PrepTime = "10 min" },
        new Recipe { Name = "Chicken Tikka Masala", PhotoUrl = "https://picsum.photos/seed/tikka/400/300", PrepTime = "45 min" },
        new Recipe { Name = "Greek Salad", PhotoUrl = "https://picsum.photos/seed/greek/400/300", PrepTime = "15 min" },
        new Recipe { Name = "Beef Tacos", PhotoUrl = "https://picsum.photos/seed/tacos/400/300", PrepTime = "30 min" },
        new Recipe { Name = "Mushroom Risotto", PhotoUrl = "https://picsum.photos/seed/risotto/400/300", PrepTime = "35 min" },
        new Recipe { Name = "Banana Pancakes", PhotoUrl = "https://picsum.photos/seed/pancakes/400/300", PrepTime = "25 min" },
        new Recipe { Name = "Caesar Salad", PhotoUrl = "https://picsum.photos/seed/caesar/400/300", PrepTime = "12 min" },
    };

    public Recipe? SelectedRecipe
    {
        get => _selectedRecipe;
        set
        {
            if (_selectedRecipe != value)
            {
                _selectedRecipe = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand RecipeSelectedCommand { get; }

    public RecipesViewModel()
    {
        RecipeSelectedCommand = new Command<Recipe>(OnRecipeSelected);
    }

    private void OnRecipeSelected(Recipe? recipe)
    {
        // SelectedItem binding (TwoWay) handles updating SelectedRecipe automatically.
        // This command can be used for navigation or additional logic.
        if (recipe is not null)
        {
            Console.WriteLine($"Selected: {recipe.Name}");
        }
    }
}
