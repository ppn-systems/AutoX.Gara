namespace EcommerceCatalog.Models;

public class ProductGroup : List<Product>
{
    public string CategoryName { get; }
    public int ItemCount => Count;

    public ProductGroup(string categoryName, List<Product> products) : base(products)
    {
        CategoryName = categoryName;
    }
}
