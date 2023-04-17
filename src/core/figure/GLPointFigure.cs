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
    [JsonConstructor]
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

    protected override void CopySpecialized(GLFigureBase other)
    {
        base.CopySpecialized(other);

        var sother = (GLPointFigure)other;

        PointSize = sother.PointSize;
    }

    public override GLFigureBase? Mirror(in Matrix4x4 xyPlane)
    {
        var mirroredPrimitives = new ObservableCollection<GLPrimitiveBase>();
        foreach (var primitive in PrimitivesOBC)
        {
            var q = primitive.Mirror(xyPlane);
            if (q is null) return null;

            mirroredPrimitives.Add(q);
        }

        var copy = (GLPointFigure)this.CopyBase();

        copy.PointSize = PointSize;
        copy.PrimitivesOBC = mirroredPrimitives;

        return copy;
    }

    /// <summary>
    /// Set figure points size.
    /// </summary>
    /// <param name="pointSize">point size in pixels [screen].</param>
    /// <returns>This figure.</returns>
    public GLPointFigure SetPointSize(float pointSize) => this.Act(fig => fig.PointSize = pointSize);

}
