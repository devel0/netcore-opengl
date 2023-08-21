// example-0027
// earth sphere ( console program )
//
// - rotate earth sphere

InitAvalonia();

var w = GLWindow.Create(
    onFocusedControlChanged: (split, AvaloniaGLControl, isInitial) =>
    {
        if (isInitial)
        {
            split.LoadViewLayout();
        }
    }
);

static string texturePathfilename(string name) => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "texture", name);

w.GLModel.BuildModel = (glCtl, isInitial) =>
{
    if (!isInitial) return;

    var glModel = glCtl.GLModel;
    var glCtx = glCtl.GLContext;

    glModel.Clear();

    // lights loaded from layout.json ( saved using SHIFT+F2 )
    // note: ambient is set to Black in order to make dark side effective

    var earthTexture = new GLTexture2D(glCtx, SKBitmap.Decode(texturePathfilename("earth.jpg")));
    
    var sphere = new UVSphere(center: Vector3.Zero, radius: 1);
    // use of setUVTexture to map uv sphere texture coords
    var fig = sphere.Figure(divisions: 20, setUVTexture: true);
    fig.Texture2D = earthTexture;

    glModel.AddFigure(fig);
};

w.ShowSync();