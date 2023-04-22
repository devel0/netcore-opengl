namespace SearchAThing.OpenGL.Nurbs;

using GShark.Geometry;

public static partial class Toolkit
{

    /// <summary>
    /// G-Shark nurb to GLTriangle helper
    /// </summary>
    /// <param name="nurb">nurb</param>
    /// <param name="color">color of triangles generated</param>
    /// <param name="N">number of nurb divisions</param>
    /// <returns>triangles mesh of the nurb</returns>
    public static IEnumerable<GLTriangle> NurbToGL(this NurbsSurface nurb, Color? color = null, int N = 6)
    {
        var u1 = 0d;
        var u2 = 0d;
        var v1 = 0d;
        var v2 = 0d;
        var step = 1d / N;

        int triCnt = 0;

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

                    ++triCnt;
                }

                v1 += step;
            }

            u1 += step;
        }
    }

    /// <summary>
    /// G-Shark nurb to GLLine helper
    /// </summary>
    /// <param name="curve">nurb curve</param>    
    /// <param name="N">number of nurb divisions</param>
    /// <returns>lines representing given nurb curve</returns>
    public static IEnumerable<OpenGL.Core.Line> NurbToGL(this NurbsCurve curve, int N = 6)
    {
        var t = 0d;
        var step = 1d / N;

        var p = curve.PointAt(0);

        for (int i = 0; i < N; ++i)
        {
            var nextp = curve.PointAt(t + step);
            yield return OpenGL.Core.Line.FromTo(p.ToVector3(), nextp.ToVector3());

            p = nextp;

            t += step;
        }
    }


}