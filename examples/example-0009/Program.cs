namespace example;

// example-0009
// offscreen render

class Program
{

    static void Main(string[] args)
    {
        var glCtx = new GLContext();
        var glModel = new GLModel(glCtx);

        System.Console.WriteLine($"GL VERSION = {glCtx.GetVersion()}");

        glModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            glModel.Clear();

            var a = new GLVertex(new Vector3(-50, -50, 0), Color.Red);
            var b = new GLVertex(new Vector3(50, -50, 0), Color.Green);
            var c = new GLVertex(new Vector3(0, 50, 0), Color.Blue);

            var tri = new GLTriangle(a, b, c);
            var triFig = new GLTriangleFigure(tri);
            glModel.AddFigure(triFig);

            glCtl.CameraView(CameraViewType.Top);
        };

        var outpuFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        foreach (var size in new Size[] { new Size(1024, 768), new Size(640, 480) })
        {
            var capturePathfilename = Path.Combine(outpuFolder, $"example-0009-{size.Width}x{size.Height}.png");
            var glCtl = new GLControl(glModel, new DummyRenderDevice(size));
            glCtl.ZoomFit();
            glCtl.CaptureImage(capturePathfilename);
            Debug.WriteLine($"output file [{capturePathfilename}]");
        }
    }

}
