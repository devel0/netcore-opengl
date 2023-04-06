using ImageMagick;

namespace test;

public class ViewTest
{

    void BuildTest(string outFolderPath, int a, Action<GLControl> action, bool perspective, out string basenamefile)
    {

        const int WIDTH = 640;
        const int HEIGHT = 480;

        var glCtx = new GLContext();
        var glModel = new GLModel(glCtx);
        var glCtl = new GLControl(glModel, new DummyRenderDevice(new Size(WIDTH, HEIGHT)));

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
        glCtl.Perspective = perspective;
        glCtl.CameraView(CameraViewType.FrontTop);

        action(glCtl);

        basenamefile = $"V-p{(glCtl.Perspective ? 1 : 0)}-a{a}";

        // if (basenamefile == "V-p0-a0")
        // ;

        var capturePathfilename = Path.Combine(outFolderPath, $"{basenamefile}.png");

        glCtl.CaptureImage(capturePathfilename);

        ;
    }

    public static IEnumerable<object[]> AllCombinations()
    {
        foreach (var persp in new[] { true, false })
        {
            int a = 0;

            // a0
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
            }), a++, persp };

            // a1
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.CameraZoom(-.5f);
            }), a++, persp };

            // a2
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.CameraZoom(.5f);
            }), a++, persp };

            // a3
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.CameraRotate((float)PI / 2, 0);
                glCtl.ZoomFit();
            }), a++, persp };

            // a4
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.CameraRotate(-(float)PI / 2, 0);
                glCtl.ZoomFit();
            }), a++, persp };

            // a5
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.CameraRotate(0, (float)PI / 2);
                glCtl.ZoomFit();
            }), a++, persp };

            // a6
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.CameraRotate(0, -(float)PI / 2);
                glCtl.ZoomFit();
            }), a++, persp };

            // a7
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.Tilt(30);
                glCtl.ZoomFit();
            }), a++, persp };

            // a8
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.Tilt(-30);
                glCtl.ZoomFit();
            }), a++, persp };

            // a9
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.TiltLeft();
                glCtl.ZoomFit();
            }), a++, persp };

            // a10
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.TiltRight();
                glCtl.ZoomFit();
            }), a++, persp };

            // a11
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.Pan(Vector2.Zero, new Vector2(50, 0));
            }), a++, persp };

            // a12
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.Pan(Vector2.Zero, new Vector2(0, 50));
            }), a++, persp };

            // a13
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.Pan(Vector2.Zero, new Vector2(50, 50));
            }), a++, persp };

            // a14
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.PanDown();
            }), a++, persp };

            // a15
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.PanUp();
            }), a++, persp };

            // a16
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.PanLeft();
            }), a++, persp };

            // a17
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.PanRight();
            }), a++, persp };

            // a18
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.CameraZoomIn();
            }), a++, persp };

            // a19
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.CameraZoomOut();
            }), a++, persp };

            // a20
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.Tilt(0f);
            }), a++, persp };

            // a21
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.SaveView();
                glCtl.Tilt(45f);
                glCtl.LoadView();
            }), a++, persp };

            // a22
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.ToggleWireframe();
                glCtl.CameraView(CameraViewType.Top);
            }), a++, persp };

            // a23
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.ToggleWireframe();
                glCtl.CameraView(CameraViewType.Top);
                glCtl.ModelRotate(20f.ToRad(), 0);
                glCtl.ZoomFit();
            }), a++, persp };

            // a24
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.ToggleWireframe();
                glCtl.CameraView(CameraViewType.Top);
                glCtl.ModelRotate(-20f.ToRad(), 0);
                glCtl.ZoomFit();
            }), a++, persp };

            // a25
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.ToggleWireframe();
                glCtl.CameraView(CameraViewType.Top);
                glCtl.ModelRotate(0, 20f.ToRad());
                glCtl.ZoomFit();
            }), a++, persp };

            // a26
            yield return new object[] { new Action<GLControl>((glCtl) =>
            {
                glCtl.Perspective = persp;
                glCtl.ToggleWireframe();
                glCtl.CameraView(CameraViewType.Top);
                glCtl.ModelRotate(0, -20f.ToRad());
                glCtl.ZoomFit();
            }), a++, persp };
        }
    }

    [Theory]
    [MemberData("AllCombinations")]
    public void TestView(Action<GLControl> act, int a, bool perspective)
    {
        //  

        var testFolder = TestProjectFolder();
        Assert.NotNull(testFolder);
        var validationFolderPathname = Path.Combine(testFolder, "data", "view-offscreen-test1");

        var GEN_VALIDATION = false; // TODO: set to false ( for a normal counterverify mode )

        Debug.WriteLine($"Generate test result");

        string outFolderPath;

        if (GEN_VALIDATION)
            outFolderPath = validationFolderPathname;

        else
            outFolderPath = AppDomain.CurrentDomain.BaseDirectory;

        BuildTest(outFolderPath, a++, act, perspective, out var basenamefile);

        var gen_filename = basenamefile + ".png";

        string genPathfilename = Path.Combine(outFolderPath, gen_filename);

        if (!GEN_VALIDATION)
        {
            string outPathfilename = Path.Combine(outFolderPath, basenamefile + "-2-gen.png");
            string diffPathfilename = Path.Combine(outFolderPath, basenamefile + "-3-diff.png");
            var validatedPathfilename = Path.Combine(validationFolderPathname, gen_filename);

            File.Copy(genPathfilename, outPathfilename, overwrite: true);
            File.Copy(validatedPathfilename, Path.Combine(outFolderPath, basenamefile + "-1-orig.png"), overwrite: true);

            using (var imageValidated = new MagickImage(validatedPathfilename))
            using (var imageToValidate = new MagickImage(outPathfilename))
            using (var imageDiff = new MagickImage())
            {
                var abs = imageValidated.Compare(imageToValidate, ErrorMetric.Absolute, imageDiff);
                var mse = imageValidated.Compare(imageToValidate, ErrorMetric.MeanSquared);
                Console.WriteLine($"{basenamefile,25}.png   abs:{abs,-20}   mse:{mse,-20}");
                imageDiff.Write(diffPathfilename);
                if (mse >= MSE_TOL)
                {
                    Console.WriteLine($"FAIL {validatedPathfilename} {outPathfilename}   mse:{mse,-20} >= {MSE_TOL}");
                }
                Assert.True(mse < MSE_TOL);
            }
        }



    }

}