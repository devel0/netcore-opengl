namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Converter Color to hex string
/// </summary>
public class ColorHexStringConverter : IValueConverter
{

    #region Instance    
    static ColorHexStringConverter? _Instance = null;
    public static ColorHexStringConverter Instance
    {
        get
        {
            if (_Instance is null) _Instance = new ColorHexStringConverter();
            return _Instance;
        }
    }
    #endregion    

    public ColorHexStringConverter()
    {
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color c)
            return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");

        throw new NotImplementedException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {        
        if (value is string str)
            return ColorTranslator.FromHtml(str);
            
        throw new NotImplementedException();
    }
}
