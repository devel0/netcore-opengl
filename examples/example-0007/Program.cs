namespace example;

using static example.Toolkit;

// example-0007
// draw nurb tube with triangle selection on click
//
// - click on a triangle of the mesh to get highlighted over all views
// - drag splitter handler to resize view
// - press 'c' to close splitted view, 'h' to split horizontally, 'v' to split vertically

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create(

            // specializing this method allow to to split initial control
            // TIP: view layout can be saved through Control+F2 then reloaded programmtically (see example-0005)
            onFocusedControlChanged: (glSplit, avaloniaGLControl, isInitial) =>
            {
                if (!isInitial) return;

                if (glSplit.FocusedControl is GLView initialGlView)
                {
                    initialGlView.AvaloniaGLControl.GLControl.CameraView(CameraViewType.Front);
                    initialGlView.AvaloniaGLControl.GLControl.Title = "Front";

                    // split current (1) horizontally toward 2

                    //  (1) | *2

                    AvaloniaGLControl? aControl;

                    if ((aControl = glSplit.Split(GridSplitDirection.Horizontally)?.AvaloniaGLControl) is not null)
                    {
                        // the new splitted control will initialized by the same method onGLControlCreated
                        // here we change some control properties for this new view
                        var glCtl = aControl.GLControl;
                        glCtl.Wireframe = true;
                        glCtl.CameraView(CameraViewType.Top);
                        glCtl.Title = "Top";
                    }

                    // split current (2) vertically toward 3

                    //  1 | (2)
                    // ---|-----
                    //    | *3  

                    if ((aControl = glSplit.Split(GridSplitDirection.Vertically)?.AvaloniaGLControl) is not null)
                    {
                        var glCtl = aControl.GLControl;
                        glCtl.Wireframe = true;
                        glCtl.CameraView(CameraViewType.Right);
                        glCtl.Title = "Right";
                    }

                    // split current (3) horizontally toward 4

                    //  1 |  2
                    // ---|----------
                    //    | (3) | *4  


                    if ((aControl = glSplit.Split(GridSplitDirection.Horizontally)?.AvaloniaGLControl) is not null)
                    {
                        var glCtl = aControl.GLControl;
                        glCtl.Wireframe = true;
                        glCtl.CameraView(CameraViewType.FrontBottomLeft);
                        glCtl.Title = "FrontBottomLeft";
                    }

                    // back to initial control
                    glSplit.FocusedControl = initialGlView;

                    // split current (1) vertically toward 5

                    //  (1) |   2
                    // -----|-------
                    //   *5 | 3 | 4  

                    if ((aControl = glSplit.Split(GridSplitDirection.Vertically)?.AvaloniaGLControl) is not null)
                    {
                        var glCtl = aControl.GLControl;
                        glCtl.Wireframe = true;
                        glCtl.CameraView(CameraViewType.Front);
                        glCtl.Title = "Front";
                    }

                }
            }

        );

        GLTriangleFigure? fig = null;

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            fig = Example0007(SURF_DIVS: 20);
            fig.Move(500, 100, 200);
            glModel.AddFigure(fig);

            var lightPos = new Vector3(-82, -665, 1074);
            glModel.PointLights.Clear();
            glModel.PointLights.Add(new GLPointLight(lightPos, Color.White));
        };

        DateTime? lastPressTimestamp = null;

        w.PointerPressed += (a, e) =>
        {
            if (e.Handled) return;

            lastPressTimestamp = DateTime.Now;
        };

        w.PointerReleased += (a, e) =>
        {
            if (e.Handled) return;

            if (lastPressTimestamp is null) return;

            var pressDurationMs = (DateTime.Now - lastPressTimestamp.Value).TotalMilliseconds;

            // if not a single click return
            if (pressDurationMs > 250) return;

            lastPressTimestamp = null;

            var glView = w.GLControlSplit?.FocusedControl;
            if (glView is null) return;

            // retrieve current position relative to the focused control view
            var cp = e.GetCurrentPoint(glView);

            var glCtl = glView.AvaloniaGLControl.GLControl;
            var glModel = glCtl.GLModel;

            if (cp.Properties.PointerUpdateKind == Avalonia.Input.PointerUpdateKind.LeftButtonReleased)
            {
                var sp = cp.Position.ToVector2();

                var mm = glCtl.ModelMatrix;
                var vm = glCtl.ViewMatrix;
                var pm = glCtl.ProjectionMatrix;
                var camp = glCtl.CameraPos;
                var ssize = glCtl.Size();
                var vtxMgr = glCtl.GLModel.GLVertexManager;
                var perspective = glCtl.Perspective;

                //! [RayCast]
                // retrieve ray to cast [local] to test any object in the scene
                // note: when testing a figure primitive the ray need to be backwarded to object coord (see below)
                var lraycast = glCtl.RayCastLocal(screen: cp.Position.ToVector2());

                // create static list of figures, because we create new figures inside the loop
                var figures = glModel.Figures.ToList();

                foreach (var fig in figures)
                {
                    // lraycast is relative to local space coord
                    //
                    // testing figure vs lraycast could not work correctly because a figure
                    // can have an object matrix applied that translates into different local space
                    // respect those considered by the RayCastLocal where no figure concept exists thus ObjectMatrix=Identity.
                    //
                    // In order to compare intersection between lraycast and primitives of the figure
                    // we need before to transform lraycast to an object raycast suitable to match the same space
                    // where figure primitives lies.
                    Line oraycast;

                    if (fig.ObjectMatrixIsIdentity) // no transform required if fig object matrix already an identity
                        oraycast = lraycast;

                    else
                        oraycast = lraycast.Transform(fig.ObjectMatrix.Inverse());

                    foreach (var tri in fig.Primitives.OfType<GLTriangle>())
                    {
                        var q = oraycast.Intersect(tri.Plane);
                        //! [RayCast]
                        if (q is not null && tri.Contains(q.Value))
                        {
                            // change the color and order ( default:0 ) to 1 in order to bring over the triangle
                            // elsewhere edges of adjacent triangles can cover highlighted triangle edges
                            Debug.WriteLine($"hit tri [{tri}]");
                            tri.SetColor(Color.Yellow);
                            tri.Order = 1;

                            // add a further line (orange) to hitted triangle
                            var line = GLLine.FromTo(
                                (GLVertex)tri.V1.Copy(),
                                new GLVertex((tri.V2.Position + tri.V3.Position) / 2));
                            var lineFig = new GLLineFigure(line) { Order = 1 };
                            lineFig.SetColor(Color.DarkOrange);

                            // note: linefig built using [local] figure coordinates
                            // so we need to copy the same object matrix to replicate effective position
                            lineFig.ObjectMatrix = fig.ObjectMatrix;
                            vtxMgr.AddFigure(lineFig);

                            // invalidate all the layout views
                            w.GLControlSplit?.Invalidate();
                        }
                    }
                }
            }
        };

        w.ShowSync();
    }

}
