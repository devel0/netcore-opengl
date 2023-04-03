namespace example;

// example-0013
// text multilines

class Program
{

    static void Main(string[] args)
    {
        const float TEXT_HEIGHT = 1f;

        InitAvalonia();

        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            glModel.AddFigure(MakeWCSFigure());

            void addText(Vector3 insPt, string text, GLTextVHAlignment alignment)
            {
                glModel.AddFigure(new GLPointFigure(new GLPoint(new GLVertex(insPt, Color.Cyan))) { PointSize = 10 });

                var figs = glModel.MakeTextFigure(new GLText(WCS.Move(insPt), text)
                {
                    Height = TEXT_HEIGHT,
                    Alignment = alignment,
                    SpacingBetweenLines = 1
                });
                glModel.AddFigure(figs);

                var figBBox = new BBox(figs.Select(fig => fig.OBBox()))
                    .MakeFigure(Color.Yellow);
                glModel.AddFigure(figBBox);
            }

            // multiline text can have newlines
            addText(
                insPt: new Vector3(10, 4, 0),
                text: "Sample text\nOn newline\n\nAbove is empty line\nend line",
                alignment: GLTextVHAlignment.TopCenter);

            glCtl.Perspective = false;
            glCtl.CameraView(CameraViewType.Top);
        };

        w.ShowSync();
    }

}
