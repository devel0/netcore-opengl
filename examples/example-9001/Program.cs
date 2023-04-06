using SearchAThing.OpenGL.Shapes;

namespace example;

// example-test-view
// show the figure used in view unit test

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        const int WIDTH = 640;
        const int HEIGHT = 480;        

        var w = GLWindow.Create(
            width: WIDTH,
            height: HEIGHT);

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var BOX_CENTER = new Vector3(50, 100, 200);
            var BOX_SIZE = new Vector3(10, 20, 5);

            var box = new Box(WCS, BOX_SIZE, csBaseIsMiddle: true);
            box.SideTop.SetColor(Color.Red);
            box.SideBottom.SetColor(Color.Green);
            box.SideLeft.SetColor(Color.Blue);
            box.SideRight.SetColor(Color.Magenta);
            box.SideFront.SetColor(Color.White);
            box.SideBack.SetColor(Color.Cyan);
            foreach (var fig in box.Sides)
            {
                fig.ObjectMatrix = Matrix4x4.CreateTranslation(BOX_CENTER);
                glModel.AddFigure(fig);
            }

            glModel.AddFigure(new GLPointFigure(new GLPoint(BOX_CENTER - BOX_SIZE * new Vector3(0, .5f, 0))
                .Act(fig =>
                {
                    fig.Order = 1;
                    fig.SetPrimitiveColor(Color.Magenta);
                }))
                { PointSize = 10 });

            //-----------

            glModel.Override(ambient: 1f);

            glCtl.Wireframe = false;
            glCtl.UseShadow = false;
            glCtl.ShowModelBBox = false;
            glCtl.CameraView(CameraViewType.FrontTop);

            // glCtl.Invalidate(InvalidateEnum.RebuildModelAndRedraw);

            glCtl.Perspective = false;
        };

        // var glCtl = new GLControl(glModel, new DummyRenderDevice(new Size(WIDTH, HEIGHT)));



        // action(glCtl);

        // basenamefile = $"V-p{(glCtl.Perspective ? 1 : 0)}-a{a}";

        // var capturePathfilename = Path.Combine(outFolderPath, $"{basenamefile}.png");

        // glCtl.CaptureImage(capturePathfilename);

        // show the gl window
        w.ShowSync();
    }

}
