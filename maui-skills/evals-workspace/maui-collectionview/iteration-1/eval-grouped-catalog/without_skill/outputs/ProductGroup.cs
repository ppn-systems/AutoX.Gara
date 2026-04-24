using System.Collections.ObjectModel;
using ECommerce.Models;

namespace ECommerce.Models;

public class ProductGroup : ObservableCollection<Product>
{
    public string CategoryName { get; set; }
    public int ItemCount => Count;

    public ProductGroup(string categoryName, IEnumerable<Product> products)
        : base(products)
    {
        CategoryName = categoryName;
    }
}
