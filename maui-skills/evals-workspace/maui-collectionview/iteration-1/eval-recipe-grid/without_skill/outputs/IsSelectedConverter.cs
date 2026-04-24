using System.Globalization;
using RecipeApp.Models;

namespace RecipeApp.Converters;

/// <summary>
/// Accepts a two-element array [ currentItem, selectedItem ] and returns
/// a Color that is used as the Border.Stroke value.
/// Returns Colors.DodgerBlue when the items are the same object, otherwise
/// Colors.Transparent — creating a visible highlight border on the selected card.
/// </summary>
public class IsSelectedConverter : IMultiValueConverter
{
    public static readonly IsSelectedConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [Recipe current, Recipe selected] && ReferenceEquals(current, selected))
            return Colors.DodgerBlue;

        return Colors.Transparent;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
