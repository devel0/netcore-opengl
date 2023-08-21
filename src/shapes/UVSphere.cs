namespace SearchAThing.OpenGL.Shapes;

/// <summary>
/// UVSphere shape.
/// </summary>
public class UVSphere
{

    /// <summary>
    /// Center of the sphere.
    /// </summary>    
    public Vector3 Center { get; private set; }

    /// <summary>
    /// Radius of the sphere.
    /// </summary>    
    public float Radius { get; private set; }

    /// <summary>
    /// Create a (uv) sphere with given center and radius.
    /// </summary>
    /// <param name="center">Center of the sphere.</param>
    /// <param name="radius">Radius of the sphere.</param>
    public UVSphere(in Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    /// <summary>
    /// Gl triangle figure that represents the sphere.<br/>
    /// The <see cref="GLTriangleFigure.ComputeNormal"/> is approriately set to compute vertex sphere normals.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure. (Min:4)</param>            
    /// <param name="setUVTexture">If true sets vertexes texture coordinates to fit a rectangle image onto the sphere.</param>   
    public GLTriangleFigure Figure(int divisions, bool setUVTexture = false)
    {
        var fig = new GLTriangleFigure(Triangles(divisions, setUVTexture));

        fig.SetupComputeNormal(computeNormal: (tri, v) => Vector3.Normalize(v.Position - Center));

        return fig;
    }

    class VNfo
    {
        public Vector3 v;
        public Vector2 textureST;
    }

    /// <summary>
    /// Gl triangles that represents the sphere.<br/>
    /// The <see cref="GLTriangleFigure.ComputeNormal"/> is approriately set to compute vertex sphere normals.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure. (Min:4)</param>    
    /// <param name="setUVTexture">If true sets vertexes texture coordinates to fit a rectangle image onto the sphere.</param>   
    public IEnumerable<GLTriangle> Triangles(int divisions, bool setUVTexture = false)
    {
        if (divisions < 2) throw new ArgumentException($"divisions must >= 2");

        // techdoc: UVSphere.dxf

        var R = Radius;
        var C = Center;
        var N = divisions;

        // arc pts goes from a pole (0,0,-r) to other (0,0,r)
        var arcpts = new List<VNfo>();
        List<VNfo>? prevArcpts = null;

        List<VNfo>? firstArcpts = null;

        for (int w = 0; w < 2 * N; ++w)
        {
            for (int k = 0; k <= N; ++k)
            {
                Vector3 p;

                var alpha = (float)(k * PI / N); // alpha [0, PI] : along lat
                var phi = (float)(w * PI / N); // phi [0, 2*PI) : along lng

                // System.Console.WriteLine($"alpha:{alpha} phi:{phi}");

                if (k == 0)
                    p = C - Vector3.UnitZ * R;

                else if (k == N)
                    p = C + Vector3.UnitZ * R;

                else
                {
                    p = C +
                        Vector3.Transform(
                            Vector3.Transform(-Vector3.UnitZ * R, Matrix4x4.CreateRotationX(alpha)),
                            Matrix4x4.CreateRotationZ((float)(-PI / 2 + phi))
                        );
                }

                var vnfo = new VNfo { v = p };
                if (setUVTexture) vnfo.textureST = new Vector2((float)(phi / (2 * PI)), (float)(1 - alpha / PI));
                arcpts.Add(vnfo);
            }

            if (firstArcpts is null) firstArcpts = arcpts;

            if (prevArcpts is not null)
            {
                for (int k = 0; k < arcpts.Count - 1; ++k)
                {
                    var p1 = prevArcpts[k];
                    var p2 = arcpts[k];
                    var p3 = arcpts[k + 1];
                    var p4 = prevArcpts[k + 1];

                    if (setUVTexture)
                        foreach (var tri in new Plate(p1.v, p2.v, p3.v, p4.v).GetTriangles().WithIndex())
                        {
                            // - first triangle (P1, P2, P3)
                            if (tri.idx == 0)
                            {
                                tri.item.V1.TextureST = p1.textureST;
                                tri.item.V2.TextureST = p2.textureST;
                                tri.item.V3.TextureST = p3.textureST;
                            }

                            // - second triangle (P3, P4, P1)                        
                            else if (tri.idx == 1)
                            {
                                tri.item.V1.TextureST = p3.textureST;
                                tri.item.V2.TextureST = p4.textureST;
                                tri.item.V3.TextureST = p1.textureST;
                            }

                            yield return tri.item;
                        }

                    else
                        foreach (var tri in new Plate(p1.v, p2.v, p3.v, p4.v).GetTriangles())
                            yield return tri;

                }
            }

            if (w < 2 * N - 1)
            {
                prevArcpts = arcpts;

                arcpts = new List<VNfo>();
            }
        }

        if (firstArcpts is not null)
        {
            for (int k = 0; k < arcpts.Count - 1; ++k)
            {
                var p1 = arcpts[k];
                var p2 = firstArcpts[k];
                var p3 = firstArcpts[k + 1];
                var p4 = arcpts[k + 1];

                if (setUVTexture)
                    foreach (var tri in new Plate(p1.v, p2.v, p3.v, p4.v).GetTriangles().WithIndex())
                    {
                        // - first triangle (P1, P2, P3)
                        if (tri.idx == 0)
                        {
                            tri.item.V1.TextureST = p1.textureST;
                            tri.item.V2.TextureST = new Vector2(1, p2.textureST.Y);
                            tri.item.V3.TextureST = new Vector2(1, p3.textureST.Y);
                        }

                        // - second triangle (P3, P4, P1)                        
                        else if (tri.idx == 1)
                        {
                            tri.item.V1.TextureST = new Vector2(1, p3.textureST.Y);
                            tri.item.V2.TextureST = p4.textureST;
                            tri.item.V3.TextureST = p1.textureST;
                        }

                        yield return tri.item;
                    }

                else
                    foreach (var tri in new Plate(p1.v, p2.v, p3.v, p4.v).GetTriangles())
                        yield return tri;
            }
        }
    }

}
