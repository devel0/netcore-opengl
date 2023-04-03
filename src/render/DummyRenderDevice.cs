namespace SearchAThing.OpenGL.Render;

/// <summary>
/// A render device that doesn't write GL pixel anywhere.<br/>
/// This object can be used for testing purpouse.
/// </summary>
public class DummyRenderDevice : IRenderDevice, INotifyPropertyChanged
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

    #region Size

    private Size _Size;
    
    public Size Size
    {
        get => _Size;
        set
        {
            var changed = value != _Size;
            if (changed)
            {
                _Size = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region TransferGLPixelsCount
    
    private int _TransferGLPixelsCount = 0;
    
    public int TransferGLPixelsCount
    {
        get => _TransferGLPixelsCount;
        set
        {
             var changed = value != _TransferGLPixelsCount;
             if (changed)
             {
                 _TransferGLPixelsCount = value;
                 OnPropertyChanged();
             }
        }
    }
    
    #endregion    
    
    public DummyRenderDevice(Size size)
    {
        Size = size;
    }

    public void Post(Action action) => action();

    bool transferringGLPixels = false;

    public void TransferGLPixels(GL GL)
    {
        if (transferringGLPixels || Size.IsEmpty) return;

        transferringGLPixels = true;
        
        ++TransferGLPixelsCount;
        
        transferringGLPixels = false;
    }

}