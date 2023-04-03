using GVector3 = GShark.Geometry.Vector3;
using GShark.Geometry;

namespace example;

public static partial class Toolkit
{

    /// <summary>
    /// G-Shark Point3 to Vector3
    /// </summary>    
    public static System.Numerics.Vector3 ToVector3(this Point3 p) =>
        new System.Numerics.Vector3((float)p.X, (float)p.Y, (float)p.Z);

    /// <summary>
    /// G-Shark Vector3 to Vector3
    /// </summary>    
    public static System.Numerics.Vector3 ToVector3(this GVector3 p) =>
        new System.Numerics.Vector3((float)p.X, (float)p.Y, (float)p.Z);

    /// <summary>
    /// Vector3 to G-Shark Point3
    /// </summary>
    public static Point3 ToPoint3(Vector3 v) => new Point3(v.X, v.Y, v.Z);

    /// <summary>
    /// G-Shark nurb to GLTriangle helper
    /// </summary>
    /// <param name="nurb">nurb</param>
    /// <param name="color">color of triangles generated</param>
    /// <param name="N">number of nurb divisions</param>
    /// <returns>triangles mesh of the nurb</returns>
    public static IEnumerable<GLTriangle> NurbToGL(NurbsSurface nurb, Color color, int N = 6)
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
            if (u2 > 1) u2 = Round(u2, N / 10 + 1);

            for (int vi = 0; vi < N; ++vi)
            {
                v2 = v1 + step;
                if (v2 > 1) v2 = Round(v2, N / 10 + 1);

                var p1 = ToVector3(nurb.PointAt(u1, v1));
                var p2 = ToVector3(nurb.PointAt(u2, v1));
                var p3 = ToVector3(nurb.PointAt(u2, v2));
                var p4 = ToVector3(nurb.PointAt(u1, v2));

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

    public static GLTriangleFigure Example0007(
        int SURF_DIVS = 100,
        float REVOLVE_ANGLE = (float)PI,
        float FIG_RADIUS = 300.0f,
        float TUBE_SPAN = 1000.0f)
    {
        var circleCenter = new Point3();
        var NcircleCenter = ToVector3(circleCenter);

        var revolveCenter = TUBE_SPAN * new Point3(1, 0, 0);

        var revolveDirection = new GVector3(0, 1, 0);

        var circleRadius = FIG_RADIUS;

        var circle = new Circle(
            circleCenter + new Point3(0, -1, 0) * circleRadius,
            circleCenter + new Point3(1, 0, 0) * circleRadius,
            circleCenter + new Point3(0, 1, 0) * circleRadius);

        var NrevolveCenter = ToVector3(revolveCenter);

        var revolveAxis = new Ray(revolveCenter, revolveDirection);

        var rotationAngle = REVOLVE_ANGLE;

        var figColor = Color.FromArgb(69, 98, 148);

        var nurb = NurbsSurface.Revolved(circle, revolveAxis, rotationAngle);

        var glNurb = NurbToGL(nurb, figColor, SURF_DIVS).ToList();

        var u = 0d;
        var step = 1d / SURF_DIVS;

        var revolve_path = new List<Point3>();

        for (int ui = 0; ui <= SURF_DIVS; ++ui)
        {
            if (u > 1) u = u.MRound(1e-6);

            var p1 = nurb.PointAt(u, 0);
            var p2 = nurb.PointAt(u, 0.5);
            var pm = (p1 + p2) / 2;

            revolve_path.Add(pm);

            u += step;
        }

        var revolvePathNurb = new NurbsCurve(revolve_path, 3);

        return new GLTriangleFigure(glNurb.AsEnumerable())
        {
            ComputeNormal = (tri, vtx) =>
            {
                var q = revolvePathNurb!.ClosestPoint(ToPoint3(vtx.Position));

                return Vector3.Normalize(vtx.Position - ToVector3(q));
            }
        };

    }

}
