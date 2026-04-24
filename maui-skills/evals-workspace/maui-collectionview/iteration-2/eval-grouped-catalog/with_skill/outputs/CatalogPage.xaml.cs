using EcommerceCatalog.ViewModels;

namespace EcommerceCatalog.Views;

public partial class CatalogPage : ContentPage
{
    public CatalogPage()
    {
        InitializeComponent();
        BindingContext = new CatalogViewModel();
    }
}
