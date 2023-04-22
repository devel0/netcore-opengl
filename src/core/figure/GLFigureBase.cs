namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Base abstract class for point, line, triangle figures.
/// </summary>
public abstract class GLFigureBase : IGLFigure
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

    #region IGLVertexManagerObject

    GLVertexManager? _ParentVertexManager = null;

    public GLVertexManager? ParentVertexManager
    {
        get => _ParentVertexManager;
        internal set => _ParentVertexManager = value;
    }

    /// <summary>
    /// Create an instance of derived type.
    /// </summary>    
    /// <seealso cref="SearchAThing.OpenGL.Core.GLFigureBase.Copy"/>
    protected abstract GLFigureBase MakeInstance();

    /// <summary>
    /// Create a copy with all elements down the hierarchy until <see cref="GLFigureBase"/> included.
    /// </summary>    
    public GLFigureBase CopyBase()
    {
        var res = MakeInstance();

        res.ObjectMatrix = ObjectMatrix;
        res.Visible = Visible;
        res.Order = Order;
        res.Tag = Tag;

        return res;
    }

    protected abstract void CopySpecialized(GLFigureBase other);

    public IGLVertexManagerObject Copy()
    {
        var res = CopyBase();

        res.CopySpecialized(this);

        return res;
    }

    /// <summary>
    /// Create a mirrored figure against given xy plane.
    /// </summary>
    /// <param name="xyPlane">XY mirror plane.</param>
    /// <returns>Mirrored figure.</returns>
    public abstract GLFigureBase? Mirror(in Matrix4x4 xyPlane);

    #endregion

    #region IGLFigure

    public event FigureInvalidatedDelegate? FigureInvalidated;

    public abstract ReadOnlyObservableCollection<GLPrimitiveBase> Primitives { get; }

    public abstract GLPrimitiveType PrimitiveType { get; }

    #region ExcludeFromShadeWithEdge

    private bool _ExcludeFromShadeWithEdge = false;
    /// <summary>
    /// ExcludeFromShadeWithEdge
    /// </summary>
    public bool ExcludeFromShadeWithEdge
    {
        get => _ExcludeFromShadeWithEdge;
        set
        {
            var changed = value != _ExcludeFromShadeWithEdge;
            if (changed)
            {
                _ExcludeFromShadeWithEdge = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ObjectMatrixIsIdentity

    private bool? _ObjectMatrixIsIdentity = null;

    /// <summary>
    /// States if object matrix is an identity.
    /// </summary>    
    /// <remarks>
    /// This property caches Matrix4x4.IsIdentity information and keeps updated only upon effective object matrix change.
    /// </remarks>
    public bool ObjectMatrixIsIdentity
    {
        get
        {
            if (_ObjectMatrixIsIdentity is null)
                _ObjectMatrixIsIdentity = ObjectMatrix.IsIdentity;

            return _ObjectMatrixIsIdentity.Value;
        }
    }

    #endregion

    #region ObjectMatrix

    private Matrix4x4 _ObjectMatrix = DEFAULT_ObjectMatrix;

    public Matrix4x4 ObjectMatrix
    {
        get => _ObjectMatrix;
        set
        {
            var changed = value != _ObjectMatrix;
            if (changed)
            {
                _ObjectMatrix = value;
                OnPropertyChanged();
                _ObjectMatrixIsIdentity = null;
                OnPropertyChanged(nameof(ObjectMatrixIsIdentity));
                Invalidate();
            }
        }
    }

    #endregion

    #region Visible

    private bool _Visible = DEFAULT_FigureVisible;

    public bool Visible
    {
        get => _Visible;
        set
        {
            var changed = value != _Visible;
            if (changed)
            {
                _Visible = value;
                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    #endregion

    #region Order

    private int _Order = DEFAULT_FigureOrder;

    public int Order
    {
        get => _Order;
        set
        {
            var changed = value != _Order;
            if (changed)
            {
                _Order = value;
                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    #endregion

    #region Highlight

    private bool _Highlight = false;

    public bool Highlight
    {
        get => _Highlight;
        set
        {
            var changed = value != _Highlight;
            if (changed)
            {
                _Highlight = value;
                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    #endregion

    #region Alpha

    private float? _Alpha = null;

    public float? Alpha
    {
        get => _Alpha;
        set
        {
            var changed = value != _Alpha;
            if (changed)
            {
                _Alpha = value;
                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    #endregion

    #region Selected

    private bool _Selected = false;
    /// <summary>
    /// Selected.
    /// TODO: doc
    /// </summary>
    public bool Selected
    {
        get => _Selected;
        set
        {
            var changed = value != _Selected;
            if (changed)
            {
                _Selected = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ScreenCoord

    private bool _ScreenCoord = DEFAULT_ScreenCoord;

    public bool ScreenCoordMode
    {
        get => _ScreenCoord;
        set
        {
            var changed = value != _ScreenCoord;
            if (changed)
            {
                _ScreenCoord = value;
                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    #endregion

    #region Tag

    private object? _Tag = null;

    public object? Tag
    {
        get => _Tag;
        set
        {
            var changed = value != _Tag;
            if (changed)
            {
                _Tag = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region ExpandBBox

    private bool _ExpandBBox = true;

    public bool ExpandBBox
    {
        get => _ExpandBBox;
        set
        {
            var changed = value != _ExpandBBox;
            if (changed)
            {
                _ExpandBBox = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    public abstract string SimpleCmd();

    public IEnumerable<uint> Indexes
    {
        get
        {
            if (this.IsAttached())
                return Primitives
                    .OrderByDescending(w => w.Order)
                    .SelectMany(primitive => primitive.Vertexes.Select(vertex => vertex.Index!.Value));

            return new uint[] { };
        }
    }

    BBox? _LBBox = null;
    // TODO: manage figure vertex invalidation

    public BBox LBBox
    {
        get
        {
            if (_LBBox is null)
                _LBBox = new BBox(Primitives.Select(w => w.LBBox));

            return _LBBox;
        }
    }

    public BBox OBBox => new BBox(Primitives.Select(w => w.LBBox.Transform(ObjectMatrix)));

    public virtual bool EvalInShadowMap => true;

    #endregion        

    /// <summary>
    /// Emit <see cref="FigureInvalidated"/> event.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Invalidate() => FigureInvalidated?.Invoke(this);
  

    public override string ToString()
    {
        var tagNfo = "";
        if (Tag is not null) tagNfo = $" Tag:{Tag}";

        return $"{Primitives.Count} {PrimitiveType} {tagNfo}";
    }
}

public static partial class Ext
{

    /// <summary>
    /// Vertexes belonging this figure [object].
    /// </summary>        
    public static IEnumerable<GLVertex> Vertexes(this IGLFigure figure)
    {
        var vertexes = new HashSet<GLVertex>();

        foreach (var primitive in figure.Primitives)
        {
            foreach (var vertex in primitive.Vertexes)
                vertexes.Add(vertex);
        }

        return vertexes;
    }

    /// <summary>
    /// Object space bbox of given figures vertexes.
    /// </summary>
    /// <param name="figures">Gl figures to compute bbox.</param>    
    /// <returns>Figures bbox [object].</returns>
    public static BBox OBBox(this IEnumerable<GLFigureBase> figures) => new BBox(figures.Select(w => w.OBBox));

}
