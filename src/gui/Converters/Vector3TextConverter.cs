namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Converter to allow text editing of vector3 components.
/// </summary>
public class Vector3TextConverter : IValueConverter
{

    #region Instance
    static Vector3TextConverter? _Instance = null;
    public static Vector3TextConverter Instance
    {
        get
        {
            if (_Instance is null) _Instance = new Vector3TextConverter();
            return _Instance;
        }
    }
    #endregion    

    string Sep;

    public Vector3TextConverter(string sep = ",")
    {
        Sep = sep;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Vector3 v)
        {
            if (parameter is string digitsStr && int.TryParse(digitsStr, out var digits))
                return Invariant($"{Round(v.X, digits)}{Sep}{Round(v.Y, digits)}{Sep}{Round(v.Z, digits)}");
            else
                return Invariant($"{v.X}{Sep}{v.Y}{Sep}{v.Z}");
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            var ss = str.Split(Sep);
            return new Vector3(
                float.Parse(ss[0], CultureInfo.InvariantCulture),
                float.Parse(ss[1], CultureInfo.InvariantCulture),
                float.Parse(ss[2], CultureInfo.InvariantCulture));
        }
        return null;
    }
}
