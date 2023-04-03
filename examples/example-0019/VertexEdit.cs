namespace example;

public class VertexEdit : INotifyPropertyChanged
{
    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    Func<Vector3> getter;
    Action<Vector3> setter;

    #region X

    private float _X = 0;
    /// <summary>
    /// X
    /// </summary>
    public float X
    {
        get => _X;
        set
        {
            var changed = value != _X;
            if (changed)
            {
                _X = value;
                Write();
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Y

    private float _Y = 0;
    /// <summary>
    /// Y
    /// </summary>
    public float Y
    {
        get => _Y;
        set
        {
            var changed = value != _Y;
            if (changed)
            {
                _Y = value;
                Write();
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Z

    private float _Z = 0;
    /// <summary>
    /// Z
    /// </summary>
    public float Z
    {
        get => _Z;
        set
        {
            var changed = value != _Z;
            if (changed)
            {
                _Z = value;
                Write();
                OnPropertyChanged();
            }
        }
    }

    #endregion

    void Read()
    {
        var v = getter();
        X = v.X;
        Y = v.Y;
        Z = v.Z;
    }

    void Write()
    {
        setter(new Vector3(X, Y, Z));
    }

    public VertexEdit(Func<Vector3> getter, Action<Vector3> setter)
    {
        this.getter = getter;
        this.setter = setter;

        Read();
    }

}
