namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Triangles figure with texture mapped to represent a char.
/// </summary>
public class GLTextCharFigure : GLTriangleFigure
{

    #region GLText

    private GLText _GLText = new GLText(Matrix4x4.Identity, "");

    /// <summary>
    /// Gl text object.
    /// </summary>
    public GLText GLText
    {
        get => _GLText;
        set
        {
            var changed = value != _GLText;
            if (changed)
            {
                _GLText = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region TextOff

    private int _TextOff = 0;

    /// <summary>
    /// Offset in <see cref="GLText.Text"/> for the char here representing.
    /// </summary>
    public int TextOff
    {
        get => _TextOff;
        private set
        {
            var changed = value != _TextOff;
            if (changed)
            {
                _TextOff = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Don't drop shadow.
    /// </summary>
    public override bool EvalInShadowMap => false;

    GLTextCharFigure() { }

    /// <summary>
    /// Create gl text char figure.
    /// </summary>
    /// <param name="glText">Gl text object.</param>
    /// <param name="textOff">Offset in gl text objec text string.</param>
    public GLTextCharFigure(GLText glText, int textOff)
    {
        _GLText = glText;
        _TextOff = textOff;
    }

    protected override GLFigureTypeBase<GLTriangle> MakeInstance() => new GLTextCharFigure();

    protected override void CopyFromSpecialized(GLFigureBase other)
    {
        base.CopyFromSpecialized(other);

        var sother = (GLTextCharFigure)other;

        GLText = new GLText(
            sother.GLText.CS,
            sother.GLText.Text,
            sother.GLText.Height,
            sother.GLText.Color,
            sother.GLText.Alignment);

        TextOff = sother.TextOff;
    }

    public override string ToString() => $"Char [{GLText.Text[TextOff]}]";

}