namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Specialization of typed figure within the <see cref="GLPoint"/>} primitive.
/// </summary>
public class GLPointFigure : GLFigureTypeBase<GLPoint>, IGLPointFigure
{

    #region PointSize

    private float _PointSize = DEFAULT_PointSize;
       
    public float PointSize
    {
        get => _PointSize;
        set
        {
            var changed = value != _PointSize;
            if (changed)
            {
                _PointSize = value;

                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    #endregion

    public override GLPrimitiveType PrimitiveType => GLPrimitiveType.Point;

    /// <summary>
    /// Enumeration of the point primitives of this figure.
    /// </summary>    
    public IEnumerable<GLPoint> Points => Primitives.OfType<GLPoint>();

    /// <summary>
    /// Create an empty points figure.
    /// </summary>
    public GLPointFigure() : base() { }

    /// <summary>
    /// Create a point typed figure with the given set of <see cref="GLPoint"/> primitives created by the given coords [local].
    /// </summary>    
    public GLPointFigure(params Vector3[] points) : base(points.Select(w => new GLPoint(w))) { }

    /// <summary>
    /// Create a point typed figure with the given set of <see cref="GLPoint"/> primitives created by the given coords [local].
    /// </summary>
    public GLPointFigure(IEnumerable<Vector3> points) : base(points.Select(w => new GLPoint(w))) { }

    /// <summary>
    /// Create a point typed figure with the given set of <see cref="GLPoint"/> primitives.
    /// </summary>
    public GLPointFigure(params GLPoint[] points) : base(points) { }

    /// <summary>
    /// Create a point typed figure with the given set of <see cref="GLPoint"/> primitives.
    /// </summary>
    public GLPointFigure(IEnumerable<GLPoint> points) : base(points) { }

    protected override GLFigureBase MakeInstance() => new GLPointFigure();

    protected override void CopyFromSpecialized(GLFigureBase other)
    {
        base.CopyFromSpecialized(other);

        var sother = (GLPointFigure)other;

        PointSize = sother.PointSize;
    }
    
}
