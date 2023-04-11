namespace SearchAThing.OpenGL.Core;

public class GLPoint : GLPrimitiveBase, IGLPoint
{

    #region Vertex

    private GLVertex _Vertex = new GLVertex();
    /// <summary>
    /// Point vertex.
    /// </summary>
    public GLVertex Vertex
    {
        get => _Vertex;
        set
        {
            value.EnsureDetached();

            var changed = value != _Vertex;
            if (changed)
            {
                if (ParentFigure is not null)
                    ParentVertexManager?.RemoveVertex(_Vertex);

                _Vertex = value;

                if (ParentFigure is not null)
                    ParentVertexManager?.AddVertex(value);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    GLPoint() : base(GLPrimitiveType.Point) { }

    /// <summary>
    /// Create an opengl point by given vertex.
    /// </summary>
    /// <param name="v">Point vertex [object].</param>    
    public GLPoint(GLVertex v) : base(GLPrimitiveType.Point)
    {
        Vertex = v;
    }

    /// <summary>
    /// Create an opengl point by given coordinate and optional vertex color.
    /// </summary>    
    /// <param name="x">Point coord x [object].</param>
    /// <param name="y">Point coord y [object].</param>
    /// <param name="z">Point coord z [object].</param>
    /// <param name="color">Point color.</param>        
    /// <returns></returns>
    public GLPoint(float x, float y, float z, in Color? color = null) : this(new Vector3(x, y, z), color) { }

    /// <summary>
    /// Create an opengl point by given coordinate and optional vertex color.
    /// </summary>
    /// <param name="coord">Point position [object].</param>
    /// <param name="color">Point color.</param>    
    public GLPoint(in Vector3 coord, in Color? color = null) : this(new GLVertex(coord, color)) { }

    protected override GLPrimitiveBase MakeInstance() => new GLPoint();

    protected override void CopySpecialized(GLPrimitiveBase other)
    {
        var sother = (GLPoint)other;

        Vertex = (GLVertex)sother.Vertex.Copy();
    }

    public override GLPrimitiveBase? Mirror(in Matrix4x4 xyPlane)
    {
        var mirroredVertex = Vertex.Mirror(xyPlane);
        if (mirroredVertex is null) return null;

        var copy = (GLPoint)this.CopyBase();

        copy.Vertex = mirroredVertex;

        return copy;
    }

    public override IEnumerable<GLVertex> Vertexes
    {
        get
        {
            yield return Vertex;
        }
    }

}

public static partial class Ext
{

    /// <summary>
    /// Create point figure from given point set.
    /// </summary>    
    public static GLPointFigure ToFigure(this IEnumerable<GLPoint> points) => new GLPointFigure(points);

    /// <summary>
    /// Create point figure from given point set.
    /// </summary>    
    public static GLPointFigure ToFigure(this IEnumerable<Vector3> points) => points.Select(v => new GLPoint(v)).ToFigure();

}