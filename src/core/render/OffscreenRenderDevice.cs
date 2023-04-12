using ImageMagick;

namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Render device for offscreen mode that acts as a bitmap file writer.
/// </summary>
public class OffscreenRenderDevice : IRenderDevice, INotifyPropertyChanged
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

    #region Pathfilename

    private string _Pathfilename;
    /// <summary>
    /// Pathfilename
    /// </summary>
    public string Pathfilename
    {
        get => _Pathfilename;
        set
        {
            var changed = value != _Pathfilename;
            if (changed)
            {
                _Pathfilename = value;
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

    /// <summary>
    /// Create an offscreen render device.
    /// </summary>
    /// <param name="pathfilename">Target bitmap pathfilename.</param>
    /// <param name="size">Size of image created.</param>
    public OffscreenRenderDevice(string pathfilename, in  Size size)
    {
        Size = size;
        _Pathfilename = pathfilename;
    }

    public void Post(Action action) => action();

    bool transferringGLPixels = false;

    //! [OffscreenTrasferGLPixels]
    public void TransferGLPixels(GL GL)
    {
        if (transferringGLPixels || Size.IsEmpty)
            return;

        transferringGLPixels = true;

        GL.PixelStore(PixelStoreParameter.PackRowLength, Size.Width);

        var buf = new byte[Size.Width * Size.Height * 4];

        unsafe
        {
            fixed (void* ptr = buf)
            {
                GL.ReadPixels(0, 0, (uint)Size.Width, (uint)Size.Height,
                    PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
            }
        }

        using (var image = new MagickImage())
        {
            image.ReadPixels(buf, new PixelReadSettings(Size.Width, Size.Height, StorageType.Char, PixelMapping.RGBA));

            image.Write(Pathfilename);
        }

        ++TransferGLPixelsCount;

        transferringGLPixels = false;

    }
    //! [OffscreenTrasferGLPixels]

}