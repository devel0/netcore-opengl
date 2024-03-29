namespace SearchAThing.OpenGL.Core;

[JsonObject(MemberSerialization.OptIn)]
public class GLTriangle : GLPrimitiveBase, IGLTriangle
{

    #region V1

    private GLVertex _V1 = new GLVertex();

    [JsonProperty]
    public GLVertex V1
    {
        get => _V1;
        set
        {
            value.EnsureDetached();

            var changed = value != _V1;
            if (changed)
            {
                if (ParentFigure is not null)
                    ParentVertexManager?.RemoveVertex(_V1);

                _V1 = value;

                if (ParentFigure is not null)
                    ParentVertexManager?.AddVertex(_V1);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region V2

    private GLVertex _V2 = new GLVertex();

    [JsonProperty]
    public GLVertex V2
    {
        get => _V2;
        set
        {
            value.EnsureDetached();

            var changed = value != _V2;
            if (changed)
            {
                if (ParentFigure is not null)
                    ParentVertexManager?.RemoveVertex(_V2);

                _V2 = value;

                if (ParentFigure is not null)
                    ParentVertexManager?.AddVertex(_V2);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region V3

    private GLVertex _V3 = new GLVertex();

    [JsonProperty]
    public GLVertex V3
    {
        get => _V3;
        set
        {
            value.EnsureDetached();

            var changed = value != _V3;
            if (changed)
            {
                if (ParentFigure is not null)
                    ParentVertexManager?.RemoveVertex(_V3);

                _V3 = value;

                if (ParentFigure is not null)
                    ParentVertexManager?.AddVertex(_V3);

                OnPropertyChanged();
            }
        }
    }

    #endregion    

    public Matrix4x4 Plane => MakeCS(V1.Position, this.DefaultTriangleNormal());

    public Matrix4x4 PlaneNZ
    {
        get
        {
            if (ParentFigure is GLTriangleFigure triFig)
                return MakeCS(V1.Position, triFig.ComputeNormal(this, V1));

            throw new Exception($"parent figure not defined");
        }
    }

    GLTriangle() : base(GLPrimitiveType.Triangle) { }

    /// <summary>
    /// Create an opengl triangle by given three vertexes.
    /// </summary>
    /// <param name="v1">First vertex.</param>
    /// <param name="v2">Second vertex.</param>
    /// <param name="v3">Third vertex.</param>    
    public GLTriangle(GLVertex v1, GLVertex v2, GLVertex v3) : base(GLPrimitiveType.Triangle)
    {
        _V1 = v1;
        _V2 = v2;
        _V3 = v3;
    }

    /// <summary>
    /// Create an opengl triangle by given three coordinates.
    /// </summary>
    /// <param name="p1">First position.</param>
    /// <param name="p2">Second position.</param>
    /// <param name="p3">Third position.</param>    
    public GLTriangle(in Vector3 p1, in Vector3 p2, in Vector3 p3) : base(GLPrimitiveType.Triangle)
    {
        _V1 = new GLVertex(p1);
        _V2 = new GLVertex(p2);
        _V3 = new GLVertex(p3);
    }

    public bool Contains(in Vector3 point)
    {
        var a = V1.Position - point;
        var b = V2.Position - point;
        var c = V3.Position - point;

        var u = Vector3.Cross(b, c);
        var v = Vector3.Cross(c, a);
        var w = Vector3.Cross(a, b);

        if (Vector3.Dot(u, v) < 0 || Vector3.Dot(u, w) < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Enumerate triangle as opengl lines (V1,V2), (V2,V3) and (V3,V1).
    /// </summary>    
    public IEnumerable<GLLine> Lines()
    {
        yield return GLLine.FromTo(V1, V2);
        yield return GLLine.FromTo(V2, V3);
        yield return GLLine.FromTo(V3, V1);
    }

    protected override GLPrimitiveBase MakeInstance() => new GLTriangle();

    protected override void CopySpecialized(GLPrimitiveBase other)
    {
        var sother = (GLTriangle)other;

        V1 = (GLVertex)sother.V1.Copy();
        V2 = (GLVertex)sother.V2.Copy();
        V3 = (GLVertex)sother.V3.Copy();
    }

    public override GLPrimitiveBase? Mirror(in Matrix4x4 xyPlane)
    {
        var mirroredV1 = V1.Mirror(xyPlane);
        if (mirroredV1 is null) return null;

        var mirroredV2 = V2.Mirror(xyPlane);
        if (mirroredV2 is null) return null;

        var mirroredV3 = V3.Mirror(xyPlane);
        if (mirroredV3 is null) return null;

        var copy = (GLTriangle)this.CopyBase();

        copy.V1 = mirroredV1;
        copy.V2 = mirroredV2;
        copy.V3 = mirroredV3;

        return copy;
    }

    public override IEnumerable<GLVertex> Vertexes
    {
        get
        {
            yield return V1;
            yield return V2;
            yield return V3;
        }
    }

    public override string ToString() => Invariant($"{V1} {V2} {V3}");

    /// <summary>
    /// [t ]x1,y1,z1,x2,y2,z2,x3,y3,z3
    /// </summary>    
    public override string SimpleCmd(bool includeHeader = true)
    {
        var res = includeHeader ? $"{SIMPLE_CMD_TRIANGLE} " : "";

        return res + $"{V1.Position.SimpleCmd()},{V2.Position.SimpleCmd()},{V3.Position.SimpleCmd()}";
    }
}

public static partial class Ext
{

    /// <summary>
    /// Create a gl figure from given gl triangles.
    /// </summary>    
    public static GLTriangleFigure ToFigure(this IEnumerable<GLTriangle> triangles) => new GLTriangleFigure(triangles);

    /// <summary>
    /// Create sci Triangle3D from given gl triangle.
    /// </summary>
    public static Triangle3D ToTriangle3D(this GLTriangle tri) =>
        new Triangle3D(tri.V1.Position.ToVector3D(), tri.V2.Position.ToVector3D(), tri.V3.Position.ToVector3D());

    /// <summary>
    /// Retrieve intersection segment if exists between given two triangles.
    /// </summary>
    /// <param name="tol">Length comparision tolerance.</param>
    /// <param name="tri1">First triangle.</param>
    /// <param name="tri2">Second triangle.</param>
    /// <param name="bboxSkipped">True if intersect test skipped due to non intersecting bboxes.</param>
    /// <returns>Intersection line between two triangles or null if no intersection exists.</returns>
    public static Line? Intersect(this GLTriangle tri1, float tol, GLTriangle tri2, out bool bboxSkipped)
    {
        if (tri1.LBBox.Intersects(tol, tri2.LBBox))
        {
            bboxSkipped = false;
            var q = tri1.ToTriangle3D().Intersect(tol, tri2.ToTriangle3D());
            if (q is not null)
                return q.ToLine();
        }

        bboxSkipped = true;

        return null;
    }

}