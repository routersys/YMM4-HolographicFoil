using System.Globalization;
using System.Windows;
using System.Windows.Data;
using YukkuriMovieMaker.ItemEditor;

namespace HolographicFoil;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class ManualAngleVisibleAttribute : Attribute, ICustomVisibilityAttribute2
{
    public Binding GetBinding(object item, object propertyOwner) => new(nameof(HolographicFoilEffect.ViewSource))
    {
        Source = item,
        Converter = new EnumVisibilityConverter(HolographicFoilViewSource.Manual)
    };
}

[AttributeUsage(AttributeTargets.Property)]
internal sealed class AutomaticAngleVisibleAttribute : Attribute, ICustomVisibilityAttribute2
{
    public Binding GetBinding(object item, object propertyOwner) => new(nameof(HolographicFoilEffect.ViewSource))
    {
        Source = item,
        Converter = new EnumVisibilityConverter(HolographicFoilViewSource.Manual, true)
    };
}

[AttributeUsage(AttributeTargets.Property)]
internal sealed class AngleRevealVisibleAttribute : Attribute, ICustomVisibilityAttribute2
{
    public Binding GetBinding(object item, object propertyOwner) => new(nameof(HolographicFoilEffect.RevealMode))
    {
        Source = item,
        Converter = new EnumVisibilityConverter(HolographicFoilRevealMode.None, true)
    };
}

[AttributeUsage(AttributeTargets.Property)]
internal sealed class ImageAtlasVisibleAttribute : Attribute, ICustomVisibilityAttribute2
{
    public Binding GetBinding(object item, object propertyOwner) => new(nameof(HolographicFoilEffect.RevealMode))
    {
        Source = item,
        Converter = new EnumVisibilityConverter(HolographicFoilRevealMode.ImageAtlas)
    };
}

internal sealed class EnumVisibilityConverter(object expected, bool invert = false) : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var equals = Equals(value, expected);
        return equals ^ invert ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
