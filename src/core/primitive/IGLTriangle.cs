namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Primitive for GL_TRIANGLES opengl element.
/// </summary>
public interface IGLTriangle : IGLPrimitive
{

    /// <summary>
    /// Triangle first vertex.
    /// </summary>    
    GLVertex V1 { get; set; }

    /// <summary>
    /// Triangle second vertex.
    /// </summary>    
    GLVertex V2 { get; set; }

    /// <summary>
    /// Triangle third vertex.
    /// </summary>    
    GLVertex V3 { get; set; }

    /// <summary>
    /// Compute default triangle plane such as:<br/>
    /// - origin = V1<br/>
    /// - normal = Normalize((V2-V1) x (V3-V1))
    /// </summary>    
    /// <seealso cref="Toolkit.DefaultTriangleNormal(IGLTriangle)"/>
    Matrix4x4 Plane { get; }

    /// <summary>
    /// Compute triangle plane such as:<br/>
    /// - origin = V1<br/>
    /// - basez is concordant to owner figure <see cref="GLTriangleFigure.ComputeNormal"/> delegate.
    /// </summary>        
    Matrix4x4 PlaneNZ { get; }

    /// <summary>
    /// States if given point is contained in the triangle, borders included.
    /// </summary>
    /// <param name="point">Point to test.</param>    
    bool Contains(in Vector3 point);

}

public static partial class Toolkit
{

    /// <summary>
    /// Default triangle compute normal function:<br/>
    /// Normalize((V2-V1) x (V3-V1))
    /// </summary>
    /// <param name="tri">Triangle which compute default normal.</param>
    /// <returns>Triangle default normal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 DefaultTriangleNormal(this IGLTriangle tri) =>
        Vector3.Normalize(Vector3.Cross(tri.V2.Position - tri.V1.Position, tri.V3.Position - tri.V1.Position));

    /// <summary>
    /// Average color of triangles vertexes color.
    /// </summary>    
    public static Color AverageColor(this IGLTriangle tri) =>
        ((tri.V1.MaterialColor + tri.V2.MaterialColor + tri.V3.MaterialColor) / 3).ToColor();

    /// <summary>
    /// Retrieve dxf face from given triangle.
    /// </summary>
    /// <param name="tri">Gl triangle.</param>
    /// <returns>Dxf face.</returns>
    public static netDxf.Entities.Face3D ToDxfFace3D(this IGLTriangle tri) =>
        new netDxf.Entities.Face3D(tri.V1.ToDxfVector3(), tri.V2.ToDxfVector3(), tri.V3.ToDxfVector3())
        { Color = tri.AverageColor().ToDxfColor() };

}

public static partial class Ext
{

    /// <summary>
    /// Retrieve gl triangle segments {v1,v2},{v2,v3},{v3,v1} [object].
    /// </summary>    
    public static IEnumerable<Line> Segments(this IGLTriangle tri)
    {
        var v1 = tri.V1.Position;
        var v2 = tri.V2.Position;
        var v3 = tri.V3.Position;

        if (tri.ParentFigure is not null && tri.ParentFigure.ObjectMatrix is Matrix4x4 om)
        {
            v1 = LocalToObject(v1, om);
            v2 = LocalToObject(v2, om);
            v3 = LocalToObject(v3, om);
        }

        yield return Line.FromTo(v1, v2);
        yield return Line.FromTo(v2, v3);
        yield return Line.FromTo(v3, v1);
    }

    /// <summary>
    /// Find feasible triangle pairs that could intersect by evaluating the
    /// <see cref="SearchAThing.Sci.DiscreteSpace{T}"/> over this set of triangles
    /// against the given set of triangles.<br/>
    /// Search will be done using <see cref="SearchAThing.Sci.DiscreteSpace{T}.GetItemsAt(Vector3D, double)"/> method
    /// within given margin parameter factor respect the triangle bbox.
    /// </summary>
    /// <param name="tris1">This triangles.</param>
    /// <param name="tris2">Other triangles.</param>
    /// <param name="tol">Intersection test length comparision tolerance.</param>    
    /// <param name="margin">Bbox factor (Default:1.2).</param>    
    public static IEnumerable<(T tri1, T tri2)>
        FeasibleTriIntersectionTests<T>(this IEnumerable<T> tris1,
            double tol,
            IEnumerable<T> tris2,
            double margin = 1.2d) where T : IGLTriangle
    {
        var ds2 = new DiscreteSpace<T>(tol, tris2,
            (tri) =>
            {
                return tri.LBBox.Middle.ToVector3D();
            }, _spaceDim: 3);

        foreach (var tri in tris1)
        {
            var qTris = ds2.GetItemsAt(tri.LBBox.Middle, margin * tri.LBBox.Size.Max());

            foreach (var qtri in qTris)
            {
                if (object.ReferenceEquals(qtri, tri)) continue;

                if (tri.LBBox.Intersects(qtri.LBBox))
                    yield return (tri, qtri);
            }
        }
    }

}