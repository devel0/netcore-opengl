namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Converter that create a <see cref="Vector3Wrap"/> vector3 wrapper that can be used for interactive editing
/// of vector3 components.
/// </summary>
public class Vector3EditConverter : IValueConverter
{

    #region Instance
    static Vector3EditConverter? _Instance = null;
    public static Vector3EditConverter Instance
    {
        get
        {
            if (_Instance is null) _Instance = new Vector3EditConverter();
            return _Instance;
        }
    }
    #endregion

    public Vector3EditConverter()
    {
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is INotifyPropertyChanged container && parameter is string path)
        {
            var wrap = new Vector3Wrap(container, path);
            return wrap;
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
