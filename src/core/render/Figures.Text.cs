namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    /// <summary>
    /// Generate a triangle figure textured to represents the given <see cref="GLText"/>
    /// </summary>
    /// <param name="glModel">Gl model ( used to access <see cref="GLFontCharManager"/> ).</param>
    /// <param name="glText">Gl text.</param>
    /// <returns>Gl triangle figure.</returns>
    public static List<GLTriangleFigure> MakeTextFigure(this GLModel glModel, GLText glText) =>
        glModel.GLContext.MakeTextFigure(glText);

    /// <summary>
    /// Generate a triangle figure textured to represents the given <see cref="GLText"/><br/>    
    /// </summary>
    /// <param name="glContext">Gl context ( used to access <see cref="GLFontCharManager"/> ).</param>
    /// <param name="glText">Gl text.</param>
    /// <returns>Gl triangle figure.</returns>    
    public static List<GLTriangleFigure> MakeTextFigure(this GLContext glContext, GLText glText)
    {
        var res = new List<GLTriangleFigure>();

        var fontChars = glContext.FontCharMgr.GetFontChars(glText.Text, glText.Font);

        var lines = new List<GLTextLine>();

        {
            var ss = glText.Text.Split('\n');
            var off = 0;
            foreach (var str in ss)
            {
                lines.Add(new GLTextLine(glText, off, off + str.Length - 1));
                off += str.Length + 1;
            }
        }

        var cs = glText.CS;
        var baseX = cs.BaseX();
        var baseY = cs.BaseY();
        var pos = Vector3.Zero;
        var font = glText.Font;

        var metrics = font.Metrics;

        // each line are draw as BottomLeft align to cs origin, then are moved using ObjectMatrix
        foreach (var line in lines)
        {
            var gf = glText.Height / FONT_BITMAP_PIXEL;

            var xOff = 0f;

            GLFontChar? prevFc = null;

            for (int gi = line.StartOff; gi <= line.EndOff; ++gi)
            {
                var glTextCharFigure = new GLTextCharFigure(glText, gi);

                var fc = fontChars[gi];

                //            XXXXX------------------^
                //            XXXXX                  |
                //            XXXXX                  |
                //            XXXXX                  |
                //     |MidY..XXXXX       H/2........|Height
                //     |      XXXXX                  |
                //     +------XXXXX-------           |
                //     |      XXXXX      |Bottom     |
                //     |      XXXXX    --v-----------v
                //     |
                //   Y v
                //

                // [fc.TextBounds]: Bottom = Height/2 + MidY
                //                  MidY = Bottom - Height/2

                var yOff = (-fc.TextBounds.Bottom) * gf;

                var px = fc.TextBounds.Left;
                if (prevFc is not null)
                    px += (prevFc.Measure - prevFc.TextBounds.Left);

                xOff += px * gf;

                if (fc.Bitmap.Width > 0 && fc.Bitmap.Height > 0)
                {
                    var p = baseX * xOff + baseY * yOff;

                    #region triangles

                    var w = fc.TextBounds.Width * gf;
                    var h = fc.TextBounds.Height * gf;

                    var v1 = p;
                    var v2 = v1 + cs.BaseX() * w;
                    var v3 = v2 + cs.BaseY() * h;
                    var v4 = v1 + cs.BaseY() * h;

                    /*
                        v4     v3
                        +------+
                        |      |
                        |      |
                        |      |
                        +------+
                        v1     v2

                    */

                    if (fc.Bitmap.Width > 0 && fc.Bitmap.Height > 0)
                    {
                        /*
                                0,0    1,0
                                v4     v3
                                +------+
                                |     /| ^
                                |   /xx| |
                                | /xxxx|
                                +------+
                                v1  -> v2
                                0,1    1,1
                            */

                        var tri1_v1 = new GLVertex(v1, textureST: new Vector2(0, 1));
                        var tri1_v2 = new GLVertex(v2, textureST: new Vector2(1, 1));
                        var tri1_v3 = new GLVertex(v3, textureST: new Vector2(1, 0));

                        var t1_tri = new GLTriangle(tri1_v1, tri1_v2, tri1_v3);

                        glTextCharFigure.Add(t1_tri);

                        /*
                              0,0    1,0
                              v4  <- v3
                              +------+
                              |xxxxx/|
                            | |xxx/  |
                            v |x/    |
                              +------+
                              v1     v2
                              0,1    1,1
                        */

                        var tri2_v1 = new GLVertex(v3, textureST: new Vector2(1, 0));
                        var tri2_v2 = new GLVertex(v4, textureST: new Vector2(0, 0));
                        var tri2_v3 = new GLVertex(v1, textureST: new Vector2(0, 1));

                        var t2_tri = new GLTriangle(tri2_v1, tri2_v2, tri2_v3);

                        glTextCharFigure.Add(t2_tri);

                        glTextCharFigure.Texture2D = fc.Texture;

                        line.AddFig(glTextCharFigure);
                    }
                    #endregion
                }

                prevFc = fc;
            }
            // }
        }

        var move = Vector3.Zero;
        GLTextLine? prev = null;

        var xaxis = cs.BaseX();
        var yaxis = cs.BaseY();

        var prevLinesVSize = 0f;

        var allLinesSizeY = lines.Select(w => w.BBox.SizeCS.Y).Sum();

        foreach (var line in lines)
        {
            if (prev is not null)
                prevLinesVSize += prev.BBox.SizeCS.Y * glText.SpacingBetweenLines;

            switch (glText.Alignment)
            {
                case GLTextVHAlignment.TopLeft:
                    move = cs.Origin() - line.BBox.Min - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize);
                    break;

                case GLTextVHAlignment.TopCenter:
                    move = cs.Origin() - line.BBox.Min - xaxis * line.BBox.SizeCS.X / 2 - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize);
                    break;

                case GLTextVHAlignment.TopRight:
                    move = cs.Origin() - line.BBox.Min - xaxis * line.BBox.SizeCS.X - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize);
                    break;

                // //

                case GLTextVHAlignment.MiddleLeft:
                    move = cs.Origin() - line.BBox.Min - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize - allLinesSizeY / 2);
                    break;

                case GLTextVHAlignment.MiddleCenter:
                    move = cs.Origin() - line.BBox.Min - xaxis * line.BBox.SizeCS.X / 2 - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize - allLinesSizeY / 2);
                    break;

                case GLTextVHAlignment.MiddleRight:
                    move = cs.Origin() - line.BBox.Min - xaxis * line.BBox.SizeCS.X - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize - allLinesSizeY / 2);
                    break;

                // //

                case GLTextVHAlignment.BottomLeft:
                    move = cs.Origin() - line.BBox.Min + yaxis * (allLinesSizeY - line.BBox.SizeCS.Y - prevLinesVSize);
                    break;

                case GLTextVHAlignment.BottomCenter:
                    move = cs.Origin() - line.BBox.Min - xaxis * line.BBox.SizeCS.X / 2 - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize - allLinesSizeY);
                    break;

                case GLTextVHAlignment.BottomRight:
                    move = cs.Origin() - line.BBox.Min - xaxis * line.BBox.SizeCS.X - yaxis * (line.BBox.SizeCS.Y + prevLinesVSize - allLinesSizeY);
                    break;
            }

            if (!move.Equals(Vector3.Zero))
            {
                foreach (var fig in line.Figs)
                {
                    fig.ObjectMatrix = Matrix4x4.CreateTranslation(move);
                }
            }

            res.AddRange(line.Figs);

            prev = line;
        }

        return res;
    }

}