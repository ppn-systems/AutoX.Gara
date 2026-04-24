using RecipeApp.ViewModels;

namespace RecipeApp.Views;

public partial class RecipesPage : ContentPage
{
    public RecipesPage()
    {
        InitializeComponent();
        BindingContext = new RecipesViewModel();
    }
}
