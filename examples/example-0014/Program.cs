namespace example;

// example-0014
// heavy text test

class Program
{

    static void Main(string[] args)
    {
        var PHRASE = "The quick brown fox jumps over the lazy dog";
        var N = 100;
        const float TEXT_HEIGHT = 1f;

        InitAvalonia();

        var w = GLWindow.Create();

        w.GLModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glCtx = glCtl.GLContext;
            var glModel = glCtl.GLModel;

            glModel.Clear();

            glModel.AddFigure(MakeWCSFigure());

            void addText(Vector3 insPt, string text, GLTextVHAlignment alignment)
            {
                glModel.AddFigure(new GLPointFigure(new GLPoint(new GLVertex(insPt, Color.Cyan))) { PointSize = 10 });

                var figs = glCtx.MakeTextFigure(new GLText(WCS.Move(insPt), text)
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

            var sb = new StringBuilder();

            for (int i = 0; i < N; ++i)
            {
                sb.AppendLine(PHRASE);
            }

            // add multiline huge text
            addText(
                insPt: new Vector3(0, 0, 0),
                text: sb.ToString(),
                alignment: GLTextVHAlignment.TopCenter);
            
            glCtl.LoadView();
        };

        w.ShowSync();
    }

}
