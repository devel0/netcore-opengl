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
    public GLTriangleFigure Figure(int divisions)
    {
        var fig = new GLTriangleFigure(Triangles(divisions));

        fig.SetupComputeNormal(computeNormal: (tri, v) => Vector3.Normalize(v.Position - Center));

        return fig;
    }

    /// <summary>
    /// Gl triangles that represents the sphere.<br/>
    /// The <see cref="GLTriangleFigure.ComputeNormal"/> is approriately set to compute vertex sphere normals.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure. (Min:4)</param>    
    public IEnumerable<GLTriangle> Triangles(int divisions)
    {
        if (divisions < 2) throw new ArgumentException($"divisions must >= 2");

        // techdoc: UVSphere.dxf

        var R = Radius;
        var C = Center;
        var N = divisions;

        // arc pts goes from a pole (0,0,-r) to other (0,0,r)
        var arcpts = new List<Vector3>();
        List<Vector3>? prevArcpts = null;

        List<Vector3>? firstArcpts = null;

        for (int w = 0; w < 2 * N; ++w)
        {
            for (int k = 0; k <= N; ++k)
            {
                Vector3 p;

                if (k == 0)
                    p = C - Vector3.UnitZ * R;

                else if (k == N)
                    p = C + Vector3.UnitZ * R;

                else
                {
                    var alpha = (float)(k * PI / N);
                    var phi = (float)(w * PI / N);

                    p = C +
                        Vector3.Transform(
                            Vector3.Transform(-Vector3.UnitZ * R, Matrix4x4.CreateRotationX(alpha)),
                            Matrix4x4.CreateRotationZ((float)(-PI / 2 + phi))
                        );
                }

                arcpts.Add(p);
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

                    foreach (var tri in new Plate(p1, p2, p3, p4).GetTriangles())
                        yield return tri;
                }
            }

            if (w < 2 * N - 1)
            {
                prevArcpts = arcpts;

                arcpts = new List<Vector3>();
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

                foreach (var tri in new Plate(p1, p2, p3, p4).GetTriangles())
                    yield return tri;
            }
        }
    }

}
