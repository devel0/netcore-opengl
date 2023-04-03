namespace example;

// example-0012
// show text alignment types with their bounding box

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

            glModel.Clear();

            glModel.AddFigure(MakeWCSFigure());

            var useCustomFont = true;
            float textHeight;
            SKFont? customFont = null;

            if (useCustomFont)
            {
                customFont = SKFontManager.Default.MatchFamily("Manjari Thin").ToFont();
                textHeight = 1f;
            }
            else
            {
                textHeight = .8f;
            }
            
            void addText(Vector3 insPt, string text, GLTextVHAlignment alignment)
            {
                // show insertion point with a point ( 10 pixel [screen] )
                glModel.AddFigure(new GLPointFigure(new GLPoint(new GLVertex(insPt, Color.Cyan))) { PointSize = 10 });

                var fig = glModel.MakeTextFigure(new GLText(WCS.Move(insPt), text)
                {
                    Height = textHeight,
                    Alignment = alignment
                }.Act(txt => { if (customFont is not null) txt.Font = customFont; }));

                glModel.AddFigure(fig);

                // show text bbox with a yellow rectangle
                var figBBox = new BBox(fig.Select(w => w.OBBox()))
                    .MakeFigure(Color.Yellow);
                glModel.AddFigure(figBBox);
            }

            addText(new Vector3(0, 4, 0), "TopLeft", GLTextVHAlignment.TopLeft);
            addText(new Vector3(0, 2, 0), "MiddleLeft", GLTextVHAlignment.MiddleLeft);
            addText(new Vector3(0, 0, 0), "BottomLeft", GLTextVHAlignment.BottomLeft);

            addText(new Vector3(10, 4, 0), "TopCenter", GLTextVHAlignment.TopCenter);
            addText(new Vector3(10, 2, 0), "MiddleCenter", GLTextVHAlignment.MiddleCenter);
            addText(new Vector3(10, 0, 0), "BottomCenter", GLTextVHAlignment.BottomCenter);

            addText(new Vector3(20, 4, 0), "TopRight", GLTextVHAlignment.TopRight);
            addText(new Vector3(20, 2, 0), "MiddleRight", GLTextVHAlignment.MiddleRight);
            addText(new Vector3(20, 0, 0), "BottomRight", GLTextVHAlignment.BottomRight);

            glCtl.Perspective = false;
            glCtl.CameraView(CameraViewType.Top);
        };

        w.ShowSync();
    }

}
