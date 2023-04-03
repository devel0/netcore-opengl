namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Wrap X,Y,Z components of an object containing a INotifyPropertyChanged vector3.
/// </summary>
public class Vector3Wrap : INotifyPropertyChanged
{

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    INotifyPropertyChanged container;
    string propname;

    Type containerType;
    PropertyInfo? propinfo;

    /// <summary>
    /// vector3 wrap constructor
    /// </summary>
    /// <param name="container">object that hold rvector3</param>
    /// <param name="propname">property name of the rvector3 in the given container</param>
    public Vector3Wrap(INotifyPropertyChanged container, string propname)
    {
        this.container = container;
        this.propname = propname;
        containerType = container.GetType();
        propinfo = containerType.GetProperty(propname);
        container.PropertyChanged += container_PropertyChanged;
    }

    /// <summary>
    /// track changes over container property and forward X,Y,Z property changed event
    /// </summary>    
    private void container_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == propname)
        {
            OnPropertyChanged("X");
            OnPropertyChanged("Y");
            OnPropertyChanged("Z");
        }
    }

    Vector3? V
    {
        get
        {
            if (propinfo is null) return null;
            var res = propinfo.GetMethod?.Invoke(container, null);
            if (res is Vector3 v) return v;
            return null;
        }
        set
        {
            if (propinfo is not null)
                propinfo.SetMethod?.Invoke(container, new[] { (object?)value });
        }
    }

    #region X

    float _X
    {
        get
        {
            if (V is null) return 0;
            return V.Value.X;
        }
    }

    /// <summary>
    /// X
    /// </summary>
    public float X
    {
        get => _X;
        set
        {
            var changed = value != _X;
            if (changed && V is not null)
            {
                var cur = V.Value;
                V = cur with { X = value };

                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Y

    float _Y
    {
        get
        {
            if (V is null) return 0;
            return V.Value.Y;
        }
    }

    /// <summary>
    /// Y
    /// </summary>
    public float Y
    {
        get => _Y;
        set
        {
            var changed = value != _Y;
            if (changed && V is not null)
            {
                var cur = V.Value;
                V = cur with { Y = value };

                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Z

    float _Z
    {
        get
        {
            if (V is null) return 0;
            return V.Value.Z;
        }
    }

    /// <summary>
    /// Z
    /// </summary>
    public float Z
    {
        get => _Z;
        set
        {
            var changed = value != _Z;
            if (changed && V is not null)
            {
                var cur = V.Value;
                V = cur with { Z = value };

                OnPropertyChanged();
            }
        }
    }

    #endregion

}
