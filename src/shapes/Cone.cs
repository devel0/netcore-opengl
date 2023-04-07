namespace SearchAThing.OpenGL.Shapes;

/// <summary>
/// Cone, truncated cone or pyramid shape.
/// </summary>
public class Cone
{

    /// <summary>
    /// Coordinate system of the cone base.
    /// </summary>    
    public Matrix4x4 BaseCS { get; private set; }

    /// <summary>
    /// Base radius of the cone.
    /// </summary>
    public float BaseRadius { get; private set; }

    /// <summary>
    /// Top radius of the cone.
    /// </summary>
    /// <value></value>
    public float TopRadius { get; private set; }

    /// <summary>
    /// Height of the cone.
    /// </summary>    
    public float Height { get; private set; }

    /// <summary>
    /// If true figure include bottom closure (Default: true).
    /// </summary>    
    public bool BottomCap { get; private set; }

    /// <summary>
    /// If true figure include top closure (Default: true).
    /// </summary>    
    public bool TopCap { get; private set; }
    
    /// <summary>
    /// Create a:<br/>
    /// - Truncated cone ( if topRadius smaller than baseRadius ).<br/>
    /// - Pyramic ( if topRadius equals zero ).<br/>
    /// - Cone ( if baseRadius == topRadius ).<br/>
    /// </summary>
    /// <param name="baseCS">Coordinate system of the cone base.</param>
    /// <param name="baseRadius">Base radius of the cone.</param>
    /// <param name="topRadius">Top radius of the cone.</param>
    /// <param name="height">Height of the cone.</param>
    /// <param name="bottomCap">If true figure include bottom cap (Default:true).</param>
    /// <param name="topCap">If true figure include top cap (Default:true).</param>
    public Cone(in Matrix4x4 baseCS, float baseRadius, float topRadius, float height,
        bool bottomCap = true, bool topCap = true)
    {
        BaseCS = baseCS;
        BaseRadius = baseRadius;
        TopRadius = topRadius;
        Height = height;
        BottomCap = bottomCap;
        TopCap = topCap;
    }

    /// <summary>
    /// Gl triangle figure that represents the cone.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure. (Min:4)</param>    
    public GLTriangleFigure Figure(int divisions = DEFAULT_CONE_DIVISIONS)
    {
        var fig = new GLTriangleFigure(Triangles(divisions));

        return fig;
    }

    /// <summary>
    /// Gl triangles that represents the cone.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure. (Min:4)</param>        
    public IEnumerable<GLTriangle> Triangles(int divisions)
    {
        if (divisions < 4) throw new ArgumentException($"divisions must >= 4");
        if (BaseRadius <= 0) throw new ArgumentException($"BaseRadius must > 0");
        if (TopRadius < 0) throw new ArgumentException($"TopRadius must >= 0");
        if (Height <= 0) throw new ArgumentException($"Height must > 0");

        var R = BaseRadius;
        var r = TopRadius;
        var h = Height;
        var N = divisions;

        var cs = BaseCS;

        var C = cs.Origin();
        var basex = cs.BaseX();
        var basey = cs.BaseY();
        var basez = cs.BaseZ();

        var H = (h * R) / (R - r);
        var D = C + basez * H;

        var alpha = 0f;
        var alphaStep = (float)(2 * PI / N);

        Vector3? q = null;

        for (int i = 0; i < N + 1; ++i)
        {
            var p = C + Vector3.Transform(basex, Matrix4x4.CreateFromAxisAngle(basez, alpha)) * R;

            if (q is not null)
            {
                var triD = new GLTriangle(q.Value, p, D); ;

                if (r == 0) // pyramid
                    yield return triD;

                else
                {
                    var vh = basez * h;
                    Vector3 qq, pp;

                    if (r == R) // cone
                    {
                        // base
                        qq = q.Value + vh;
                        pp = p + vh;

                        var plate = new Plate(pp, qq, q.Value, p);
                        foreach (var tri in plate.GetTriangles()) yield return tri;
                    }

                    else // truncated cone
                    {
                        // base
                        var cutplane = BaseCS.Move(basez * h);

                        var lines = triD.Lines();

                        var ips = lines
                            .Select(l => l.Intersect(cutplane))
                            .Where(ip => ip is not null)
                            .Select(ip => ip!.Value)
                            .OrderBy(ip => (ip - p).Length())
                            .ToList();

                        qq = ips[1];
                        pp = ips[0];

                        foreach (var tri in new Plate(q.Value, p, pp, qq).GetTriangles())
                            yield return tri;
                    }

                    // top                        
                    if (TopCap)
                        yield return new GLTriangle(qq, pp, C + vh);
                }

                // base        
                if (BottomCap)                             
                    yield return new GLTriangle(C, p, q.Value);
            }

            q = p;

            alpha += alphaStep;
        }
    }

}
