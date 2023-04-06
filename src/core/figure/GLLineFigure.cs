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

    protected override void CopySpecialized(GLFigureBase other) =>
        base.CopySpecialized(other);

    public override GLFigureBase? Mirror(in Matrix4x4 xyPlane)
    {
        var mirroredPrimitives = new ObservableCollection<GLPrimitiveBase>();
        foreach (var primitive in PrimitivesOBC)
        {
            var q = primitive.Mirror(xyPlane);
            if (q is null) return null;

            mirroredPrimitives.Add(q);
        }

        var copy = (GLLineFigure)this.CopyBase();
        
        copy.PrimitivesOBC = mirroredPrimitives;

        return copy;
    }

}