using GShark.Geometry;

namespace example;

public static partial class Toolkit
{

    public static GLTriangleFigure Example0007(
        int SURF_DIVS = 100,
        float REVOLVE_ANGLE = (float)PI,
        float FIG_RADIUS = 300.0f,
        float TUBE_SPAN = 1000.0f)
    {
        var circleCenter = new Point3();
        var NcircleCenter = circleCenter.ToVector3();

        var revolveCenter = TUBE_SPAN * new Point3(1, 0, 0);

        var revolveDirection = new GShark.Geometry.Vector3(0, 1, 0);

        var circleRadius = FIG_RADIUS;

        var circle = new Circle(
            circleCenter + new Point3(0, -1, 0) * circleRadius,
            circleCenter + new Point3(1, 0, 0) * circleRadius,
            circleCenter + new Point3(0, 1, 0) * circleRadius);

        var NrevolveCenter = revolveCenter.ToVector3();

        var revolveAxis = new Ray(revolveCenter, revolveDirection);

        var rotationAngle = REVOLVE_ANGLE;

        var figColor = Color.FromArgb(69, 98, 148);

        var nurb = NurbsSurface.Revolved(circle, revolveAxis, rotationAngle);

        var glNurb = nurb.NurbToGL(figColor, SURF_DIVS).ToList();

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

        var fig = new GLTriangleFigure(glNurb.AsEnumerable());

        fig.SetupComputeNormal(computeNormal: (tri, vtx) =>
        {
            var q = revolvePathNurb!.ClosestPoint(vtx.Position.ToPoint3());

            return Vector3.Normalize(vtx.Position - q.ToVector3());
        });

        return fig;
    }

}
