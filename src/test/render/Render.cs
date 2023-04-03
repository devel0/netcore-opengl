using ImageMagick;
using static example.Toolkit;

namespace test;

public class RenderTest
{

    void BuildTest(TestConfig config, string capturePathfilename)
    {
        const int WIDTH = 640;
        const int HEIGHT = 480;

        using (var glCtx = new GLContext())
        {
            var glModel = new GLModel(glCtx);
            var glCtl = new GLControl(glModel, new DummyRenderDevice(new Size(WIDTH, HEIGHT)));

            example.Toolkit.RenderUnitTest_SetupBuildModel(glModel, (glCtl) =>
            {
                glCtl.Wireframe = config.Wireframe;
                glCtl.ShadeWithEdge = config.ShadeWithEdge;
                glCtl.Perspective = config.Perspective;
                glCtl.UseShadow = config.UseShadow;
                glCtl.ShowNormals = config.ShowNormals;
                glCtl.ShowModelBBox = true;
                glCtl.CameraView(config.ViewType);

            });

            glCtl.CaptureImage(capturePathfilename);
        }
    }

    public static IEnumerable<object[]> AllCombinations()
    {
        foreach (var viewType in new[] { CameraViewType.Front, CameraViewType.FrontRight })
        {
            foreach (var perspective in new[] { true, false })
            {
                foreach (var shadow in new[] { true, false })
                {
                    foreach (var wireframe in new[] { true, false })
                    {
                        foreach (var shadewithedge in new[] { true, false })
                        {
                            foreach (var normal in new[] { true, false })
                            {
                                if (wireframe && shadewithedge) continue;
                                if (!wireframe && normal) continue;

                                var config = new TestConfig();
                                config.ViewType = viewType;
                                config.Wireframe = wireframe;
                                config.ShadeWithEdge = shadewithedge;
                                config.Perspective = perspective;
                                config.UseShadow = shadow;
                                config.ShowModelBBox = true;
                                config.ShowNormals = normal;

                                yield return new[] { config };
                            }
                        }
                    }
                }
            }
        }
    }

    [Theory]
    [MemberData("AllCombinations")]
    public void TestRender(TestConfig config)
    {
        var testFolder = TestProjectFolder();
        Assert.NotNull(testFolder);
        var validationFolderPathname = Path.Combine(testFolder, "data", "render-offscreen-test1");

        var GEN_VALIDATION = false; // TODO: set to false ( for a normal counterverify mode )

        Debug.WriteLine($"Generate test result : {config.ToString()}");

        string outFolderPath;

        if (GEN_VALIDATION)
            outFolderPath = validationFolderPathname;

        else
            outFolderPath = AppDomain.CurrentDomain.BaseDirectory;

        var gen_filename = config.ToString() + ".png";

        string outPathfilename = Path.Combine(outFolderPath, gen_filename);
        string genPathfilename = Path.Combine(outFolderPath, config.ToString() + "-2-gen.png");
        string diffPathfilename = Path.Combine(outFolderPath, config.ToString() + "-3-diff.png");        
        
        BuildTest(config, outPathfilename);

        if (!GEN_VALIDATION)
        {            
            var validatedPathfilename = Path.Combine(validationFolderPathname, gen_filename);
            File.Copy(validatedPathfilename, Path.Combine(outFolderPath, config.ToString() + "-1-orig.png"), overwrite: true);

            using (var imageValidated = new MagickImage(validatedPathfilename))
            using (var imageToValidate = new MagickImage(outPathfilename))
            using (var imageDiff = new MagickImage())
            {
                var abs = imageValidated.Compare(imageToValidate, ErrorMetric.Absolute, imageDiff);
                var mse = imageValidated.Compare(imageToValidate, ErrorMetric.MeanSquared);
                Console.WriteLine($"{config.ToString(),25}.png   abs:{abs,-20}   mse:{mse,-20}");
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