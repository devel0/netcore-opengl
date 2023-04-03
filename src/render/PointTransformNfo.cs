namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Helper class used by <see cref="SearchAThing.OpenGL.GUI.GLDevTool"/>.<br/>
/// Given the vertex, size, gl matrixes it will update object, normal, local, world, eye, clip, screen transformed coordinates.
/// </summary>
public class PointTransformNfo : INotifyPropertyChanged
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

    #region Normal

    private Vector3 _Normal = Vector3.Zero;
    /// <summary>
    /// Normal
    /// </summary>
    public Vector3 Normal
    {
        get => _Normal;
        private set
        {
            var changed = value != _Normal;
            if (changed)
            {
                _Normal = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Object

    private Vector3 _Object = Vector3.Zero;
    /// <summary>
    /// Object
    /// </summary>
    public Vector3 Object
    {
        get => _Object;
        private set
        {
            var changed = value != _Object;
            if (changed)
            {
                _Object = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Local

    private Vector3 _Local = Vector3.Zero;
    /// <summary>
    /// Local
    /// </summary>
    public Vector3 Local
    {
        get => _Local;
        private set
        {
            var changed = value != _Local;
            if (changed)
            {
                _Local = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region World

    private Vector3 _World = Vector3.Zero;
    /// <summary>
    /// World
    /// </summary>
    public Vector3 World
    {
        get => _World;
        private set
        {
            var changed = value != _World;
            if (changed)
            {
                _World = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Eye

    private Vector3 _Eye = Vector3.Zero;
    /// <summary>
    /// Eye
    /// </summary>
    public Vector3 Eye
    {
        get => _Eye;
        private set
        {
            var changed = value != _Eye;
            if (changed)
            {
                _Eye = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Clip

    private Vector4 _Clip = Vector4.Zero;
    /// <summary>
    /// Clip
    /// </summary>
    public Vector4 Clip
    {
        get => _Clip;
        private set
        {
            var changed = value != _Clip;
            if (changed)
            {
                _Clip = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ClipW

    private Vector3 _ClipW = Vector3.Zero;
    /// <summary>
    /// ClipW
    /// </summary>
    public Vector3 NDC
    {
        get => _ClipW;
        private set
        {
            var changed = value != _ClipW;
            if (changed)
            {
                _ClipW = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Screen

    private Vector2 _Screen = Vector2.Zero;
    /// <summary>
    /// Screen
    /// </summary>
    public Vector2 Screen
    {
        get => _Screen;
        private set
        {
            var changed = value != _Screen;
            if (changed)
            {
                _Screen = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    public GLVertex Vertex { get; private set; }

    public GLControl? GLControl { get; private set; } = null;

    Matrix4x4 om, mm, vm, pm;

    Vector2 size; 

    public bool DeferredRecompute { get; private set; }

    public PointTransformNfo(GLControl glControl, GLVertex vertex, bool deferredRecompute = false)
    {
        DeferredRecompute = deferredRecompute;

        GLControl = glControl;
        Vertex = vertex;

        if (!DeferredRecompute)
            glControl.PropertyChanged += glControl_PropertyChanged;

        this.size = GLControl.Size();

        this.om = vertex.ParentFigure is null ? Matrix4x4.Identity : vertex.ParentFigure.ObjectMatrix;
        this.mm = GLControl.ModelMatrix;
        this.vm = GLControl.ViewMatrix;
        this.pm = GLControl.ProjectionMatrix;

        if (vertex.ParentFigure is not null)
        {
            vertex.ParentFigure.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(IGLFigure.ObjectMatrix) && sender is IGLFigure fig)
                {
                    this.om = fig.ObjectMatrix;
                    Recompute();
                }
            };
        }

        Recompute();
    }

    void Recompute()
    {
        var sw = size.X;
        var sh = size.Y;

        Normal = Vertex.Normal;
        Object = Vertex.Position;
        Local = ObjectToLocal(Vertex.Position, om);
        World = LocalToWorld(Local, mm);
        Eye = WordlToEye(World, vm);
        Clip = EyeToClip(Eye, pm);
        NDC = ClipToNDC(Clip);
        Screen = NDCToScreen(NDC, size);
    }

    private void glControl_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (GLControl is null) return;

        if (e.PropertyName == nameof(GLControl.ModelMatrix))
        {
            size = GLControl.Size();
            mm = GLControl.ModelMatrix;

            Recompute();
        }
        else if (e.PropertyName == nameof(GLControl.ViewMatrix))
        {
            size = GLControl.Size();
            vm = GLControl.ViewMatrix;

            Recompute();
        }
        else if (e.PropertyName == nameof(GLControl.ProjectionMatrix))
        {
            size = GLControl.Size();
            pm = GLControl.ProjectionMatrix;

            Recompute();
        }
    }

    public string Fmt(string prefix) =>
        Invariant($"{prefix}L:{Local.Round(3),-20} W:{World.Round(3),-25} E:{Eye.Round(3),-25} C:{Clip.Round(3),-40} N:{NDC.Round(3),-40} S:{Screen.Round(3)}");
}