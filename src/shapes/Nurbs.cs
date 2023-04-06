namespace SearchAThing.OpenGL.Shapes;

using GShark.Geometry;

public static partial class Ext
{

    /// <summary>
    /// G-Shark Point3 to Vector3
    /// </summary>    
    public static System.Numerics.Vector3 ToVector3(this Point3 p) =>
        new System.Numerics.Vector3((float)p.X, (float)p.Y, (float)p.Z);

    /// <summary>
    /// G-Shark Vector3 to Vector3
    /// </summary>    
    public static System.Numerics.Vector3 ToVector3(this in GShark.Geometry.Vector3 p) =>
        new System.Numerics.Vector3((float)p.X, (float)p.Y, (float)p.Z);

    /// <summary>
    /// Vector3 to G-Shark Point3
    /// </summary>
    public static Point3 ToPoint3(this in System.Numerics.Vector3 v) => new Point3(v.X, v.Y, v.Z);

    /// <summary>
    /// Enumerable Vector3 to Enumerable G-Shark Point3
    /// </summary>
    /// <param name="vs">Vector3 enumerable.</param>
    /// <returns>G-Shark Point3 enumerable.</returns>
    public static IEnumerable<Point3> ToPoint3(this IEnumerable<System.Numerics.Vector3> vs) =>
        vs.Select(v => v.ToPoint3());

}

public static partial class Toolkit
{

    /// <summary>
    /// G-Shark nurb to GLTriangle helper
    /// </summary>
    /// <param name="nurb">nurb</param>
    /// <param name="color">color of triangles generated</param>
    /// <param name="N">number of nurb divisions</param>
    /// <returns>triangles mesh of the nurb</returns>
    public static IEnumerable<GLTriangle> NurbToGL(this NurbsSurface nurb, Color color, int N = 6)
    {
        var u1 = 0d;
        var u2 = 0d;
        var v1 = 0d;
        var v2 = 0d;
        var step = 1d / N;

        for (int ui = 0; ui < N; ++ui)
        {
            v1 = 0;
            u2 = u1 + step;
            if (u2 > 1) u2 = Round(u2);//, N / 10 + 1);

            for (int vi = 0; vi < N; ++vi)
            {
                v2 = v1 + step;
                if (v2 > 1) v2 = Round(v2);//, N / 10 + 1);

                var p1 = nurb.PointAt(u1, v1).ToVector3();
                var p2 = nurb.PointAt(u2, v1).ToVector3();
                var p3 = nurb.PointAt(u2, v2).ToVector3();
                var p4 = nurb.PointAt(u1, v2).ToVector3();

                var plate = new Plate(p1, p2, p3, p4);

                foreach (var tri in plate.GetTriangles(color))
                {
                    yield return tri;
                }

                v1 += step;
            }

            u1 += step;
        }
    }

}