namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Converter Color to Avalonia.Media.Color
/// </summary>
public class AColorConverter : IValueConverter
{

    #region Instance    
    static AColorConverter? _Instance = null;
    public static AColorConverter Instance
    {
        get
        {
            if (_Instance is null) _Instance = new AColorConverter();
            return _Instance;
        }
    }
    #endregion    

    public AColorConverter()
    {
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color c)
            return new Avalonia.Media.Color(c.A, c.R, c.G, c.B);

        throw new NotImplementedException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Avalonia.Media.Color acolor && targetType == typeof(Color))
            return Color.FromArgb((int)acolor.ToUint32());

        throw new NotImplementedException();
    }
}
