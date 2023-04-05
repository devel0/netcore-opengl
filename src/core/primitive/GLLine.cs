namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Opengl line primitive.
/// </summary>
public class GLLine : GLPrimitiveBase, IGLLine
{

    #region From

    private GLVertex _From = new GLVertex();

    public GLVertex From
    {
        get => _From;
        set
        {
            value.EnsureDetached();

            var changed = value != _From;
            if (changed)
            {
                if (ParentFigure is not null)
                    ParentVertexManager?.RemoveVertex(_From);

                _From = value;

                if (ParentFigure is not null)
                    ParentVertexManager?.AddVertex(value);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region To

    private GLVertex _To = new GLVertex();

    public GLVertex To
    {
        get => _To;
        set
        {
            value.EnsureDetached();

            var changed = value != _To;
            if (changed)
            {
                if (ParentFigure is not null)
                    ParentVertexManager?.RemoveVertex(_To);

                _To = value;

                if (ParentFigure is not null)
                    ParentVertexManager?.AddVertex(value);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    GLLine(GLVertex from, GLVertex to) : base(GLPrimitiveType.Line)
    {
        From = from;
        To = to;
    }

    /// <summary>
    /// Create an opengl line.
    /// </summary>    
    /// <remarks>
    /// This entity can be added into the model through a <see cref="GLLineFigure"/>.
    /// </remarks>
    public GLLine() : base(GLPrimitiveType.Line)
    {
    }

    /// <summary>
    /// Create an opengl line by given geometric line template.
    /// </summary>    
    /// <remarks>
    /// This entity can be added into the model through a <see cref="GLLineFigure"/>.
    /// </remarks>
    public GLLine(Line line) : this(new GLVertex(line.From), new GLVertex(line.To))
    {
    }

    protected override GLPrimitiveBase MakeInstance() => new GLLine();

    protected override void CopyFromSpecialized(GLPrimitiveBase other)
    {
        var sother = (GLLine)other;

        From = (GLVertex)sother.From.Copy();
        To = (GLVertex)sother.To.Copy();
    }

    /// <summary>
    /// Create a new opengl line by two points through given two gl vertexes.
    /// </summary>
    /// <param name="from">From vertex.</param>
    /// <param name="to">To vertex.</param>    
    public static GLLine FromTo(GLVertex from, GLVertex to) => new GLLine(from, to);

    /// <summary>
    /// Create a new opengl line by two given points setting given optional color.
    /// </summary>
    /// <param name="from">From position.</param>
    /// <param name="to">To position.</param>
    /// <param name="color">(Optional) color.</param>    
    public static GLLine FromTo(Vector3 from, Vector3 to, Color? color = null) =>
        new GLLine(new GLVertex(from, color), new GLVertex(to, color));

    /// <summary>
    /// Create a new opengl line by two given 2d points ( assuming z=0 ) setting given optional color.
    /// </summary>
    /// <param name="from">From position.</param>
    /// <param name="to">To position.</param>
    /// <param name="color">(Optional) color.</param>    
    public static GLLine FromTo(Vector2 from, Vector2 to, Color? color = null) =>
        new GLLine(new GLVertex(from.ToVector3(), color), new GLVertex(to.ToVector3(), color));

    /// <summary>
    /// Create a new opengl line by two given points setting color of two points.
    /// </summary>
    /// <param name="from">From position.</param>
    /// <param name="to">To position.</param>
    /// <param name="colorFrom">From vertex color.</param>
    /// <param name="colorTo">To vertex color.</param>    
    public static GLLine FromTo(Vector3 from, Vector3 to, Color colorFrom, Color colorTo) =>
        new GLLine(new GLVertex(from, colorFrom), new GLVertex(to, colorTo));

    /// <summary>
    /// Create a new opengl line by given from point extending of the given vector.
    /// </summary>
    /// <param name="point">From position.</param>
    /// <param name="v">Extension vector.</param>    
    public static GLLine PointV(GLVertex point, GLVertex v) =>
        new GLLine(point, new GLVertex
        {
            Position = point.Position + v.Position,
            MaterialColor = v.MaterialColor,
            MaterialProperties = v.MaterialProperties,
            Normal = v.Normal,
            TextureST = v.TextureST
        });

    /// <summary>
    /// Create a new opengl line by given point extending from there extending of the given vector setting optionally the color.
    /// </summary>
    /// <param name="point">From position.</param>
    /// <param name="v">Extension vector.</param>
    /// <param name="color">Color to apply the line ( from, to vertexes ).</param>
    /// <returns></returns>
    public static GLLine PointV(Vector3 point, Vector3 v, Color? color = null) =>
        PointV(new GLVertex(point, color), new GLVertex(v, color));

    /// <summary>
    /// Create a new opengl line by given 2d point extending of the given 2d vector (assuming Z=0)
    /// settings optionally the color.
    /// </summary>
    /// <param name="point">From position.</param>
    /// <param name="v">Extension vector.</param>
    /// <param name="color">(Optional) color to set.</param>    
    public static GLLine PointV(Vector2 point, Vector2 v, Color? color = null) =>
        PointV(new GLVertex(point.ToVector3(), color), new GLVertex(v.ToVector3(), color));

    /// <summary>
    /// Create a new opengl line by given point extending of the given vector with given color for the from, to vertexes.
    /// </summary>
    /// <param name="point">From position.</param>
    /// <param name="v">Extension vector.</param>
    /// <param name="colorFrom">From vertex color.</param>
    /// <param name="colorTo">To vertex color.</param>    
    public static GLLine PointV(Vector3 point, Vector3 v, Color colorFrom, Color colorTo) =>
        PointV(new GLVertex(point, colorFrom), new GLVertex(v, colorTo));

    public override IEnumerable<GLVertex> Vertexes
    {
        get
        {
            yield return From;
            yield return To;
        }
    }

    /// <summary>
    /// Compute intersection point of this gl line to the given plane.
    /// </summary>
    /// <param name="plane">World matrix plane.</param>
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.MakeCS"/>
    /// <returns>Intersection point or null if no intersection found.</returns>
    public Vector3? Intersect(Matrix4x4 plane) => Line.FromTo(From.Position, To.Position).Intersect(plane);

    public override string ToString() => Invariant($"{From} {To}");
}

public static partial class Toolkit
{

    /// <summary>
    /// Enumerate given point set with gl lines.
    /// </summary>
    /// <param name="pts">Points set.</param>    
    /// <param name="closed">If true a line will added between last and first point.</param>   
    public static IEnumerable<GLLine> ToGLLines(this IEnumerable<Vector3> pts, bool closed = false) =>
        pts
        .WithNextPrimitive(repeatFirstAtEnd: closed)
        .Where(nfo => !nfo.isLast)
        .Select(nfo => GLLine.FromTo(nfo.item, nfo.next!.Value));

}
