using EcommerceCatalog.ViewModels;

namespace EcommerceCatalog.Views;

public partial class ProductCatalogPage : ContentPage
{
    public ProductCatalogPage()
    {
        InitializeComponent();
        BindingContext = new ProductCatalogViewModel();
    }
}
