namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Helper class used by <see cref="Toolkit.MakeTextFigure(GLContext, Render.GLText)"/>.
/// </summary>
class GLTextLine
{
    public int StartOff { get; private set; }

    public int EndOff { get; private set; }

    BBox? _BBox = null;
    public BBox BBox
    {
        get
        {
            if (_BBox is null)
            {
                var glcs = GLText.CS;

                if (Run.Trim().Length == 0)
                    _BBox = new BBox(glcs, new Vector3(), new Vector3(0, GLText.Height, 0));

                else
                {
                    var pts = Figs.SelectMany(w => w.Primitives.SelectMany(p => p.Vertexes.Select(t => t.Position)));

                    _BBox = new BBox(glcs, pts);
                }
            }
            return _BBox;
        }
    }

    List<GLTriangleFigure> _figs = new List<GLTriangleFigure>();
    public IReadOnlyList<GLTriangleFigure> Figs => _figs;

    public void AddFig(GLTriangleFigure fig) => _figs.Add(fig);

    public GLText GLText { get; private set; }

    public string Text => GLText.Text;
    public string Run => Text.Substring(StartOff, EndOff - StartOff + 1);

    public GLTextLine(GLText glText, int start, int end)
    {
        GLText = glText;
        StartOff = start;
        EndOff = end;
    }

    public override string ToString() => $"start:{StartOff} end:{EndOff} figs:{Figs.Count} textPortion:[{Text.Substring(StartOff, EndOff - StartOff + 1)}] bbox:{BBox}";
}
