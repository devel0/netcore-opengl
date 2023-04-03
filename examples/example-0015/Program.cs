namespace example;

// example-0015
// raycast in orthogonal mode for snapping test

class Program
{

    static void Main(string[] args)
    {
        var GRID_SIZE = 2f;
        var GRID_STEP = .1f;
        var PAD = .25f;
        var SNAP = GRID_STEP / 5;

        InitAvalonia();

        // to avoid reprocess vertex array buffer here we use a separate vertex manager
        // where to draw dynamic temp entities such as tracking horiz/vert lines.
        // this will created once and registered in BuildModel and cleared during pointer moved handler
        GLVertexManager? customVtxMgr = null;

        var w = GLWindow.Create(
            onFocusedControlChanged: (glSplit, avaloniaGLControl, isInitial) =>
            {
                if (!isInitial) return;

                var glCtl = avaloniaGLControl.GLControl;
                var glModel = glCtl.GLModel;

                avaloniaGLControl.PointerMoved += (a, e) =>
                {
                    if (customVtxMgr is null) return;

                    var p = e.GetPosition(avaloniaGLControl).ToVector2();

                    var size = glCtl.Device.Size.ToVector2();

                    var lcoord = glCtl.RayCastLocal(screen: p).From;

                    customVtxMgr.Clear();

                    customVtxMgr.AddFigure(
                        MakeRectangle(
                            csCenter: WCS,
                            w: GRID_SIZE + 2 * PAD,
                            h: GRID_SIZE + 2 * PAD,
                            Color.Blue));

                    var rightTop = new Vector3(GRID_SIZE / 2 + PAD, GRID_SIZE / 2 + PAD, 0);
                    var rightBottom = new Vector3(GRID_SIZE / 2 + PAD, -GRID_SIZE / 2 - PAD, 0);

                    customVtxMgr.AddFigure(glModel.MakeTextFigure(new GLText(
                        cs: WCS.Move(rightTop),
                        text: $"{lcoord}",
                        height: PAD / 2,
                        alignment: GLTextVHAlignment.TopRight)));

                    #region draw horizontal matching line
                    {
                        var xrest = lcoord.X % GRID_STEP;

                        var xMatches =
                            lcoord.X >= -GRID_SIZE / 2 && lcoord.X <= GRID_SIZE / 2 &&
                            (xrest <= SNAP || xrest >= GRID_STEP - SNAP);

                        var xMatchVal = 0f;

                        if (xMatches)
                        {
                            xMatchVal = (float)Round(lcoord.X / GRID_STEP) * GRID_STEP;

                            var fig = new GLLineFigure(GLLine.FromTo(
                                new GLVertex(new Vector3(xMatchVal, -GRID_SIZE / 2, 0)),
                                new GLVertex(new Vector3(xMatchVal, GRID_SIZE / 2, 0))))
                            {
                                Order = 1
                            };

                            fig.SetColor(Color.Yellow);

                            customVtxMgr.AddFigure(fig);
                        }
                    }
                    #endregion

                    #region draw vertical matching line
                    {

                        var yrest = lcoord.Y % GRID_STEP;

                        var yMatches =
                           lcoord.Y >= -GRID_SIZE / 2 && lcoord.Y <= GRID_SIZE / 2 &&
                           (yrest <= SNAP || yrest >= GRID_STEP - SNAP);

                        var yMatchVal = 0f;

                        if (yMatches)
                        {
                            yMatchVal = (float)Round(lcoord.Y / GRID_STEP) * GRID_STEP;

                            var fig = new GLLineFigure(GLLine.FromTo(
                                new GLVertex(new Vector3(-GRID_SIZE / 2, yMatchVal, 0)),
                                new GLVertex(new Vector3(GRID_SIZE / 2, yMatchVal, 0))))
                            {
                                Order = 1
                            };

                            fig.SetColor(Color.Yellow);

                            customVtxMgr.AddFigure(fig);
                        }
                    }
                    #endregion

                    glCtl.ZoomFit();
                };
            }
        );

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            customVtxMgr = new GLVertexManager();
            glModel.AddCustomVertexManager(customVtxMgr);

            glModel.AddFigure(MakeWCSFigure(size: GRID_SIZE / 2).Act(fig => fig.Order = 1));

            {
                var lines = new List<GLLine>();

                float x = -GRID_SIZE / 2;
                while (x.LessThanOrEqualsTol(1e-2f, 1))
                {
                    lines.Add(new Vector3(x, -1, 0).LineV(Vector3.UnitY * 2, Color.Gray));
                    x += GRID_STEP;
                }

                float y = -GRID_SIZE / 2;
                while (y.LessThanOrEqualsTol(1e-2f, 1))
                {
                    lines.Add(new Vector3(-1, y, 0).LineV(Vector3.UnitX * 2, Color.Gray));
                    y += GRID_STEP;
                }

                glModel.AddFigure(new GLLineFigure(lines));
            }

            glCtl.Perspective = false;
            glCtl.CameraView(CameraViewType.Top);
        };

        w.ShowSync();
    }

}
