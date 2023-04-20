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
    public static Line FromTo(in Vector3 from, in Vector3 to) => new Line(from, to - from);

    /// <summary>
    /// Create a line by the from position and given extension.
    /// </summary>
    /// <param name="point">From coordinate.</param>
    /// <param name="v">Extension vector.</param>    
    public static Line PointV(in Vector3 point, in Vector3 v) => new Line(point, v);

    /// <summary>
    /// Create a new line by scaling from, to coordinates respect the given reference point and scale factor.
    /// </summary>
    /// <param name="refpt">Reference point.</param>
    /// <param name="factor">Scale factor.</param>
    /// <returns>Scaled line.</returns>
    public Line Scale(in Vector3 refpt, float factor) =>
        Line.FromTo(From.ScaleAbout(refpt, factor), To.ScaleAbout(refpt, factor));

    /// <summary>
    /// Test if this line intersect given plane and retrieve intersection point or null if no intersect.
    /// </summary>
    /// <remarks>
    /// References: https://en.wikipedia.org/wiki/Line%E2%80%93plane_intersection
    /// </remarks>
    /// <param name="plane">Plane to test line intersection with. ( <see cref="MakeCS"/> to create planes )</param>
    /// <returns>Intersection point or null if no intersection found.</returns>
    public Vector3? Intersect(in Matrix4x4 plane)
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
    /// Intresect this line to the given other.
    /// </summary>
    /// <param name="tol">Length tolerance comparision.</param>
    /// <param name="other">Other line.</param>
    /// <param name="dst">Distance between two intersecting lines.</param>
    /// <param name="thisAsSegment">Treat this line as a segment from,to.</param>
    /// <param name="otherAsSegment">Treat other line as a segment from,to.</param>
    /// <returns>Intersection point or null if line parallel or no intersects.</returns>
    public Vector3? Intersect(float tol, in Line other, out float dst, bool thisAsSegment = false, bool otherAsSegment = false)
    {
        var tLine = this.ToLine3D();
        var oLine = other.ToLine3D();

        var perpSeg = tLine.ApparentIntersect(oLine);

        dst = 0;
        
        if (perpSeg is not null && perpSeg.From.EqualsTol(tol, perpSeg.To))
        {
            if (thisAsSegment && !tLine.SegmentContainsPoint(tol, perpSeg.From)) return null;            
            if (otherAsSegment && !oLine.SegmentContainsPoint(tol, perpSeg.To)) return null;

            dst = (float)perpSeg.Length;

            return perpSeg.MidPoint.ToNVector3();
        }        

        return null;

        // if (ip is null) return null;

        // return ip.ToNVector3();
    }

    /// <summary>
    /// States if this line contains given point within given tolerance.
    /// </summary>
    /// <param name="tol">Comparision tolerance.</param>
    /// <param name="p">Point to test if its contained in this line.</param>
    /// <param name="prj">Point projected onto this line.</param>
    /// <param name="segmentMode">If true line will treated as a segment ( Default: false ).</param>
    /// <returns>Distance to projected point if contained under given tolerance, null otherwise.</returns>
    public float? Contains(float tol, in Vector3 p, out Vector3 prj, bool segmentMode = false)
    {
        if (Length > 0)
        {
            prj = (p - From).Project(V) + From;

            var dprj = (p - prj).Length();

            if (dprj <= tol)
            {
                if (segmentMode)
                {
                    // https://github.com/devel0/netcore-sci/blob/087cc96882d02f915b7ca60927747a1b29d0d51b/src/sci/Line3D.cs#L443

                    // line contains given point if there is a scalar s 
                    // for which p = From + s * V 
                    var s = 0.0;

                    // to find out the scalar we need to test the first non null component 

                    if (!(V.X.EqualsTol(tol, 0))) s = (p.X - From.X) / V.X;
                    else if (!(V.Y.EqualsTol(tol, 0))) s = (p.Y - From.Y) / V.Y;
                    else if (!(V.Z.EqualsTol(tol, 0))) s = (p.Z - From.Z) / V.Z;

                    // s is the scalar of V vector that runs From->To 

                    if (s >= 0.0 && s <= 1.0)
                        return dprj;

                    // point on the line but outside exact segment
                    // check with tolerance

                    if (s < 0)
                        return p.EqualsTol(tol, From) ? dprj : null;
                    else
                        return p.EqualsTol(tol, To) ? dprj : null;
                }

                return dprj;
            }
        }
        else
        {
            prj = Vector3.Zero;            
        }

        return null;
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
    public static Line LineV(this in Vector3 from, in Vector3 v) => Line.PointV(from, v);

    /// <summary>
    /// Convert given sci line to line.
    /// </summary>    
    public static Line ToLine(this Line3D line) => Line.FromTo(line.From, line.To);

    /// <summary>
    /// Convert given line to sci line.
    /// </summary>    
    public static Line3D ToLine3D(this Line line) => new Line3D(line.From.ToVector3D(), line.To.ToVector3D());

    /// <summary>
    /// Intersect given raycast to given figure primitives.<br/>
    /// Given [local] raycast will automatically backwarded to [object] raycast depending on the object matrix of given figure.<br/>
    /// <seealso cref="GLControl.RayCastLocal"/>
    /// </summary>
    /// <param name="lraycast">Ray cast [local].</param>
    /// <param name="tol">Length tolerance comparision ( used for point and line primitives match ).</param>
    /// <param name="figure">Figure to test for hit vs. given raycast.</param>
    /// <returns>List of hits, at most one foreach figure primitive.</returns>
    public static IEnumerable<RayCastHitTest> Intersect(this Line lraycast, float tol, GLFigureBase figure)
    {
        Line oraycast;

        if (figure.ObjectMatrixIsIdentity) // no transform required if fig object matrix already an identity
            oraycast = lraycast;

        else
            oraycast = lraycast.Transform(figure.ObjectMatrix.Inverse());

        switch (figure.PrimitiveType)
        {
            case GLPrimitiveType.Point:
                {
                    foreach (var pt in figure.Primitives.OfType<GLPoint>())
                    {
                        var dprj = oraycast.Contains(tol, pt.Vertex.Position, out var prj, segmentMode: true);
                        if (dprj is not null)
                            yield return new RayCastHitTest(figure, pt, prj, dprj.Value);
                    }
                }
                break;

            case GLPrimitiveType.Line:
                {
                    foreach (var line in figure.Primitives.OfType<GLLine>())
                    {
                        var _line = Line.FromTo(line.From.Position, line.To.Position);

                        var qip = oraycast.Intersect(tol, _line, out var dst, otherAsSegment: true);                        

                        if (qip is not null)
                            yield return new RayCastHitTest(figure, line, qip.Value, dst);
                    }
                }
                break;

            case GLPrimitiveType.Triangle:
                {
                    foreach (var tri in figure.Primitives.OfType<GLTriangle>())
                    {
                        var ip = oraycast.Intersect(tri.Plane);
                        if (ip is not null && tri.Contains(ip.Value))
                            yield return new RayCastHitTest(figure, tri, ip.Value, distance: 0);
                    }
                }
                break;
        }
    }

}