namespace example;

// example-0004
// draw text
//
// - press 'x' to disable texture
// - press 'w' to show triangles ( 2 each char )

class Program
{

    static void Main(string[] args)
    {
        InitAvalonia();

        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;
            var glCtx = glModel.GLContext;

            glModel.Clear();

            var txt = new GLText(
                // plane xy where text resides ( origin defines the text insertion point )
                WCS,
                "Eq.1 ( α = γ * φ )")
            {
                Color = Color.Yellow
            };

            // to create text we need to use gl context that holds fonts manager
            // the resulting figure is a set of triangles textured with the font bitmap
            var fig = glCtx.MakeTextFigure(txt);

            glModel.AddFigure(fig);

            glCtl.CameraView(CameraViewType.FrontTop);
        };

        w.ShowSync();
    }

}
