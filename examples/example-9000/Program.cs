namespace sample;

// quickstart

using Vector3 = System.Numerics.Vector3;
using Color = System.Drawing.Color;
using SearchAThing.OpenGL.Core;
using static SearchAThing.OpenGL.Core.Constants;
using SearchAThing.OpenGL.GUI;
using static SearchAThing.OpenGL.GUI.Toolkit;
using SearchAThing.OpenGL.Shapes;

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
            if (!isInitial) return;

            var glModel = glCtl.GLModel;                                

            // clear the model   
            glModel.Clear();            

            // place a point light at xyz=(2,2,2)
            glModel.PointLights.Add(new GLPointLight(2, 2, 2));

            // create and add a sphere centered at (0,0,0) with radius=1 and meshed as uvsphere with N=20 divisions
            var sphere = new UVSphere(center: Vector3.Zero, radius: 1);            
            glModel.AddFigure(sphere.Figure(divisions: 20).SetColor(Color.Cyan));

            // place a base box which receive sphere shadow centered like the sphere but 2*z lower ( out of sphere )
            // with size xyz=(5, 5, .1f) larger than sphere but with small thickness
            var basebox = new Box(cs: WCS.Move(sphere.Center - Vector3.UnitZ * 2), csSize: new Vector3(5, 5, .1f));
            glModel.AddFigure(basebox.Sides);            

            glCtl.CameraView(CameraViewType.Right);
        };

        // show the gl window
        w.ShowSync();
    }
}