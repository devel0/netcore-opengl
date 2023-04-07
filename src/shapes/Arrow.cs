namespace SearchAThing.OpenGL.Shapes;

/// <summary>
/// Arrow shape.
/// </summary>
public class Arrow
{

    // TODO: expose tip factors

    /// <summary>
    /// Arrow from position.
    /// </summary>
    public Vector3 From { get; private set; }

    /// <summary>
    /// Arrow to position.
    /// </summary>    
    public Vector3 To { get; private set; }

    /// <summary>
    /// Arrow rod diameter.
    /// </summary>    
    public float Diameter { get; private set; }

    /// <summary>
    /// Computed tip diameter.    
    /// </summary>
    /// <seealso cref="DEFAULT_ARROW_TIP_DIAMETER_FACTOR"/>.
    public float TipDiameter => Diameter * DEFAULT_ARROW_TIP_DIAMETER_FACTOR;

    /// <summary>
    /// Computed arrow length as (To - From).Length().
    /// </summary>    
    public float Length => (To - From).Length();

    /// <summary>
    /// Computed tip length.
    /// </summary>
    /// <seealso cref="DEFAULT_ARROW_TIP_LEN_FACTOR"/>.
    public float TipLength => Length * DEFAULT_ARROW_TIP_LEN_FACTOR;

    /// <summary>
    /// Create an arrow from, to with given optional diameter.
    /// </summary>
    /// <param name="from">Arrow from position.</param>
    /// <param name="to">Arrow to position.</param>
    /// <param name="diameterFactor">(Optional) Arrow rod diameter factor to compute rod diameter as length * factor ( default:0.1 ).</param>
    public Arrow(in Vector3 from, in Vector3 to, float? diameterFactor = null)
    {
        From = from;
        To = to;
        if (diameterFactor is null) diameterFactor = .1f;
        Diameter = (to - from).Length() * diameterFactor.Value;
    }

    /// <summary>
    /// Gl triangles figure that represents the arrow.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure. (Min:4)</param>    
    /// <param name="lineMode">If true a line with 3 lines instead of triangles will created (Default:false).</param>    
    public GLFigureBase Figure(int divisions = DEFAULT_ARROW_DIVISIONS, bool lineMode = false)
    {
        if (lineMode)
            return new GLLineFigure(Lines());
        else
            return new GLTriangleFigure(Triangles(divisions));
    }

    /// <summary>
    /// GL line figure that presents the arrow.
    /// </summary>
    public IEnumerable<GLLine> Lines()
    {
        var cs = MakeCS(From, Vector3.Normalize(To - From));

        var r = Diameter / 2;
        var l = Length;
        var tl = TipLength;
        var tr = TipDiameter / 2;
        var rl = l - tl;

        var rod = GLLine.FromTo(From, To);
        yield return rod;

        yield return GLLine.FromTo(To, From + cs.BaseZ() * rl + cs.BaseX() * tr);
        yield return GLLine.FromTo(To, From + cs.BaseZ() * rl - cs.BaseX() * tr);

        yield return GLLine.FromTo(To, From + cs.BaseZ() * rl + cs.BaseY() * tr);
        yield return GLLine.FromTo(To, From + cs.BaseZ() * rl - cs.BaseY() * tr);
    }

    /// <summary>
    /// Gl triangle figure that represents the arrow.
    /// </summary>
    /// <param name="divisions">Number of divisions to refine the figure. (Min:4)</param>    
    public IEnumerable<GLTriangle> Triangles(int divisions = DEFAULT_ARROW_DIVISIONS)
    {
        var cs = MakeCS(From, Vector3.Normalize(To - From));

        var r = Diameter / 2;
        var l = Length;
        var tl = TipLength;
        var tr = TipDiameter / 2;
        var rl = l - tl;

        var rod = new Cone(cs, r, r, rl);
        foreach (var tri in rod.Triangles(divisions)) yield return tri;

        var tip = new Cone(cs.Move(cs.BaseZ() * rl), tr, 0, tl);
        foreach (var tri in tip.Triangles(divisions)) yield return tri;
    }

}