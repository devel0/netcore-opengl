using Avalonia.Threading;
using GShark.Geometry;

namespace example;

// example-0025
/// Nurb surface intersection generating nurb curves using FeasibleTriIntersectionTests extension method.
//
// - use application 1,2,3 keys to regenerate nurb curves within different divisions
// - press application speace key to toggle hit test triangles
//
// notes: NURBSURF_CALC_DIVS is the nr. of division for the nurb surface in order to compute
//        nurb curves by triangle-triangle intersections. This could lower than
//        NURBSURF_FIGURE_DIVS but results in a decent approximation, then the curve
//        is divided by NURBCURVE_DIVS.

class Program
{

    class NurbIntNfo
    {
        public GLTriangle tri1;
        public GLTriangle tri2;
        public SearchAThing.OpenGL.Core.Line? line;
        public bool iskip;
    }

    static void Main(string[] args)
    {
        InitAvalonia();

        GLTriangleFigure? testedTriFig = null;

        var w = GLWindow.Create(
            onGLControlCreated: (avaloniaGLControl) =>
            {
                avaloniaGLControl.GLControl.ControlFigureVisible = (glControl, glFigure) =>
                {
                    // shows tested triangle only on views with the title containing specific word
                    if (glFigure == testedTriFig)
                        return glControl.Title.Contains("TRIHIT");

                    return true;
                };
            },

            onFocusedControlChanged: (split, AvaloniaGLControl, isInitial) =>
            {
                if (isInitial)
                {
                    split.LoadViewLayout();
                }
            }
        );

        GLTriangleFigure? fig1 = null;
        GLVertexManager nurbCurvesVtxMgr = new GLVertexManager();
        List<NurbsCurve> nurbCurves = new List<NurbsCurve>();

        int NURBCURVE_DIVS = 50;
        var NURBSURF_FIGURE_DIVS = 40;
        var NURBSURF_CALC_DIVS = 20;
        var USE_DISCRETE_SPACE = true; // set to false to try without discrete space

        void RedrawNurbCurves()
        {
            nurbCurvesVtxMgr.Clear();
            foreach (var curve in nurbCurves)
            {
                var lines = curve.NurbToGL(NURBCURVE_DIVS).ToFigure().SetColor(Color.Yellow);
                nurbCurvesVtxMgr.AddFigure(lines);
            }
        }

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            float ALPHA = .5f;

            glModel.Clear();

            glModel.AddCustomVertexManager(nurbCurvesVtxMgr);

            glModel.AddFigure(MakeWCSFigure());

            // data pts from: https://github.com/pboyer/verb/blob/3692d7c5f41aab43eef457e74fc0e0513b252cbd/examples/surfaceIntersection.html
            var pts1 = new List<List<Point3>>()
            {
                new List<Point3>() { new Point3(0, 0, -10), new Point3(10, 0, 0), new Point3(20, 0, 0), new Point3(30, 0, 0), new Point3(40, 0, 0), new Point3(50, 0, 9) },
                new List<Point3>() { new Point3(0, -10, 0), new Point3(10, -10, 10), new Point3(20, -10, 10), new Point3(30, -10, 0), new Point3(40, -10, 0), new Point3(50, -10, 0) },
                new List<Point3>() { new Point3(0, -20, 0), new Point3(10, -20, 10), new Point3(20, -20, 10), new Point3(30, -20, 0), new Point3(40, -20, -2), new Point3(50, -20, 0) },
                new List<Point3>() { new Point3(0, -30, 0), new Point3(10, -30, 0), new Point3(20, -30, 0), new Point3(30, -30, 0), new Point3(40, -30, 0), new Point3(50, -30, 0) },
                new List<Point3>() { new Point3(0, -40, 0), new Point3(10, -40, 0), new Point3(20, -40, 0), new Point3(30, -40, 4), new Point3(40, -40, -20), new Point3(50, -40, 0) },
                new List<Point3>() { new Point3(0, -50, 12), new Point3(10, -50, 0), new Point3(20, -50, 0), new Point3(30, -50, 0), new Point3(50, -50, 0), new Point3(50, -50, 15) },
            };
            var surf1 = NurbsSurface.FromPoints(3, 3, pts1);
            var tris1 = surf1.NurbToGL(Color.White.SetAlphaF(ALPHA), N: NURBSURF_FIGURE_DIVS).ToList();
            var tris1Calc = surf1.NurbToGL(Color.White.SetAlphaF(ALPHA), N: NURBSURF_CALC_DIVS).ToList();
            fig1 = tris1.ToFigure();
            fig1.SetupComputeNormal(mean: true);
            glModel.AddFigure(fig1);

            var surf2 = NurbsSurface.FromCorners(
                new Point3(50, -50, 3),
                new Point3(50, 0, 3),
                new Point3(0, 0, 3),
                new Point3(0, -50, 5));
            var tris2 = surf2.NurbToGL(N: NURBSURF_FIGURE_DIVS).ToList();
            var tris2Calc = surf2.NurbToGL(N: NURBSURF_CALC_DIVS).ToList();
            var fig2 = tris2.ToFigure().SetColor(Color.Cyan.SetAlphaF(ALPHA));
            fig2.SetupComputeNormal(mean: true);
            glModel.AddFigure(fig2);

            var tol = glModel.LBBox.TolHint;

            // var allTris = tris1.Union(tris2).ToList();

            List<NurbsCurve> Intersect(NurbsSurface surf1, NurbsSurface surf2, out List<NurbIntNfo> intNfos)
            {
                var res = new List<NurbsCurve>();

                var CNT = 0;

                var sw = new Stopwatch();
                sw.Start();
                if (USE_DISCRETE_SPACE)
                {
                    var F = tris1Calc
                        .FeasibleTriIntersectionTests(tol, tris2Calc)
                        .ToList();

                    CNT = F.Count;

                    intNfos =
                    (
                        F
                        .Select(x => new NurbIntNfo
                        {
                            tri1 = x.tri1,
                            tri2 = x.tri2,
                            line = x.tri1.Intersect(tol, x.tri2, out var iskip),
                            iskip = iskip
                        })
                    )
                    .Where(r => !r.iskip)
                    .ToList();
                }
                else
                {
                    CNT = tris1Calc.Count * tris2Calc.Count;

                    intNfos =
                    (
                        from tri1 in tris1Calc
                        from tri2 in tris2Calc
                        select new NurbIntNfo
                        {
                            tri1 = tri1,
                            tri2 = tri2,
                            line = tri1.Intersect(tol, tri2, out var iskip),
                            iskip = iskip
                        }
                    )
                    .Where(r => !r.iskip)
                    .ToList();
                }

                sw.Stop();

                var grps = GroupByAdjacency(tol, intNfos.Select(r => r.line!).ToList());

                foreach (var lines in grps.Where(r => r.Count > 1))
                {
                    var pts = lines.DistinctPoints(tol).Select(w => w.ToPoint3()).ToList();
                    res.Add(new NurbsCurve(pts, degree: 2));
                }

                Dispatcher.UIThread.Post(() => glModel.SendNotification("Stats",
                    $"Adjacency groups: {grps.Count}\n" +
                    $"- {CNT} tris test in {sw.ElapsedMilliseconds} ms",
                    grps.Count == 4 ? GLNotificationType.Information : GLNotificationType.Warning));

                return res;
            }

            nurbCurves = Intersect(surf1, surf2, out var intNfos);

            // if (intNfos.Count > 0)
            //     glModel.AddFigure(intNfos.Where(r => r.line is not null).Select(w => w.line!).ToFigure().SetColor(Color.Yellow));

            RedrawNurbCurves();

            var vtxCnt = glModel.GLVertexManager.VertexCount;

            // this figure not increase the nr. of vertex in the vtx manage
            testedTriFig = intNfos
                .Select(w => w.tri1).Union(intNfos.Select(w => w.tri2)).ToHashSet()
                .Select(w => (GLTriangle)w.Copy())
                .ToFigure()
                .Act(fig =>
                {
                    fig.Order = 1;
                    fig.SetColor(Color.White);
                });

            glModel.AddFigure(testedTriFig);
        };

        w.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Space && testedTriFig is not null)
            {
                testedTriFig.Visible = !testedTriFig.Visible;
                w.Invalidate();
            }

            if (e.Key == Key.D1 || e.Key == Key.D2 || e.Key == Key.D3)
            {
                NURBCURVE_DIVS = 3 + 6 * (int)Pow((((int)e.Key) - ((int)Key.D1)), 2);
                RedrawNurbCurves();
                w.GLModel.SendNotification("", $"nurb curves DIVS:{NURBCURVE_DIVS}");
                w.Invalidate();
            }
        };

        w.ShowSync();
    }

}