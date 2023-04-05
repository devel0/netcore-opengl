namespace SearchAThing.OpenGL.Shapes;

/// <summary>
/// Ellipse
/// </summary>
public class Ellipse
{

    /// <summary>
    /// Coordinate system of the ellipse center.
    /// </summary>    
    public Matrix4x4 BaseCS { get; private set; }

    /// <summary>
    /// Center of the ellipse
    /// </summary>
    public Vector3 Center { get; private set; }

    /// <summary>
    /// Radius basex of the ellipse.
    /// </summary>    
    public float RadiusX { get; private set; }

    /// <summary>
    /// Radius basey of the ellipse.
    /// </summary>    
    public float RadiusY { get; private set; }

    public Ellipse(Matrix4x4 baseCS, float radiusX, float radiusY)
    {
        BaseCS = baseCS;
        RadiusX = radiusX;
        RadiusY = radiusY;
    }

    /// <summary>
    /// Gl line figure that represents the ellipse.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure.</param>    
    public GLLineFigure Figure(int divisions = DEFAULT_CONE_DIVISIONS)
    {
        var fig = new GLLineFigure(Lines(divisions));

        return fig;
    }

    /// <summary>
    /// Gl lines that represents the ellipse.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure.</param>        
    public IEnumerable<GLLine> Lines(int divisions)
    {
        var cs = BaseCS;

        var C = cs.Origin();
        var basex = cs.BaseX();
        var basey = cs.BaseY();
        var basez = cs.BaseZ();

        var N = divisions;

        Vector3? prevP = null;

        var alpha = 0f;
        var alphaStep = (float)(2 * PI / N);

        for (int i = 0; i < N + 1; ++i)
        {
            var p =
                C +
                basex * RadiusX * (float)Cos(alpha) +
                basey * RadiusY * (float)Sin(alpha);

            if (prevP is not null)
                yield return GLLine.FromTo(prevP.Value, p);

            alpha += alphaStep;

            prevP = p;
        }
    }

}
