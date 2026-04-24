using EcommerceCatalog.ViewModels;

namespace EcommerceCatalog.Views;

public partial class CatalogPage : ContentPage
{
    public CatalogPage()
    {
        InitializeComponent();
        BindingContext = new CatalogViewModel();
    }

    // The ViewModel handles all loading and command logic.
    // The code-behind stays intentionally minimal — no business logic here.
}
