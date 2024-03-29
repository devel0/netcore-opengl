namespace SearchAThing.OpenGL.Core;

public class GLVertex : IGLVertex
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

    public GLVertexStruct ToStruct() => new GLVertexStruct
    {
        Position = Position,
        Normal = EffectiveNormal,
        MatColor = MaterialColor,
        MatProp = MaterialProperties,
        TextureST = TextureST,
        Flags = (uint)Flags
    };

    public uint? Index { get; internal set; }

    IGLPrimitive? _ParentPrimitive = null;
    public IGLPrimitive? ParentPrimitive
    {
        get
        {
            return _ParentPrimitive;
        }
        internal set
        {
            _ParentPrimitive = value;
        }
    }

    public IGLFigure? ParentFigure => ParentPrimitive?.ParentFigure;

    public GLVertexManager? ParentVertexManager => ParentFigure?.ParentVertexManager;

    #region Position

    private Vector3 _Position = DEFAULT_VertexPosition;

    public Vector3 Position
    {
        get => _Position;
        set
        {
            var changed = value != _Position;
            if (changed)
            {
                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.RemoveVertex(this);

                _Position = value;

                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.AddVertex(this);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ScreenCoordMode

    public bool ScreenCoordMode
    {
        get => ParentFigure is not null ? ParentFigure.ScreenCoordMode : false;
    }

    #endregion

    #region Color

    private Vector4 _MaterialColor = DEFAULT_MaterialColor.ToVector4();

    public Vector4 MaterialColor
    {
        get => _MaterialColor;
        set
        {
            var changed = value != _MaterialColor;
            if (changed)
            {
                ParentVertexManager?.RemoveVertex(this);

                _MaterialColor = value;

                ParentVertexManager?.AddVertex(this);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Properties

    private Vector3 _MaterialProperties = DEFAULT_MaterialProperties.ToVector3();

    public Vector3 MaterialProperties
    {
        get => _MaterialProperties;
        set
        {
            var changed = value != _MaterialProperties;
            if (changed)
            {
                ParentVertexManager?.RemoveVertex(this);

                _MaterialProperties = value;

                ParentVertexManager?.AddVertex(this);

                OnPropertyChanged();
            }
        }
    }

    #endregion 

    #region Normal

    private Vector3 _Normal = DEFAULT_Normal;

    public Vector3 Normal
    {
        get => _Normal;
        set
        {
            var changed = value != _Normal;
            if (changed)
            {
                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.RemoveVertex(this);

                _Normal = value;

                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.AddVertex(this, computeNormal: false);

                OnPropertyChanged();

                OnPropertyChanged(nameof(EffectiveNormal));
            }
        }
    }

    internal void SetNormal(in Vector3 normal)
    {
        var changed = normal != _Normal;

        if (changed)
        {
            _Normal = normal;

            OnPropertyChanged(nameof(Normal));

            OnPropertyChanged(nameof(EffectiveNormal));
        }
    }

    #endregion

    #region InvertNormal

    private bool _InvertNormal = false;

    public bool InvertNormal
    {
        get => _InvertNormal;
        set
        {
            var changed = value != _InvertNormal;
            if (changed)
            {
                _InvertNormal = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(EffectiveNormal));
            }
        }
    }

    #endregion

    #region EffectiveNormal

    private Vector3 _EffectiveNormal = DEFAULT_Normal;

    public Vector3 EffectiveNormal
    {
        get
        {
            return InvertNormal ? -Normal : Normal;
        }
    }

    #endregion

    #region TextureST

    private Vector2 _TextureST = DEFAULT_TexCoord;

    public Vector2 TextureST
    {
        get => _TextureST;
        set
        {
            var changed = value != _TextureST;
            if (changed)
            {
                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.RemoveVertex(this);

                _TextureST = value;

                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.AddVertex(this);

                OnPropertyChanged();
            }
        }
    }

    #endregion    

    #region Flags

    private GLVertexFlag _Flags = 0;

    /// <summary>
    /// Vertex flags can used to switch some vertex feature.<br/>
    /// By default no flags are active.
    /// <seealso cref="GLVertexFlag"/>
    /// </summary>
    [JsonProperty]
    public GLVertexFlag Flags
    {
        get => _Flags;
        set
        {
            var changed = value != _Flags;
            if (changed)
            {
                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.RemoveVertex(this);

                _Flags = value;

                if (ParentFigure is not null)
                    ((GLVertexManager?)ParentVertexManager)?.AddVertex(this);

                OnPropertyChanged();
            }
        }
    }

    #endregion    

    public string Signature =>
        Invariant($"{Position}_{(ScreenCoordMode ? 1 : 0)}_{MaterialColor}_{MaterialProperties}_{Normal}_{TextureST}_{(long)Flags}");

    /// <summary>
    /// Create a gl vertex ( default 0,0,0 ) [object].
    /// </summary>
    public GLVertex()
    {
    }

    /// <summary>
    /// Create a gl vertex with given position [object].
    /// </summary>
    /// <param name="position">Vertex position [object].</param>
    /// <param name="rgbaColor">(Optional) Vertex color encoded in vector4 (range 0..1) as (red, green, blue, alpha).<br/>Alpha=1 (full opaque).<br/>Alpha=0 (full transparent). (Default: white).</param>
    /// <param name="materialProperties">(Optional) Describe the strength (0..1) of the ambient, diffuse, specular light effects. (Default: ambient:0.8, diffuse:0.6, strength:0.6).</param>
    /// <param name="normal">(Optional) Vertex normal [object]. (Default: zero).</param>
    /// <param name="textureST">(Optional) Vertex texture mapping in the range [0,0]...[1,1].</param>
    public GLVertex(in Vector3 position,
        in Vector4? rgbaColor = null,
        in Vector3? materialProperties = null,
        in Vector3? normal = null,
        in Vector2? textureST = null)
    {
        Position = position;
        MaterialColor = rgbaColor is null ? DEFAULT_MaterialColor.ToVector4() : rgbaColor.Value;
        MaterialProperties = materialProperties is null ? DEFAULT_MaterialProperties.ToVector3() : materialProperties.Value;
        Normal = normal is null ? DEFAULT_Normal : normal.Value;
        TextureST = textureST is null ? DEFAULT_TexCoord : textureST.Value;
    }

    /// <summary>
    /// Create a gl vertex with given position [object] and color.
    /// </summary>
    /// <param name="position">Vertex position [object].</param>
    /// <param name="color">Vertex color.</param>    
    public GLVertex(in Vector3 position, in Color? color) : this(position, color?.ToVector4())
    {
    }

    /// <summary>
    /// Create a gl vertex with given position [object] and texture coord.
    /// </summary>
    /// <param name="position">Vector position [object].</param>
    /// <param name="textureST">Texture position in the ranage [0,0]...[1,1].</param>    
    public GLVertex(in Vector3 position, in Vector2 textureST) : this(position, null, null, null, textureST)
    {
    }

    public bool IsAttached => ParentVertexManager is not null;

    public IGLVertexManagerObject Copy() => new GLVertex
    {
        Position = Position,
        MaterialColor = MaterialColor,
        MaterialProperties = MaterialProperties,
        Normal = Normal,
        TextureST = TextureST
    };

    /// <summary>
    /// Mirror this vertex to the other side of given xy plane.
    /// </summary>
    /// <param name="refXYPlane">Reference xy plane.</param>
    /// <returns>Copy of this vertex, mirrored.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GLVertex? Mirror(in Matrix4x4 refXYPlane)
    {
        var qprj = Position.Project(refXYPlane);
        if (qprj is null) return null;

        var vtx = (GLVertex)Copy();
        vtx.Position = qprj.Value + (qprj.Value - vtx.Position);
        vtx.InvertNormal = !vtx.InvertNormal;

        return vtx;
    }

    public override string ToString() => Invariant($"{Position}");

}

public static partial class Ext
{

    /// <summary>
    /// Test given flags mask.
    /// </summary>
    /// <param name="vertex">Vertex which test flags.</param>
    /// <param name="mask">Flags to test.</param>
    /// <returns>True if all mask bits are set.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TestFlags(this GLVertex vertex, GLVertexFlag mask) => (vertex.Flags & mask) == mask;

    /// <summary>
    /// Set given flags mask.
    /// </summary>
    /// <param name="vertex">Vertex which sets flags.</param>
    /// <param name="mask">Flags to set.</param>
    /// <returns>This vertex.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static GLVertex SetFlags(this GLVertex vertex, GLVertexFlag mask)
    {
        vertex.Flags |= mask;
        return vertex;
    }

    /// <summary>
    /// Clear given flags mask.
    /// </summary>
    /// <param name="vertex">Vertex which clear flags.</param>
    /// <param name="mask">Flags to clear.</param>
    /// <returns>This vertex.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static GLVertex ClearFlags(this GLVertex vertex, GLVertexFlag mask)
    {
        vertex.Flags &= ~mask;
        return vertex;
    }

    /// <summary>
    /// Toggle given flags mask.
    /// </summary>
    /// <param name="vertex">Vertex which toggle flags.</param>
    /// <param name="mask">Flags to toggle.</param>
    /// <returns>This vertex.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static GLVertex ToggleFlags(this GLVertex vertex, GLVertexFlag mask)
    {
        vertex.Flags ^= mask;
        return vertex;
    }

}

public static partial class Constants
{

    /// <summary>
    /// (Default) vertex position (0,0,0).
    /// </summary>
    public static readonly Vector3 DEFAULT_VertexPosition = Vector3.Zero;

    /// <summary>
    /// (Default) vertex normal (0,0,0).
    /// </summary>
    public static readonly Vector3 DEFAULT_Normal = Vector3.Zero;

    /// <summary>
    /// (Default) vertex texture mapping coordinate (0,0).
    /// </summary>
    public static readonly Vector2 DEFAULT_TexCoord = Vector2.Zero;



}