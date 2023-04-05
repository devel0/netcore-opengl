namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Geometric entity 3d line.<br/>
/// Internally encoded as <see cref="From"/> and <see cref="V"/> vectors, where <see cref="To"/> = From + V.<br/>
/// Can be created through static methods <see cref="FromTo"/> and <see cref="PointV"/>.
/// </summary>
public class Line
{

    /// <summary>
    /// Coordinate "from" of the line.
    /// </summary>    
    public Vector3 From { get; private set; }

    /// <summary>
    /// Vector extension of the line so that To = From + V.<br/>
    /// </summary>    
    public Vector3 V { get; private set; }

    /// <summary>
    /// Coordinate "to" of the line.
    /// </summary>
    public Vector3 To => From + V;

    /// <summary>
    /// Line midpoint as (From + To) / 2.
    /// </summary>
    public Vector3 MidPoint => (From + To) / 2;

    Line(Vector3 from, Vector3 v)
    {
        From = from;
        V = v;
    }

    /// <summary>
    /// Length of the line segment.
    /// </summary>    
    public float Length => V.Length();

    /// <summary>
    /// Create a line by two given points.
    /// </summary>
    /// <param name="from">From point.</param>
    /// <param name="to">To point.</param>    
    public static Line FromTo(Vector3 from, Vector3 to) => new Line(from, to - from);

    /// <summary>
    /// Create a line by the from position and given extension.
    /// </summary>
    /// <param name="point">From coordinate.</param>
    /// <param name="v">Extension vector.</param>    
    public static Line PointV(Vector3 point, Vector3 v) => new Line(point, v);

    /// <summary>
    /// Create a new line by scaling from, to coordinates respect the given reference point and scale factor.
    /// </summary>
    /// <param name="refpt">Reference point.</param>
    /// <param name="factor">Scale factor.</param>
    /// <returns>Scaled line.</returns>
    public Line Scale(Vector3 refpt, float factor) =>
        Line.FromTo(From.ScaleAbout(refpt, factor), To.ScaleAbout(refpt, factor));

    /// <summary>
    /// Test if this line intersect given plane and retrieve intersection point or null if no intersect.
    /// </summary>
    /// <remarks>
    /// References: https://en.wikipedia.org/wiki/Line%E2%80%93plane_intersection
    /// </remarks>
    /// <param name="plane">Plane to test line intersection with. ( <see cref="MakeCS"/> to create planes )</param>
    /// <returns>Intersection point or null if no intersection found.</returns>
    public Vector3? Intersect(Matrix4x4 plane)
    {
        var l = Vector3.Normalize(V);
        var n = plane.BaseZ();

        var l_dot_n = Vector3.Dot(l, n);
        if (l_dot_n.EqualsTol(NormalizedTolerance, 0)) return null;

        var p0 = plane.Origin();
        var l0 = From;
        var d = Vector3.Dot(p0 - l0, n) / l_dot_n;

        return l0 + l * d;
    }

    /// <summary>
    /// States if this line contains given point within given tolerance.
    /// </summary>
    /// <param name="tol">Comparision tolerance.</param>
    /// <param name="p">Point to test if its contained in this line.</param>
    /// <returns>True if point contained in this line.</returns>
    public bool Contains(float tol, in Vector3 p)
    {
        if (Length == 0) return false;

        var prj = (p - From).Project(V) + From;

        var dprj = (p - prj).Length();

        return dprj <= tol;
    }

    public override string ToString() => $"{From} + {V} = {To}";

}

public static partial class Ext
{

    /// <summary>
    /// Create a new line with from, to transformed by the given transformation matrix.
    /// </summary>
    /// <param name="line">Line to transform.</param>
    /// <param name="m">Transformation matrix.</param>
    /// <returns>Transformed line.</returns>
    public static Line Transform(this Line line, in Matrix4x4 m) =>
        Line.FromTo(Vector3.Transform(line.From, m), Vector3.Transform(line.To, m));

    /// <summary>
    /// Create a line from this point with given extension vector.
    /// </summary>
    /// <param name="from">This point.</param>
    /// <param name="v">Extension vector.</param>    
    public static Line LineV(this Vector3 from, Vector3 v) => Line.PointV(from, v);

}