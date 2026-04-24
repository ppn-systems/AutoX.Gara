using System.Collections.ObjectModel;
using System.Windows.Input;
using EcommerceCatalog.Models;

namespace EcommerceCatalog.ViewModels;

public class CatalogViewModel : INotifyPropertyChanged
{
    // ---------------------------------------------------------------------------
    // Fields
    // ---------------------------------------------------------------------------
    private ObservableCollection<ProductGroup> _productGroups = [];
    private bool _isRefreshing;
    private bool _isEmpty;

    // ---------------------------------------------------------------------------
    // Properties
    // ---------------------------------------------------------------------------

    public ObservableCollection<ProductGroup> ProductGroups
    {
        get => _productGroups;
        private set
        {
            if (_productGroups == value) return;
            _productGroups = value;
            OnPropertyChanged(nameof(ProductGroups));
            UpdateIsEmpty();
        }
    }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set
        {
            if (_isRefreshing == value) return;
            _isRefreshing = value;
            OnPropertyChanged(nameof(IsRefreshing));
        }
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        private set
        {
            if (_isEmpty == value) return;
            _isEmpty = value;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    // ---------------------------------------------------------------------------
    // Commands
    // ---------------------------------------------------------------------------

    public ICommand RefreshCommand { get; }
    public ICommand AddToCartCommand { get; }

    // ---------------------------------------------------------------------------
    // Constructor
    // ---------------------------------------------------------------------------

    public CatalogViewModel()
    {
        RefreshCommand = new Command(async () => await LoadProductsAsync());
        AddToCartCommand = new Command<Product>(OnAddToCart);

        // Load on construction without blocking the constructor.
        _ = LoadProductsAsync();
    }

    // ---------------------------------------------------------------------------
    // Private methods
    // ---------------------------------------------------------------------------

    private async Task LoadProductsAsync()
    {
        IsRefreshing = true;

        try
        {
            // Simulate a network delay.
            await Task.Delay(800);

            var products = await FetchProductsAsync();

            var groups = products
                .GroupBy(p => p.Category)
                .OrderBy(g => g.Key)
                .Select(g => new ProductGroup(g.Key, g.OrderBy(p => p.Name)))
                .ToList();

            ProductGroups = new ObservableCollection<ProductGroup>(groups);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private void OnAddToCart(Product? product)
    {
        if (product is null) return;

        // In a real app this would call a cart service.
        // For now, show a simple alert via the shell or raise a messenger event.
        Console.WriteLine($"Added to cart: {product.Name} (${product.Price:F2})");
    }

    private void UpdateIsEmpty()
    {
        IsEmpty = _productGroups.Count == 0 ||
                  _productGroups.All(g => g.Count == 0);
    }

    // ---------------------------------------------------------------------------
    // Simulated data source
    // ---------------------------------------------------------------------------

    private static Task<List<Product>> FetchProductsAsync() =>
        Task.FromResult(new List<Product>
        {
            new() { Id = 1,  Name = "Trail Running Shoes",  Category = "Footwear",     Price = 129.99m },
            new() { Id = 2,  Name = "Leather Boots",        Category = "Footwear",     Price = 189.95m },
            new() { Id = 3,  Name = "Slip-on Sneakers",     Category = "Footwear",     Price = 59.99m  },
            new() { Id = 4,  Name = "Wool Beanie",          Category = "Accessories",  Price = 24.99m  },
            new() { Id = 5,  Name = "Leather Wallet",       Category = "Accessories",  Price = 49.99m  },
            new() { Id = 6,  Name = "Sunglasses",           Category = "Accessories",  Price = 89.00m  },
            new() { Id = 7,  Name = "Graphic Tee",          Category = "Tops",         Price = 34.99m  },
            new() { Id = 8,  Name = "Polo Shirt",           Category = "Tops",         Price = 54.99m  },
            new() { Id = 9,  Name = "Hooded Sweatshirt",    Category = "Tops",         Price = 74.99m  },
            new() { Id = 10, Name = "Chino Trousers",       Category = "Bottoms",      Price = 69.99m  },
            new() { Id = 11, Name = "Slim Fit Jeans",       Category = "Bottoms",      Price = 89.99m  },
            new() { Id = 12, Name = "Cargo Shorts",         Category = "Bottoms",      Price = 44.99m  },
        });

    // ---------------------------------------------------------------------------
    // INotifyPropertyChanged
    // ---------------------------------------------------------------------------

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
