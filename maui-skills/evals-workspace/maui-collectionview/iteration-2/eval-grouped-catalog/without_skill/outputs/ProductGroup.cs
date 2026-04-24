using System.Collections.ObjectModel;
using EcommerceCatalog.Models;

namespace EcommerceCatalog.Models;

/// <summary>
/// Represents a named category group that holds a collection of products.
/// Inherits from ObservableCollection so the CollectionView reacts to item changes.
/// </summary>
public class ProductGroup : ObservableCollection<Product>
{
    public string CategoryName { get; }

    /// <summary>Computed from Count so the header stays in sync automatically.</summary>
    public int ItemCount => Count;

    public ProductGroup(string categoryName, IEnumerable<Product> products)
        : base(products)
    {
        CategoryName = categoryName;
    }
}
