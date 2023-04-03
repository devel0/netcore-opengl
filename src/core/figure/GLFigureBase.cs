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
    /// Copy data from specialized given other object.
    /// It copies member data not accessible from the <see cref="GLFigureBase"/> because depends on derived classes and need to be implemented there.
    /// </summary>
    /// <param name="other">Other specialized <see cref="GLFigureBase"/>.</param>
    protected abstract void CopyFromSpecialized(GLFigureBase other);

    public IGLVertexManagerObject Copy()
    {
        var res = MakeInstance();

        res.ObjectMatrix = ObjectMatrix;
        res.Visible = Visible;
        res.Order = Order;
        res.Tag = Tag;

        res.CopyFromSpecialized(this);

        return res;
    }

    #endregion

    #region IGLFigure

    public event FigureInvalidatedDelegate? FigureInvalidated;

    public abstract ReadOnlyObservableCollection<GLPrimitiveBase> Primitives { get; }

    public abstract GLPrimitiveType PrimitiveType { get; }

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

    /// <summary>
    /// Move the figure by the given delta.<br/>    
    /// </summary>
    /// <remarks>
    /// - No vertex change, but only the object matrix associated to this figure.
    /// - Object matrix will replaced with a single translation.
    /// </remarks>
    /// <param name="dx">Delta (x) [object]</param>
    /// <param name="dy">Delta (y) [object]</param>
    /// <param name="dz">Delta (z) [object]</param>
    /// <param name="relative">If true (default) sum the given delta elsewhere replace the object matrix translation.</param>
    /// <returns>This figure reference.</returns>
    public GLFigureBase Move(float dx, float dy, float dz, bool relative = true) =>
        Move(new Vector3(dx, dy, dz), relative);

    /// <summary>
    /// Move the figure by the given delta.<br/>    
    /// </summary>
    /// <remarks>
    /// - No vertex change, but only the object matrix associated to this figure.
    /// - Object matrix will replaced with a single translation.
    /// </remarks>
    /// <param name="coord">Delta vector3 [object]</param>
    /// <param name="relative">If true (default) sum the given delta elsewhere replace the object matrix translation.</param>    
    /// <returns>This figure reference.</returns>
    public GLFigureBase Move(Vector3 coord, bool relative = true)
    {
        ObjectMatrix = ObjectMatrix.SetOrigin(
            relative ? ObjectMatrix.Origin() + coord : coord);

        return this;
    }

    public BBox OBBox(Matrix4x4? cs = null)
    {
        var res = new BBox(cs);

        foreach (var primitive in Primitives)
            res.ApplyUnion(primitive.BBox(cs).Transform(ObjectMatrix));

        return res;
    }

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
    /// Distinct vertexes belonging this figure [object].
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

}
