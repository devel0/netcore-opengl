namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Specialization of typed figure within the <see cref="GLLine"/> primitive.
/// </summary>
public class GLLineFigure : GLFigureTypeBase<GLLine>, IGLLineFigure
{
    public override GLPrimitiveType PrimitiveType => GLPrimitiveType.Line;

    /// <summary>
    /// Enumeration of the line primitives of this figure.
    /// </summary>    
    public IEnumerable<GLLine> Lines => Primitives.OfType<GLLine>();

    /// <summary>
    /// Create a line typed figure with the given set of <see cref="GLLine"/> primitives.
    /// </summary>    
    public GLLineFigure(params GLLine[] lines) : base(lines) { }

    /// <summary>
    /// Create a line typed figure with the given set of <see cref="GLLine"/> primitives.
    /// </summary>
    public GLLineFigure(IEnumerable<GLLine> lines) : base(lines) { }

    protected override GLFigureBase MakeInstance() => new GLLineFigure();

    protected override void CopyFromSpecialized(GLFigureBase other) =>
        base.CopyFromSpecialized(other);

}