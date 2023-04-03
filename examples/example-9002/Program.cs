namespace example;

// example-test-render
// show the figure used in render unit test

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create(
            onGLControlCreated: (avaloniaGLControl) =>
            {
                var glCtl = avaloniaGLControl.GLControl;
                
                glCtl.LoadView();
            });

        example.Toolkit.RenderUnitTest_SetupBuildModel(w.GLModel);
        
        // show the gl window
        w.ShowSync();
    }

}
