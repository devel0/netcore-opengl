using GShark.Core;
using GShark.Geometry;
using Plane = GShark.Geometry.Plane;

using SearchAThing.OpenGL.Nurbs;


namespace example;

// example-0022
// not yet implemented ( https://github.com/GSharker/G-Shark/pull/412 )

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create(
            onFocusedControlChanged: (split, AvaloniaGLControl, isInitial) =>
            {
                if (isInitial)
                    split.LoadViewLayout();
            }
        );

        GLTriangleFigure? joinFig = null;

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            var RAIL_DIVS = 4;
            var TUBE_DIVS = 20;
            var GLNURB_DIVS = TUBE_DIVS;

            glModel.Clear();

            glModel.AddFigure(MakeWCSFigure());

            var tube1 = new Cone(
                baseCS: (WCS * Matrix4x4.CreateRotationX((float)(PI / 2)) * Matrix4x4.CreateRotationZ(-(float)(PI / 4)))
                    .Move(-1f, -1f, 0),
                baseRadius: 1, topRadius: 1, height: 6,
                bottomCap: false, topCap: false).Figure(divisions: TUBE_DIVS);

            var tube2 = tube1.Mirror(YZCS)!;

            glModel.AddFigure(tube1);
            glModel.AddFigure(tube2);

            glModel.PointLights.Add(new GLPointLight(0, -1, 2));

            // grab tube vertexes near join as profile to sweep

            var profilePts = tube1.Vertexes().Where(r => r.Position.Length() < 2).Select(w => w.Position).ToList();
            // var profileCenter = profilePts[0];
            var profileCenter = profilePts.Mean();
            // var profileCenter = new Vector3(-1, -1, 0);

            glModel.AddFigure(new GLPointFigure(profilePts).SetColor(Color.Green));

            var railPts = new List<Vector3>();
            {
                var N = RAIL_DIVS;
                var alpha = 0f;
                var alphaStep = (float)(PI / 2 / N);
                var rotCenter = new Vector3(0, -2f, 0);
                for (int i = 0; i < N + 1; ++i)
                {
                    railPts.Add(Vector3.Transform(profileCenter, Matrix4x4.CreateRotationZ(-alpha, rotCenter)));
                    alpha += alphaStep;
                }
            }

            var rail = new NurbsCurve(railPts.ToPoint3().ToList(), 1);             

            var sw = new Stopwatch();
            sw.Start();
            var sweepNurb = NurbsSurface.FromSweep(
                rail,
                new NurbsCurve(profilePts.ToPoint3().ToList(), 1));
            sw.Stop();
            Debug.WriteLine($"FromSweep time:{sw.Elapsed}");

            {
                var (tValues, _) = GShark.Sampling.Curve.AdaptiveSample(rail, GSharkMath.MaxTolerance);
                var frames = rail.PerpendicularFrames(tValues);
                foreach (var frame in frames)
                {
                    var cs = MakeCS(frame.Origin.ToVector3(), frame.XAxis.ToVector3(), frame.YAxis.ToVector3(), frame.ZAxis.ToVector3());
                    glModel.AddFigure(MakeCSFigure(cs));
                }
            }

            var railFig = new GLPointFigure(railPts).SetColor(Color.Yellow);
            glModel.AddFigure(railFig);

            joinFig = sweepNurb.NurbToGL(Color.Red, N: GLNURB_DIVS).ToFigure(); ;
            glModel.AddFigure(joinFig);

            // glCtl.CameraView(CameraViewType.Top);
            // glCtl.LoadView();
        };

        w.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Space)
            {
                if (joinFig is not null) { joinFig.Visible = !joinFig.Visible; w.GLControlSplit?.Invalidate(); }
            }
        };

        w.ShowSync();
    }

}