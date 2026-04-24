using System.Collections.ObjectModel;
using System.Windows.Input;
using EcommerceCatalog.Models;

namespace EcommerceCatalog.ViewModels;

public class CatalogViewModel : INotifyPropertyChanged
{
    private bool _isRefreshing;
    private ObservableCollection<ProductGroup> _groupedProducts = new();

    public ObservableCollection<ProductGroup> GroupedProducts
    {
        get => _groupedProducts;
        set
        {
            _groupedProducts = value;
            OnPropertyChanged();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            _isRefreshing = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddToCartCommand { get; }

    public CatalogViewModel()
    {
        RefreshCommand = new Command(async () => await LoadCatalogAsync());
        AddToCartCommand = new Command<Product>(OnAddToCart);
        _ = LoadCatalogAsync();
    }

    private async Task LoadCatalogAsync()
    {
        IsRefreshing = true;

        // Simulate network delay
        await Task.Delay(1000);

        var products = GetSampleProducts();

        var groups = products
            .GroupBy(p => p.Category)
            .Select(g => new ProductGroup(g.Key, g.ToList()))
            .ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            GroupedProducts = new ObservableCollection<ProductGroup>(groups);
            IsRefreshing = false;
        });
    }

    private void OnAddToCart(Product? product)
    {
        if (product is null)
            return;

        // TODO: integrate with cart service
        System.Diagnostics.Debug.WriteLine($"Added to cart: {product.Name}");
    }

    private static List<Product> GetSampleProducts() =>
    [
        new Product { Id = 1, Name = "Wireless Headphones", Price = 79.99m, Category = "Electronics" },
        new Product { Id = 2, Name = "Bluetooth Speaker", Price = 49.99m, Category = "Electronics" },
        new Product { Id = 3, Name = "USB-C Hub", Price = 34.99m, Category = "Electronics" },
        new Product { Id = 4, Name = "Running Shoes", Price = 89.99m, Category = "Footwear" },
        new Product { Id = 5, Name = "Hiking Boots", Price = 119.99m, Category = "Footwear" },
        new Product { Id = 6, Name = "Casual Sneakers", Price = 59.99m, Category = "Footwear" },
        new Product { Id = 7, Name = "Graphic Tee", Price = 24.99m, Category = "Apparel" },
        new Product { Id = 8, Name = "Denim Jacket", Price = 69.99m, Category = "Apparel" },
        new Product { Id = 9, Name = "Yoga Mat", Price = 29.99m, Category = "Fitness" },
        new Product { Id = 10, Name = "Resistance Bands", Price = 19.99m, Category = "Fitness" },
    ];

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
