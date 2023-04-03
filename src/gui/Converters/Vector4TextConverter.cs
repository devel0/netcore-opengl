namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Converter to allow text editing of vector4 components.
/// </summary>
public class Vector4TextConverter : IValueConverter
{

    #region Instance
    static Vector4TextConverter? _Instance = null;
    public static Vector4TextConverter Instance
    {
        get
        {
            if (_Instance is null) _Instance = new Vector4TextConverter();
            return _Instance;
        }
    }
    #endregion    

    string Sep;

    public Vector4TextConverter(string sep = ",")
    {
        Sep = sep;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Vector4 v)
        {
            if (parameter is string digitsStr && int.TryParse(digitsStr, out var digits))
                return Invariant($"{Round(v.X, digits)}{Sep}{Round(v.Y, digits)}{Sep}{Round(v.Z, digits)}{Sep}{Round(v.W, digits)}");
            else
                return Invariant($"{v.X}{Sep}{v.Y}{Sep}{v.Z}{Sep}{v.W}");
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            var ss = str.Split(Sep);
            return new Vector4(
                float.Parse(ss[0], CultureInfo.InvariantCulture),
                float.Parse(ss[1], CultureInfo.InvariantCulture),
                float.Parse(ss[2], CultureInfo.InvariantCulture),
                float.Parse(ss[3], CultureInfo.InvariantCulture));
        }
        return null;
    }
}
