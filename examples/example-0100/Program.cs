namespace sample;

using SearchAThing.OpenGL.Core;
using static SearchAThing.OpenGL.Core.Constants;
using SearchAThing.OpenGL.GUI;
using static SearchAThing.OpenGL.GUI.Toolkit;
using SearchAThing.OpenGL.Shapes;
using Avalonia.Input;
using DynamicData;

class Program
{
    static GLControl gLControl;

    static GLVertexManager tmpVtxMgr = new GLVertexManager(expandModelBBox: false);
    static GLTriangleFigure? sphereFig;
    static object TAG_YELLOW_PTS = new();

    static float RADIUS = 25f;
    static float RAY_TOLERANCE = 4f;

    static int SPHERE_DIVS = 50;

    static void Main(string[] args)
    {
        // this must called for console application to enable Avalonia framework
        // and must called before any other Avalonia control usage
        InitAvalonia();

        // create standalone Avalonia window for Silk.NET opengl rendering
        var w = GLWindow.Create();

        // define the GLModel build function
        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            gLControl = glCtl;

            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            // clear the model
            glModel.Clear();

            glModel.AddCustomVertexManager(tmpVtxMgr);

            glModel.PointLights.Add(new GLPointLight(52, -52, 52));

            // create and add a sphere centered at (0,0,0) with radius=1 and meshed as uvsphere with N=20 divisions
            var sphere = new UVSphere(center: Vector3.Zero, radius: RADIUS);
            sphereFig = sphere.Figure(divisions: SPHERE_DIVS).SetColor(Color.Cyan);
            glModel.AddFigure(sphereFig);

            var basebox = new Box(cs: WCS.Move(sphere.Center - Vector3.UnitZ * 70), csSize: new Vector3(150, 150, 1f));
            glModel.AddFigure(basebox.Sides);

            glCtl.CameraView(CameraViewType.Front);

            var avaloniaGLControl = w.GLControlSplit?.FocusedControl?.AvaloniaGLControl;

            if (avaloniaGLControl is not null)
            {
                avaloniaGLControl.ClearKeyGestures();
                avaloniaGLControl.ToggleWireframeGesture = DEFAULT_ToggleWireframeGesture;
                avaloniaGLControl.ViewTopGesture = DEFAULT_ViewTopGesture;
                avaloniaGLControl.ViewFrontGesture = DEFAULT_ViewFrontGesture;
            }

            w.NotificationManager?.Show(new Avalonia.Controls.Notifications.Notification(
                "Gestures",
                "KEYS  => C:clear   U:undo   SPC:toggle sphere   C-SPC:toggle yellow pts\n" +
                "MOUSE => LeftHold:rotate   Right:pick pt\n" +
                "VIEWS => F:front   T:top",
                Avalonia.Controls.Notifications.NotificationType.Information,
                TimeSpan.Zero));
        };

        w.PointerPressed += MainWindow_PointerPressed;
        w.KeyDown += MainWindow_KeyPressed;

        // show the gl window
        w.ShowSync();
    }

    private static void MainWindow_KeyPressed(object? sender, KeyEventArgs e)
    {
        var glWindow = sender as GLWindow;

        var glModel = glWindow?.GLModel;

        if (glModel is null) return;

        var avaloniaGLControl = glWindow?.GLControlSplit?.FocusedControl?.AvaloniaGLControl;

        if (avaloniaGLControl is null) return;

        var glControl = avaloniaGLControl.GLControl;

        if (glControl is null) return;

        switch (e.Key)
        {

            // show/hide sphere / yellow pts
            case Key.Space:
                {
                    if (e.KeyModifiers == KeyModifiers.Control)
                    {
                        var q = glModel.Figures.Where(r => r.Tag == TAG_YELLOW_PTS).ToList();

                        foreach (var x in q) x.Visible = !x.Visible;

                        glControl.Invalidate();
                    }

                    else if (sphereFig is not null)
                    {
                        sphereFig.Visible = !sphereFig.Visible;
                        glControl.Invalidate();
                    }
                }
                break;

            // clear figs
            case Key.C:
                tmpVtxMgr.Clear();
                latestHitPt = null;
                glControl.Invalidate();
                break;

            // remove last fig
            case Key.U:
                {
                    if (tmpVtxMgr.FigureCount > 0)
                    {
                        tmpVtxMgr.RemoveFigure(tmpVtxMgr.Figures.Last());

                        glControl.Invalidate();
                    }
                }
                break;
        }

    }

    static Vector3? latestHitPt = null;

    private static void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var glWindow = sender as GLWindow;

        var glModel = glWindow?.GLModel;

        if (glModel is null) return;

        var avaloniaGLControl = glWindow?.GLControlSplit?.FocusedControl?.AvaloniaGLControl;

        if (avaloniaGLControl is null) return;

        var glControl = avaloniaGLControl.GLControl;

        if (glControl is null) return;

        var cp = e.GetCurrentPoint(avaloniaGLControl);

        if (cp.Properties.IsRightButtonPressed)
        {
            var p = e.GetPosition(avaloniaGLControl).ToVector2();

            var lraycast = glControl.RayCastLocal(screen: p);

            // - gather all vertexes visible figure
            // - grouped by figure
            // - converting lraycast to oraycast by figure objectmatrix
            // - selecting those vertexes which distance from the oraycast lower a tolerance
            //   saving ocoord of the vertex and projection distance resulting from the contains test
            // - sort by the projection distance
            var q = glModel.Figures
                .Where(fig => fig.Visible && fig.Tag != TAG_YELLOW_PTS)
                .SelectMany(fig => fig.Vertexes().Select(vtx => new { fig = fig, vtx = vtx }))
                .GroupBy(nfo => nfo.fig)
                .Select(nfo => new
                {
                    vtxs = nfo.Select(v => v.vtx),
                    fig = nfo.Key,
                    oraycast = nfo.Key.ObjectMatrixIsIdentity ? lraycast : lraycast.Transform(nfo.Key.ObjectMatrix.Inverse())
                })
                .SelectMany(nfo => nfo.vtxs
                .Select(vtx => new
                {
                    projDst = nfo.oraycast.Contains(RAY_TOLERANCE, vtx.Position, out var prj),
                    ocoord = Vector3.Transform(vtx.Position, nfo.fig.ObjectMatrix.Inverse()),
                    vtxWorld = Vector3.Transform(vtx.Position, glControl.ModelMatrix),
                    cameraDst = Vector3.DistanceSquared(
                        Vector3.Transform(vtx.Position, glControl.ModelMatrix), glControl.CameraPos),
                    vtx = vtx
                })
                .Where(nfo => nfo.projDst is not null)
                .OrderBy(nfo => nfo.projDst))
                .ToList();

            if (q.Count > 0)
            {
                var cameraNearest = q.OrderBy(w => w.cameraDst).First();

                var backFiltered = q.Where(w =>
                    Vector3.Distance(w.vtxWorld, cameraNearest.vtxWorld) < RADIUS)
                    .OrderBy(w => w.projDst)
                    .ToList();

                var matching = backFiltered.First();

                if (latestHitPt is not null)
                {
                    var from = latestHitPt;
                    var to = matching.vtx.Position;

                    var segFig = new GLLineFigure(GLLine.FromTo(from.Value, to, Color.White));

                    tmpVtxMgr.AddFigure(segFig);
                }

                latestHitPt = matching.vtx.Position;

                foreach (var x in backFiltered)
                {
                    var isMatch = x == matching;

                    var glPoint = new GLPoint(x.ocoord, isMatch ? Color.Magenta : Color.Yellow);

                    var ptFig = new GLPointFigure(glPoint);

                    if (!isMatch) ptFig.Tag = TAG_YELLOW_PTS;

                    tmpVtxMgr.AddFigure(ptFig);
                }
            }

            ;

        }

        glControl.Invalidate();

        ;
    }

}