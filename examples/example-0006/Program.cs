using Avalonia.Threading;
using GShark.Geometry;

using static example.Toolkit;

namespace example;

// example-0006
// draw nurb surface
//
// - ctrl+w to toggle ShadeWithEdge
// - application 'space' key to toggle normal calc type
// - n key to toggle normals visiblity
// - application 'ctrl+space' key to toggle animation through objectmatrix

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create(
            onFocusedControlChanged: (split, AvaloniaGLControl, isInitial) =>
            {
                if (isInitial)
                {
                    // load view from the layout.json file (copied by the csproj)
                    split.LoadViewLayout();
                }
            }
        );

        bool computeNurbNormals = true;

        GLTriangleFigure? triFig = null;

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var plateCenterRef = new Vector3(5, 5, -5);

            var color = Color.FromArgb(68, 97, 147);

            var nurb = NurbsSurface.FromCorners(
                new Point3(0, 0, 0),
                new Point3(10, 0, 0),
                new Point3(10, 10, 15),
                new Point3(0, 10, -4));

            var tris = nurb.NurbToGL(color);

            // build figure from nurb mesh
            triFig = new GLTriangleFigure(tris);

            // define a custom vertex normal calc
            triFig.ComputeNormal = (tri, vtx) =>
            {
                if (computeNurbNormals)
                {
                    var pos = vtx.Position;
                    var uv = nurb.ClosestParameter(new Point3(pos.X, pos.Y, pos.Z));

                    var q = nurb.EvaluateAt(uv.U, uv.V, GShark.Enumerations.EvaluateSurfaceDirection.Normal);

                    return q.ToVector3();
                }
                else
                    return GLTriangleFigure.DefaultComputeNormal(tri, vtx);
            };

            glModel.AddFigure(triFig);

            glModel.PointLights.Add(new GLPointLight(new Vector3(7, 8, 14)));
        };

        Task? task = null;
        bool taskPause = false; // var to trigger anim
        var are = new AutoResetEvent(false); // thread blocker var

        w.KeyDown += (sender, e) =>
        {
            var glCtl = w.FocusedControl;
            if (glCtl is null) return;

            var glModel = glCtl.GLModel;

            if (new KeyGesture(Key.Space, KeyModifiers.Control).Matches(e))
            {
                if (task is not null)
                {
                    taskPause = !taskPause;

                    are.Set(); // unlock are.WaitOne()

                    return;
                }

                task = Task.Run(async () =>
                {
                    var angle = 0f;

                    var angleIncrement_rad = (float)2.5d.ToRad();

                    while (true)
                    {
                        if (taskPause)
                        {
                            are.WaitOne(); // waiting till are.Set()
                        }

                        // access to UI thread through Dispatcher.UIThread 
                        // because calling from non ui thread
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            var m =
                                Matrix4x4.CreateTranslation(-glModel.LBBox.Middle)
                                *
                                Matrix4x4.CreateRotationZ(angle)
                                *
                                Matrix4x4.CreateTranslation(glModel.LBBox.Middle);

                            foreach (var fig in glModel.Figures)
                                fig.ObjectMatrix = m;

                            angle += angleIncrement_rad;

                            w.GLControlSplit?.Invalidate();
                        });

                        await Task.Delay(50);
                    }

                });
            }

            if (e.Key == Key.Space)
            {
                computeNurbNormals = !computeNurbNormals;

                if (triFig is not null)
                {
                    // recompute normal on vertexes because changed the type of calc
                    // this will invoke custom ComputeNormal defined above
                    triFig.RebuildNormal();
                }

                // note : there is no need to rebuild the model, invalidate will use updated vertex normals
                glCtl.Invalidate(InvalidateEnum.RedrawOnly);
            }
        };

        w.ShowSync();
    }

}
