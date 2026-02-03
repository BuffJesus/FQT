using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

// Alias to resolve ambiguity with System.Drawing (WinForms)
using WpfColor = System.Windows.Media.Color;
using WpfColorConverter = System.Windows.Media.ColorConverter;
using WpfPoint = System.Windows.Point;

namespace FableQuestTool.Converters;

/// <summary>
/// Converts a hex color string (e.g., "#FF0000") to a SolidColorBrush
/// </summary>
public class StringToColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            try
            {
                var color = (WpfColor)WpfColorConverter.ConvertFromString(colorString);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("StringToColorBrushConverter is one-way only.");
    }
}

/// <summary>
/// Converts a hex color string to a Color object
/// </summary>
public class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string colorString && !string.IsNullOrEmpty(colorString))
        {
            try
            {
                return (WpfColor)WpfColorConverter.ConvertFromString(colorString);
            }
            catch
            {
                return Colors.Gray;
            }
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("StringToColorConverter is one-way only.");
    }
}

/// <summary>
/// Creates a linear gradient brush from two color strings for UE5-style node headers
/// </summary>
public class HeaderGradientConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is string startColor && values[1] is string endColor)
        {
            try
            {
                var color1 = (WpfColor)WpfColorConverter.ConvertFromString(startColor);
                var color2 = (WpfColor)WpfColorConverter.ConvertFromString(endColor);

                var brush = new LinearGradientBrush
                {
                    StartPoint = new WpfPoint(0, 0),
                    EndPoint = new WpfPoint(0, 1)
                };
                brush.GradientStops.Add(new GradientStop(color1, 0.0));
                brush.GradientStops.Add(new GradientStop(color2, 1.0));
                return brush;
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("HeaderGradientConverter is one-way only.");
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("NullToVisibilityConverter is one-way only.");
    }
}

public class InvertedNullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("InvertedNullToVisibilityConverter is one-way only.");
    }
}

public class NullToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("NullToBoolConverter is one-way only.");
    }
}

/// <summary>
/// Converts a Point to a Thickness for positioning elements via Margin
/// </summary>
public class PointToThicknessConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is WpfPoint point)
        {
            return new Thickness(point.X, point.Y, 0, 0);
        }
        return new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("PointToThicknessConverter is one-way only.");
    }
}

/// <summary>
/// Converts a node property type to a color brush for badges.
/// </summary>
public class PropertyTypeToColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string type || string.IsNullOrWhiteSpace(type))
        {
            return new SolidColorBrush(Colors.Gray);
        }

        return type switch
        {
            "bool" => new SolidColorBrush(Colors.IndianRed),
            "int" => new SolidColorBrush(Colors.CadetBlue),
            "float" => new SolidColorBrush(Colors.YellowGreen),
            "string" => new SolidColorBrush(Colors.HotPink),
            "text" => new SolidColorBrush(Colors.HotPink),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("PropertyTypeToColorBrushConverter is one-way only.");
    }
}

/// <summary>
/// Converts a node property type to a short badge label.
/// </summary>
public class PropertyTypeToLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string type || string.IsNullOrWhiteSpace(type))
        {
            return "UNKNOWN";
        }

        return type switch
        {
            "bool" => "BOOL",
            "int" => "INT",
            "float" => "FLOAT",
            "string" => "STRING",
            "text" => "STRING",
            _ => type.ToUpperInvariant()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("PropertyTypeToLabelConverter is one-way only.");
    }
}

/// <summary>
/// Compares two integers and returns true if they are equal.
/// </summary>
public class IntEqualsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
        {
            return false;
        }

        if (values[0] is int left && values[1] is int right)
        {
            return left == right;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("IntEqualsConverter is one-way only.");
    }
}

/// <summary>
/// Returns a wire offset for connectors, collapsing reroute anchors to the dot center.
/// </summary>
public class RerouteConnectorOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is FableQuestTool.ViewModels.ConnectorViewModel connector && connector.IsRerouteAnchor)
        {
            return new WpfPoint(0, 0);
        }

        return new WpfPoint(12, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("RerouteConnectorOffsetConverter is one-way only.");
    }
}
