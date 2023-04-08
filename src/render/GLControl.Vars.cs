namespace SearchAThing.OpenGL.Render;

public partial class GLControl : INotifyPropertyChanged
{

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        Device.Post(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));

    #endregion

    /// <summary>
    /// Gl context reference ( retrieved through gl model ).
    /// </summary>
    public GLContext GLContext => GLModel.GLContext;

    /// <summary>
    /// Depth only shader for shadow cube maps ( retrieved from gl context ).
    /// </summary>
    public GLPipeline DepthShader => GLContext.DepthShader;

    /// <summary>
    /// Main shader ( retrieved from gl context ).
    /// </summary>
    public GLPipeline MainShader => GLContext.MainShader;

    /// <summary>
    /// Geometry shader used if <see cref="GLControl.ShadeWithEdge"/> enabled ( retrieved from gl context ).
    /// </summary>
    public GLPipeline ShadeWithEdgeShader => GLContext.ShadeWithEdgeShader;

    /// <summary>
    /// Geometry shader used if <see cref="GLControl.ShowNormals"/> enabled ( retrieved from gl context ).
    /// </summary>
    public GLPipeline NormalShader => GLContext.NormalShader;

    internal GL GL => GLContext.GL;

    static int IDs = 0;

    uint? main_fb = null;
    uint? render_color = null;
    uint? render_depth = null;

    uint? shadow_fb = null;
    uint? shadow_texture = null;

    internal Size? glControlLastKnownSize = null;

    #region ID

    private string _ID = "";
    /// <summary>
    /// unique control ID
    /// </summary>
    public string ID
    {
        get => _ID;
        private set
        {
            var changed = value != _ID;
            if (changed)
            {
                _ID = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region RenderCount

    private int _RenderCount = 0;
    /// <summary>
    /// Stats the count of completed <see cref="GLControl.Render"/> calls.
    /// </summary>
    public int RenderCount
    {
        get => _RenderCount;
        private set
        {
            var changed = value != _RenderCount;
            if (changed)
            {
                _RenderCount = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Model

    private GLModel _Model;
    /// <summary>
    /// Gl model associated to this control.
    /// </summary>
    public GLModel GLModel
    {
        get => _Model;
        private set
        {
            var changed = value != _Model;
            if (changed)
            {
                _Model = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ModelMatrix

    private Matrix4x4 _ModelMatrix = Matrix4x4.Identity;
    /// <summary>
    /// ModelMatrix [local] of this control.
    /// </summary>
    /// <seealso cref="CreateModelMatrix"/>.
    public Matrix4x4 ModelMatrix
    {
        get => _ModelMatrix;
        set
        {
            var changed = value != _ModelMatrix;
            if (changed)
            {
                _ModelMatrix = value;
                OnPropertyChanged();
                ModelMatrixFmt = _ModelMatrix.Fmt(3);
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region ModelMatrixFmt

    private string? _ModelMatrixFmt = null;
    /// <summary>
    /// Formatted version of model matrix.
    /// </summary>
    public string? ModelMatrixFmt
    {
        get
        {
            if (_ModelMatrixFmt is null) _ModelMatrixFmt = _ModelMatrix.Fmt(3);
            return _ModelMatrixFmt;
        }
        private set
        {
            var changed = value != _ModelMatrixFmt;
            if (changed)
            {
                _ModelMatrixFmt = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region ViewMatrix

    private Matrix4x4? _ViewMatrix = null;
    /// <summary>
    /// ViewMatrix [world] of this control.
    /// </summary>
    /// <seealso cref="CreateViewMatrix"/>.
    public Matrix4x4 ViewMatrix
    {
        get
        {
            if (_ViewMatrix is null)
            {
                CreateViewMatrix(CameraPos, CameraTarget, CameraUp, out var vm);
                _ViewMatrix = vm;
                ViewMatrixFmt = _ViewMatrix.Value.Fmt(3);
            }
            return _ViewMatrix.Value;
        }
        set
        {
            var changed = value != _ViewMatrix;
            if (changed)
            {
                _ViewMatrix = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region ViewMatrixFmt

    private string? _ViewMatrixFmt = null;
    /// <summary>
    /// Formatted version of view matrix.
    /// </summary>
    public string? ViewMatrixFmt
    {
        get
        {
            return _ViewMatrixFmt;
        }
        private set
        {
            var changed = value != _ViewMatrixFmt;
            if (changed)
            {
                _ViewMatrixFmt = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region CameraPos

    private Vector3 _CameraPos = DEFAULT_CAMERA_POS;
    /// <summary>
    /// CameraPos [world].
    /// </summary>
    public Vector3 CameraPos
    {
        get => _CameraPos;
        set
        {
            var changed = value != _CameraPos;
            if (changed)
            {
                _CameraPos = value;
                _ViewMatrix = null;
                OnPropertyChanged(nameof(ViewMatrix));
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region CameraTarget

    private Vector3 _CameraTarget = DEFAULT_CAMERA_TARGET;
    /// <summary>
    /// CameraTarget [world].
    /// </summary>
    public Vector3 CameraTarget
    {
        get => _CameraTarget;
        set
        {
            var changed = value != _CameraTarget;
            if (changed)
            {
                _CameraTarget = value;
                _ViewMatrix = null;
                OnPropertyChanged(nameof(ViewMatrix));
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion  

    #region CameraUp

    private Vector3 _CameraUp = DEFAULT_CAMERA_UP;
    /// <summary>
    /// CameraUp [world]
    /// </summary>
    public Vector3 CameraUp
    {
        get => _CameraUp;
        set
        {
            var changed = value != _CameraUp;
            if (changed)
            {
                _CameraUp = value;
                _ViewMatrix = null;
                OnPropertyChanged(nameof(ViewMatrix));
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region ProjectionMatrix

    private Matrix4x4? _ProjectionMatrix = null;
    /// <summary>
    /// ProjectionMatrix [eye].    
    /// </summary>
    /// <seealso cref="CreatePerspectiveProjectionMatrix"/><br/>
    /// <seealso cref="CreateOrthogonalProjectionMatrix"/><br/>
    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            Matrix4x4 res;

            var changed = false;

            if (_ProjectionMatrix is null)
            {
                if (Perspective)
                {
                    CreatePerspectiveProjectionMatrix(FovDeg, Device.Size.AspectRatio(), Near, Far, out var pm);
                    res = pm;
                }

                else
                {
                    CreateOrthogonalProjectionMatrix(Size(), OrthoZoom, Near, Far, ViewMatrix, out var pm);
                    res = pm;
                }

                changed = true;

                _ProjectionMatrix = res;
            }
            else
                res = _ProjectionMatrix.Value;

            if (changed)
            {
                OnPropertyChanged();
                ProjectionMatrixFmt = res.Fmt(3);
            }

            return res;
        }
        set
        {
            var changed = value != _ProjectionMatrix;
            if (changed)
            {
                _ProjectionMatrix = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InvalidateProjectionMatrix()
    {
        _ProjectionMatrix = null;
        _ScreenProjectionMatrix = null;
        _ScreenModelMatrix = null;
    }

    #endregion

    #region ProjectionMatrixFmt

    private string? _ProjectionMatrixFmt = null;
    /// <summary>
    /// Formatted version of project matrix.
    /// </summary>
    public string? ProjectionMatrixFmt
    {
        get
        {
            return _ProjectionMatrixFmt;
        }
        private set
        {
            var changed = value != _ProjectionMatrixFmt;
            if (changed)
            {
                _ProjectionMatrixFmt = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ScreenProjectionMatrix

    private Matrix4x4? _ScreenProjectionMatrix = null;
    /// <summary>
    /// ScreenProjectionMatrix [eye].
    /// </summary>
    /// <seealso cref="IGLFigure.ScreenCoordMode"/>
    public Matrix4x4 ScreenProjectionMatrix
    {
        get
        {
            if (_ScreenProjectionMatrix is null) RebuildScreenProjectionMatrix();

            return _ScreenProjectionMatrix!.Value;
        }
        set
        {
            var changed = value != _ScreenProjectionMatrix;
            if (changed)
            {
                _ScreenProjectionMatrix = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Matrix4x4 BuildScreenProjectMatrix()
    {
        var bbox = GLModel.LBBox;
        if (!bbox.IsEmpty)
        {
            var size = Device.Size;

            var sw = size.Width + SCREEN_ADJUST_W;
            var sh = size.Height + SCREEN_ADJUST_H;

            return Matrix4x4.CreateOrthographic((float)sw, (float)sh, -1, 1);
        }
        else
            return Matrix4x4.Identity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RebuildScreenProjectionMatrix()
    {
        // var ar = Bounds.Size.AspectRatio;

        _ScreenProjectionMatrix = BuildScreenProjectMatrix();
    }

    #endregion

    #region ScreenModelMatrix

    private Matrix4x4? _ScreenModelMatrix = null;
    /// <summary>
    /// Screen model matrix [world].
    /// </summary>
    /// <seealso cref="IGLFigure.ScreenCoordMode"/>
    public Matrix4x4 ScreenModelMatrix
    {
        get
        {
            if (_ScreenModelMatrix is null) RebuildScreenModelMatrix();

            return _ScreenModelMatrix!.Value;
        }
        set
        {
            var changed = value != _ScreenModelMatrix;
            if (changed)
            {
                _ScreenModelMatrix = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Matrix4x4 BuildScreenModelMatrix()
    {
        var bbox = GLModel.LBBox;
        if (!bbox.IsEmpty)
        {
            var size = Size();

            var sw = size.X;
            var sh = size.Y;

            return
                Matrix4x4.CreateTranslation(new Vector3(
                    (float)Round(-sw / 2) + SCREEN_ADJUST_TX,
                    (float)Round(-sh / 2) + SCREEN_ADJUST_TY,
                    0));
        }
        else
            return Matrix4x4.Identity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void RebuildScreenModelMatrix()
    {
        _ScreenModelMatrix = BuildScreenModelMatrix();
    }

    #endregion

    #region AutoAdjustNearFar

    private bool _AutoAdjustNearFar = true;
    /// <summary>
    /// Auto adjust near,far to prevent z-fighting.
    /// </summary>
    /// <seealso cref="PreventZFighting"/>.
    public bool AutoAdjustNearFar
    {
        get => _AutoAdjustNearFar;
        set
        {
            var changed = value != _AutoAdjustNearFar;
            if (changed)
            {
                _AutoAdjustNearFar = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Perspective

    private bool _Perspective = DEFAULT_Perspective;
    /// <summary>
    /// If true perspective projection matrix will be used, orthogonal projection matrix elsewhere. (default:true)
    /// </summary>
    /// <seealso cref="CreatePerspectiveProjectionMatrix"/>
    /// <seealso cref="CreateOrthogonalProjectionMatrix"/>
    public bool Perspective
    {
        get => _Perspective;
        set
        {
            var changed = value != _Perspective;
            if (changed)
            {
                _Perspective = value;
                OnPropertyChanged();
                _ViewMatrix = null;
                _ProjectionMatrix = null;
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region OrthoZoom

    private float _OrthoZoom = DEFAULT_OrthoZoom;
    /// <summary>
    /// Ortho zoom [eye] factor.
    /// </summary>
    /// <seealso cref="CreateOrthogonalProjectionMatrix"/>
    public float OrthoZoom
    {
        get => _OrthoZoom;
        set
        {
            var changed = value != _OrthoZoom;
            if (changed)
            {
                _OrthoZoom = value;
                OnPropertyChanged();
                _ProjectionMatrix = null;
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region FovDeg

    private float _FovDeg = DEFAULT_FovDeg;
    /// <summary>
    /// Perspective field of view [world] (deg).
    /// </summary>
    /// <seealso cref="CreatePerspectiveProjectionMatrix"/>
    public float FovDeg
    {
        get => _FovDeg;
        set
        {
            var changed = value != _FovDeg;
            if (changed)
            {
                _FovDeg = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region Near

    private float _Near = DEFAULT_Near;
    /// <summary>
    /// Near clipping plane distance [world].
    /// </summary>
    /// <seealso cref="CreatePerspectiveProjectionMatrix"/>
    /// <seealso cref="PreventZFighting"/>
    public float Near
    {
        get => _Near;
        set
        {
            var changed = value != _Near;
            if (changed)
            {
                InvalidateProjectionMatrix();

                _Near = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region Far

    private float _Far = DEFAULT_Far;
    /// <summary>
    /// Far clipping plane distance [world].
    /// </summary>
    /// <seealso cref="CreatePerspectiveProjectionMatrix"/>
    /// <seealso cref="PreventZFighting"/>
    public float Far
    {
        get => _Far;
        set
        {
            var changed = value != _Far;
            if (changed)
            {
                InvalidateProjectionMatrix();

                _Far = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region Wireframe

    private bool _Wireframe = DEFAULT_Wireframe;
    /// <summary>
    /// If true triangles will be displayed using lines, shaded elsewhere (default:false).
    /// </summary>
    public bool Wireframe
    {
        get => _Wireframe;
        set
        {
            var changed = value != _Wireframe;
            if (changed)
            {
                _Wireframe = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region UseTexture

    private bool _UseTexture = DEFAULT_UseTexture;
    /// <summary>
    /// If true triangles with texture will display the texture, empty elsewhere. (default:true)
    /// </summary>
    public bool UseTexture
    {
        get => _UseTexture;
        set
        {
            var changed = value != _UseTexture;
            if (changed)
            {
                _UseTexture = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region UseShadow

    private bool _UseShadow = DEFAULT_UseShadows;
    /// <summary>
    /// If true and at least one point light exists in the gl model shadows are dropped, no shadow elsewhere (default:true).
    /// </summary>
    public bool UseShadow
    {
        get => _UseShadow;
        set
        {
            var changed = value != _UseShadow;
            if (changed)
            {
                _UseShadow = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region Title
    
    private string _Title = "";
    /// <summary>
    /// Title
    /// </summary>
    public string Title
    {
        get => _Title;
        set
        {
             var changed = value != _Title;
             if (changed)
             {
                 _Title = value;
                 OnPropertyChanged();
             }
        }
    }
    
    #endregion
    #region ShowModelBBox

    private bool _ShowModelBBox = false;
    /// <summary>
    /// If true the model bbox as lines are displayed using a temporary vertex manager at runtime.
    /// </summary>
    public bool ShowModelBBox
    {
        get => _ShowModelBBox;
        set
        {
            var changed = value != _ShowModelBBox;
            if (changed)
            {
                _ShowModelBBox = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region ShowCameraObject

    private bool _ShowCameraObject = false;
    /// <summary>
    /// If true a green camera frustum object represented with lines are displayed using a temporary vertex manager at runtime.
    /// </summary>
    public bool ShowCameraObject
    {
        get => _ShowCameraObject;
        set
        {
            var changed = value != _ShowCameraObject;
            if (changed)
            {
                if (value)
                    GLModel.ShowCameraObjectControls.Add(this);
                else
                    GLModel.ShowCameraObjectControls.Remove(this);

                _ShowCameraObject = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region FrameRate

    private float _FrameRate = 0;
    /// <summary>
    /// Stats the number of render per sec given by latest rendered frames.
    /// </summary>
    public float FrameRate
    {
        get => _FrameRate;
        set
        {
            var changed = value != _FrameRate;
            if (changed)
            {
                _FrameRate = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LastRenderTime

    private float _LastRenderTime = 0;
    /// <summary>
    /// Time (sec) required for latest render.
    /// </summary>
    public float LastRenderTime
    {
        get => _LastRenderTime;
        set
        {
            var changed = value != _LastRenderTime;
            if (changed)
            {
                _LastRenderTime = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ShadeWithEdge

    private bool _ShadeWithEdge = DEFAULT_SHADE_WITH_EDGE;
    /// <summary>
    /// If true a geometry shader will generate addictional lines that marks triangle edges if displayed in shaded mode.
    /// </summary>
    public bool ShadeWithEdge
    {
        get => _ShadeWithEdge;
        set
        {
            var changed = value != _ShadeWithEdge;
            if (changed)
            {
                _ShadeWithEdge = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region ShowNormals

    private bool _ShowNormals = DEFAULT_SHOW_NORMALS;
    /// <summary>
    /// If true a geometry shader will create yellow vertex normals using a size specified by a factor of the bbox.
    /// </summary>
    /// <seealso cref="GLModel.NormalFactor"/>.
    public bool ShowNormals
    {
        get => _ShowNormals;
        set
        {
            var changed = value != _ShowNormals;
            if (changed)
            {
                _ShowNormals = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region ClearColor

    private Color _ClearColor = Color.Black;
    /// <summary>
    /// Default scene background color ( Default: black ).
    /// </summary>
    public Color ClearColor
    {
        get => _ClearColor;
        set
        {
            var changed = value != _ClearColor;
            if (changed)
            {
                _ClearColor = value;
                OnPropertyChanged();
                IsControlInvalidated = true;
            }
        }
    }

    #endregion

    #region PointerCoord

    private Vector2 _PointerCoord = Vector2.Zero;
    /// <summary>
    /// Actual pointer coord [screen]
    /// </summary>
    public Vector2 PointerCoord
    {
        get => _PointerCoord;
        set
        {
            var changed = value != _PointerCoord;
            if (changed)
            {
                _PointerCoord = value;
                OnPropertyChanged();
                _PointerLRayCast = null;
                OnPropertyChanged(nameof(PointerLRayCast));
            }
        }
    }

    #endregion

    #region PointerLRayCast

    private Line? _PointerLRayCast = null;
    /// <summary>
    /// Actual pointer raycast [local]
    /// </summary>
    public Line? PointerLRayCast
    {
        get
        {
            if (_PointerLRayCast is null)            
                _PointerLRayCast = RayCastLocal(PointerCoord);
            
            return _PointerLRayCast;
        }
    }

    #endregion

}