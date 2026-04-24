using System.Collections.ObjectModel;
using System.Windows.Input;
using EcommerceCatalog.Models;

namespace EcommerceCatalog.ViewModels;

public class ProductCatalogViewModel : INotifyPropertyChanged
{
    private ObservableCollection<ProductGroup> _productGroups = new();
    private bool _isRefreshing;

    public ObservableCollection<ProductGroup> ProductGroups
    {
        get => _productGroups;
        set
        {
            _productGroups = value;
            OnPropertyChanged(nameof(ProductGroups));
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged(nameof(IsRefreshing));
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddToCartCommand { get; }

    public ProductCatalogViewModel()
    {
        RefreshCommand = new Command(async () => await LoadCatalogAsync());
        AddToCartCommand = new Command<Product>(OnAddToCart);

        // Load initial data
        _ = LoadCatalogAsync();
    }

    private async Task LoadCatalogAsync()
    {
        IsRefreshing = true;

        try
        {
            // Simulate a network call
            await Task.Delay(1200);

            var products = GetSampleProducts();

            var groups = products
                .GroupBy(p => p.Category)
                .Select(g => new ProductGroup(g.Key, g.ToList()))
                .OrderBy(g => g.CategoryName)
                .ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProductGroups = new ObservableCollection<ProductGroup>(groups);
            });
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void OnAddToCart(Product? product)
    {
        if (product is null)
            return;

        // Replace with real cart logic (e.g., navigate, show toast, update cart service)
        Console.WriteLine($"Added to cart: {product.Name} @ {product.Price:C}");
    }

    private static List<Product> GetSampleProducts()
    {
        return new List<Product>
        {
            new() { Id = 1,  Name = "Wireless Headphones",      Price = 79.99m,  Category = "Electronics" },
            new() { Id = 2,  Name = "Bluetooth Speaker",        Price = 49.99m,  Category = "Electronics" },
            new() { Id = 3,  Name = "USB-C Charging Cable",     Price = 12.99m,  Category = "Electronics" },
            new() { Id = 4,  Name = "Laptop Stand",             Price = 34.99m,  Category = "Electronics" },
            new() { Id = 5,  Name = "Running Shoes",            Price = 119.99m, Category = "Footwear" },
            new() { Id = 6,  Name = "Casual Sneakers",          Price = 64.99m,  Category = "Footwear" },
            new() { Id = 7,  Name = "Waterproof Hiking Boots",  Price = 149.99m, Category = "Footwear" },
            new() { Id = 8,  Name = "Graphic T-Shirt",          Price = 24.99m,  Category = "Clothing" },
            new() { Id = 9,  Name = "Slim-Fit Jeans",           Price = 59.99m,  Category = "Clothing" },
            new() { Id = 10, Name = "Hooded Sweatshirt",        Price = 44.99m,  Category = "Clothing" },
            new() { Id = 11, Name = "Stainless Steel Bottle",   Price = 19.99m,  Category = "Kitchen" },
            new() { Id = 12, Name = "Pour-Over Coffee Maker",   Price = 39.99m,  Category = "Kitchen" },
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
