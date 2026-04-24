using ECommerce.ViewModels;

namespace ECommerce.Views;

public partial class ProductCatalogPage : ContentPage
{
    public ProductCatalogPage()
    {
        InitializeComponent();
    }

    public ProductCatalogPage(ProductCatalogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
