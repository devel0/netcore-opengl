using Avalonia.Threading;

namespace example;

// example-0017
// figure using screen coord
//
// - try to pan and rotate to see that top left screen text and cyan border doesn't affect from
// - press space to toggle task pause
// - press application Control+b to toggle bounding box

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create();

        GLVertexManager? customVtxMgr = null;

        long ms = 0;
        var showTextBBox = false;

        Task? task = null;
        bool taskPause = false;
        var are = new AutoResetEvent(false);

        task = Task.Run(async () =>
        {
            while (true)
            {
                if (taskPause)
                    are.WaitOne();

                var glCtl = w.FocusedControl;
                if (glCtl is null || customVtxMgr is null)
                {
                    await Task.Delay(250);
                    continue;
                }

                var glCtx = glCtl.GLContext;
                var glModel = glCtl.GLModel;
                var vtxMgr = glModel.GLVertexManager;

                var viewportSize = glCtl.Size();

                var sw = viewportSize.X;
                var sh = viewportSize.Y;

                var margin = 50;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    customVtxMgr.Clear();

                    // regen screen coord text
                    {
                        var txtFigs = glModel.MakeTextFigure(
                            new GLText(
                                // use of MakeScreenCS instead of MakeWCS in order to
                                // have Y downward as the screen desktop (origin 0,0 is left,top)
                                cs: MakeScreenCS(new Vector3(margin, margin, 0)),
                                text: $"{ms}",
                                height: 50,
                                alignment: GLTextVHAlignment.BottomLeft));
                        txtFigs.ForEach(fig => fig.ScreenCoordMode = true); // set screencoord for the figure
                        customVtxMgr.AddFigure(txtFigs);

                        if (showTextBBox)
                        {
                            var bboxFig = new BBox(txtFigs.Select(w => w.OBBox())).MakeFigure(Color.Yellow);
                            bboxFig.ScreenCoordMode = true; // set screencoord for the figure
                            customVtxMgr.AddFigure(bboxFig);
                        }
                    }

                    // regen model space text
                    {
                        var txtFigs = glCtx.MakeTextFigure(
                            new GLText(WCS.Move(new Vector3(50, 50, 0)), $"{ms}",
                            50, alignment: GLTextVHAlignment.BottomLeft));
                        customVtxMgr.AddFigure(txtFigs);

                        if (showTextBBox)
                        {
                            var bboxFig = new BBox(txtFigs.Select(w => w.OBBox())).MakeFigure(Color.Yellow);
                            customVtxMgr.AddFigure(bboxFig);
                        }
                    }

                    glCtl.Invalidate();

                    ++ms;
                });

                await Task.Delay(50);
            }
        });

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glCtx = glCtl.GLContext;
            var glModel = glCtl.GLModel;

            glModel.Clear();

            customVtxMgr = new GLVertexManager();
            glModel.AddCustomVertexManager(customVtxMgr);

            // draw two lines in model space
            glModel.AddFigure(new GLLineFigure(GLLine.FromTo(new Vector3(0, 0, 0), new Vector3(100, 100, 0))));
            glModel.AddFigure(new GLLineFigure(GLLine.FromTo(new Vector3(0, 100, 0), new Vector3(100, 0, 0))));

            // add a rectangle overlay using screencoord
            var viewportSize = glCtl.Size();

            var sw = viewportSize.X;
            var sh = viewportSize.Y;

            {
                glModel.AddFigure(new GLLineFigure(new[]
                {
                    GLLine.FromTo(new Vector2(0, 0), new Vector2(sw, 0)),
                    GLLine.FromTo(new Vector2(sw, 0), new Vector2(sw, sh)),
                    GLLine.FromTo(new Vector2(sw, sh), new Vector2(0, sh)),
                    GLLine.FromTo(new Vector2(0, sh), new Vector2(0, 0)),

                    GLLine.FromTo(new Vector2(2, 2), new Vector2(sw-2, 2)),
                    GLLine.FromTo(new Vector2(sw-2, 2), new Vector2(sw-2, sh-2)),
                    GLLine.FromTo(new Vector2(sw-2, sh-2), new Vector2(2, sh-2)),
                    GLLine.FromTo(new Vector2(2, sh-2), new Vector2(2, 2)),
                })
                .Act(w =>
                {
                    w.SetColor(Color.Cyan);
                    w.ScreenCoordMode = true; // set screencoord for the figure
                }));
            }

            glCtl.LoadView();
        };

        w.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Space)
            {
                if (task is not null)
                {
                    taskPause = !taskPause;

                    are.Set();
                }
            }

            else if (e.Key == Key.B && e.KeyModifiers == KeyModifiers.Control)
            {
                showTextBBox = !showTextBBox;
            }
        };

        w.SizeChanged += (a, b) => w.GLControlSplit?.Invalidate(InvalidateEnum.RebuildModelAndRedraw);

        w.ShowSync();
    }

}
