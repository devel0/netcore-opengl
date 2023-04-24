using Avalonia.Threading;

namespace example;

// example-0018
// animation, object matrix versus model matrix versus view matrix
//
// - observe the effect of camera moving and the effect of object matrix
// - use 'ctrl+shift+c' to toggle camera object

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var BOX1_ORIGIN = new Vector3(10, 20, 30);
        var BOX2_ORIGIN = new Vector3(50, 20, 30);

        Task? task = null;
        bool taskPause = false;
        var are = new AutoResetEvent(false);

        var box1 = new List<GLFigureBase>();
        var box2 = new List<GLFigureBase>();

        void ToggleAnim(AvaloniaGLControlSplit split)
        {
            if (task is not null)
            {
                taskPause = !taskPause;

                are.Set();

                return;
            }

            var glCtl = split.FocusedControl?.AvaloniaGLControl.GLControl;
            if (glCtl is null) return;

            var glModel = glCtl.GLModel;

            var bbox = glModel.LBBox;

            // the anim task acts on objectmatrix so that the figure changes on all views
            // and on initial control camera position so that only that control rotate angle of view
            task = Task.Run(async () =>
            {
                var angle = 0f;

                var angleIncrement_rad = 3 * (float)PI / 180.0f;

                while (true)
                {
                    if (taskPause)
                        are.WaitOne();

                    await Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        glCtl.ModelMatrix = Matrix4x4.Identity;

                        var mm = glCtl.ModelMatrix;

                        // compute camera [world] target starting from the bbox.middle [local]
                        var newCameraTarget = LocalToWorld(bbox.Middle, mm);

                        // compute camera [world] position
                        var newCameraPos = LocalToWorld(bbox.Middle.Fn(w => new Vector3(w.X, w.Y, 180)) +
                            1.5f * bbox.Size.Max() * Vector3.Transform(Vector3.UnitX, Matrix4x4.CreateRotationZ(angle)), mm);

                        glCtl.CameraPos = newCameraPos;
                        glCtl.CameraTarget = newCameraTarget;

                        // recompute camera [world] up
                        glCtl.CameraUp = LocalToWorld(Vector3.UnitZ, mm) - LocalToWorld(Vector3.Zero, mm);

                        split.Invalidate();

                        angle += angleIncrement_rad;

                        // set delta bounce
                        var dZ = BOX1_ORIGIN.Z * (float)Sin(2 * angle);

                        // change box object matrix
                        foreach (var fig in box1)
                        {
                            fig.ObjectMatrix =
                                Matrix4x4.CreateTranslation(-BOX1_ORIGIN)
                                *
                                Matrix4x4.CreateRotationZ(4 * angle)
                                *
                                Matrix4x4.CreateTranslation(BOX1_ORIGIN)
                                *
                                Matrix4x4.CreateTranslation(Vector3.UnitZ * dZ);
                            ;
                        }

                        await Task.Delay(30);
                    });
                }
            });
        }

        var w = GLWindow.Create(
            onFocusedControlChanged: (split, AvaloniaGLControl, isInitial) =>
            {
                if (isInitial)
                    // load view from the layout.json file copied by the csproj                    
                    split.LoadViewLayout();
            });

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var sizef = glCtl.Size();
            var sw = sizef.X;
            var sh = sizef.Y;

            IEnumerable<GLFigureBase> createBox(Vector3 boxOrigin)
            {
                var box = new Box(
                    cs: MakeCS(origin: boxOrigin, N: Vector3.UnitZ),
                    csSize: new Vector3(10, 10, 10));

                box.SideTop.SetColor(Color.Red);
                box.SideBottom.SetColor(Color.Green);
                box.SideLeft.SetColor(Color.Blue);
                box.SideRight.SetColor(Color.Magenta);
                box.SideFront.SetColor(Color.White);
                box.SideBack.SetColor(Color.Cyan);

                glModel.AddFigure(box.Sides);

                return box.Sides;
            }

            box1 = createBox(BOX1_ORIGIN).ToList();
            box2 = createBox(BOX2_ORIGIN).ToList();

            var bboxOrigin = (BOX1_ORIGIN + BOX2_ORIGIN).Fn(v => new Vector3(v.X, v.Y, -1));
            var baseBox = new Box(
                cs: MakeCS(origin: bboxOrigin, N: Vector3.UnitZ),
                csSize: new Vector3(200, 200, -2));

            glModel.AddFigure(baseBox.Sides);

            glModel.PointLights.Add(new GLPointLight(
                pos: glModel.LBBox.Middle.Fn(w => new Vector3(w.X, w.Y, 200)),
                color: Color.FromArgb(204, 204, 204),
                showPoint: true));

            glModel.SetupLightAttenuation();
            glModel.Override(ambient: .9f);

            // glCtl.CameraView(CameraViewType.Top);

            if (w.GLControlSplit is not null)
                ToggleAnim(w.GLControlSplit);
        };

        w.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Space && w.GLControlSplit is not null)
                ToggleAnim(w.GLControlSplit);
        };

        w.ShowSync();
    }

}
