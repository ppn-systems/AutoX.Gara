using System.Collections.ObjectModel;
using System.Windows.Input;
using ECommerce.Models;

namespace ECommerce.ViewModels;

public class ProductCatalogViewModel : BaseViewModel
{
    private ObservableCollection<ProductGroup> _productGroups = new();
    private bool _isRefreshing;
    private bool _isEmpty;

    public ObservableCollection<ProductGroup> ProductGroups
    {
        get => _productGroups;
        set => SetProperty(ref _productGroups, value);
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        set => SetProperty(ref _isEmpty, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddToCartCommand { get; }

    public ProductCatalogViewModel()
    {
        RefreshCommand = new Command(async () => await LoadCatalogAsync());
        AddToCartCommand = new Command<Product>(OnAddToCart);

        // Load on construction
        Task.Run(async () => await LoadCatalogAsync());
    }

    private async Task LoadCatalogAsync()
    {
        IsRefreshing = true;

        try
        {
            // Simulate network delay
            await Task.Delay(1000);

            var products = GetSampleProducts();

            var groups = products
                .GroupBy(p => p.Category)
                .Select(g => new ProductGroup(g.Key, g.ToList()))
                .ToList();

            ProductGroups = new ObservableCollection<ProductGroup>(groups);
            IsEmpty = ProductGroups.Count == 0;
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void OnAddToCart(Product product)
    {
        if (product is null)
            return;

        // In a real app, dispatch to a cart service
        System.Diagnostics.Debug.WriteLine($"Added to cart: {product.Name} (${product.Price:F2})");

        // Example: show a toast or trigger navigation
        Application.Current?.MainPage?.DisplayAlert(
            "Cart",
            $"'{product.Name}' added to cart.",
            "OK");
    }

    private static List<Product> GetSampleProducts() =>
    [
        new Product { Id = 1,  Name = "Running Shoes",      Price = 89.99m,  Category = "Footwear" },
        new Product { Id = 2,  Name = "Trail Boots",        Price = 129.99m, Category = "Footwear" },
        new Product { Id = 3,  Name = "Sandals",            Price = 39.99m,  Category = "Footwear" },
        new Product { Id = 4,  Name = "T-Shirt",            Price = 24.99m,  Category = "Clothing" },
        new Product { Id = 5,  Name = "Jacket",             Price = 79.99m,  Category = "Clothing" },
        new Product { Id = 6,  Name = "Shorts",             Price = 34.99m,  Category = "Clothing" },
        new Product { Id = 7,  Name = "Wireless Headphones",Price = 149.99m, Category = "Electronics" },
        new Product { Id = 8,  Name = "Smart Watch",        Price = 299.99m, Category = "Electronics" },
        new Product { Id = 9,  Name = "Portable Charger",   Price = 49.99m,  Category = "Electronics" },
        new Product { Id = 10, Name = "Yoga Mat",           Price = 29.99m,  Category = "Fitness" },
        new Product { Id = 11, Name = "Dumbbells (10 lb)",  Price = 44.99m,  Category = "Fitness" },
        new Product { Id = 12, Name = "Resistance Bands",   Price = 19.99m,  Category = "Fitness" },
    ];
}
