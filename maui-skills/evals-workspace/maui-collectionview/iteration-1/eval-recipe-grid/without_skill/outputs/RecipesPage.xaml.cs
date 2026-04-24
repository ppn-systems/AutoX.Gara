using RecipeApp.ViewModels;

namespace RecipeApp.Views;

/// <summary>
/// Code-behind for the recipe grid page.
/// The ViewModel is assigned here so the page is fully self-contained;
/// in a larger app you would inject it via a DI container.
/// </summary>
public partial class RecipesPage : ContentPage
{
    public RecipesPage()
    {
        InitializeComponent();
        BindingContext = new RecipesViewModel();
    }
}
