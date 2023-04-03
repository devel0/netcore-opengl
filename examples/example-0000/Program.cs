//! [example-0000]
namespace example;

// example-0000
// draw a triangle with 3 colors (one for each vertex)
//
// use gesture such as:
// - 'w' to toggle wireframe
// - ctrl right/left to change tilt
// - mouse wheel to zoom
// - 'z' to zoomfit
// - ctrl + x to show bbox

class Program
{

    static void Main(string[] args)
    {
        // this must called for console application to enable Avalonia framework
        // and must called before any other Avalonia control usage		
        InitAvalonia();

        // create standalone Avalonia window for Silk.NET opengl rendering
        var w = GLWindow.Create();

        // define the GLModel build function        
        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            // avoid to regen model if a full invalidate happens
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            // clear the model                
            glModel.Clear();

            var a = new Vector3(-50, -50, 0);
            var b = new Vector3(50, -50, 0);
            var c = new Vector3(0, 50, 0);

            var va = new GLVertex(a, Color.Red);
            var vb = new GLVertex(b, Color.Green);
            var vc = new GLVertex(c, Color.Blue);

            var tri = new GLTriangle(va, vb, vc);

            // add triangle to the model
            glModel.AddFigure(new GLTriangleFigure(tri));

            glCtl.CameraView(CameraViewType.Top);
        };

        // show the gl window
        w.ShowSync();
    }

}

//! [example-0000]