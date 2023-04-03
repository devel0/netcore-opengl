namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Converter to allow text editing of vector2 components.
/// </summary>
public class Vector2TextConverter : IValueConverter
{

    #region Instance    
    static Vector2TextConverter? _Instance = null;
    public static Vector2TextConverter Instance
    {
        get
        {
            if (_Instance is null) _Instance = new Vector2TextConverter();
            return _Instance;
        }
    }
    #endregion

    string Sep;

    public Vector2TextConverter(string sep = ",")
    {
        Sep = sep;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Vector2 v)
        {
            if (parameter is string digitsStr && int.TryParse(digitsStr, out var digits))
                return Invariant($"{Round(v.X, digits)}{Sep}{Round(v.Y, digits)}");
            else
                return Invariant($"{v.X}{Sep}{v.Y}");
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            var ss = str.Split(Sep);
            return new Vector2(
                float.Parse(ss[0], CultureInfo.InvariantCulture),
                float.Parse(ss[1], CultureInfo.InvariantCulture));
        }
        return null;
    }
}
