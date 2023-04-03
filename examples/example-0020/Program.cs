namespace example;

// example-0020
// customize gesture
//
// - try hitting application key '1' to toggle wireframe
// - try 'w' key that is unbound now

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create(
            onGLControlCreated: (avaloniaGLControl) =>
            {
                // to remove all predefined key gestures:
                // avaloniaGLControl.ClearGestures();

                // to replace single gesture ( replace 'w' with '1' )
                avaloniaGLControl.ToggleWireframeGesture = new KeyGesture(Key.D1);

                var glCtl = avaloniaGLControl.GLControl;
                glCtl.CameraView(CameraViewType.Top);
            });

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glCtx = glCtl.GLContext;
            var glModel = glCtl.GLModel;

            glModel.Clear();

            var a = new Vector3(-50, -50, 0);
            var b = new Vector3(50, -50, 0);
            var c = new Vector3(0, 50, 0);

            var va = new GLVertex(a, Color.Red);
            var vb = new GLVertex(b, Color.Green);
            var vc = new GLVertex(c, Color.Blue);

            var tri = new GLTriangle(va, vb, vc);

            glModel.AddFigure(new GLTriangleFigure(tri));
        };

        w.ShowSync();
    }

}
