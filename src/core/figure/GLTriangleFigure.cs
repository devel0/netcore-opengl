namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Specialization of typed figure within the <see cref="GLTriangle"/> primitive.
/// </summary>
public class GLTriangleFigure : GLFigureTypeBase<GLTriangle>, IGLTriangleFigure
{

    /// <summary>
    /// Default triangle compute normal function.<br/>
    /// <seealso cref="Toolkit.DefaultTriangleNormal"/>
    /// </summary>        
    public static readonly ComputeTriangleNormalDelegate DefaultComputeNormal = (tri, vertex) =>
        tri.DefaultTriangleNormal();

    #region Texture2D

    private IGLTexture2D? _Texture2D;

    public IGLTexture2D? Texture2D
    {
        get => _Texture2D;
        set
        {
            var changed = value != _Texture2D;
            if (changed)
            {
                _Texture2D = value;
                OnPropertyChanged();
                Invalidate();
            }
        }
    }

    #endregion

    #region ComputeNormal

    private ComputeTriangleNormalDelegate _ComputeNormal = DefaultComputeNormal;

    public ComputeTriangleNormalDelegate ComputeNormal
    {
        get => _ComputeNormal;
        set
        {
            var changed = value != _ComputeNormal;
            if (changed)
            {
                _ComputeNormal = value;

                RebuildNormal();

                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Execute <see cref="ComputeNormal"/> function foreach triangle of this figure updating <see cref="GLVertex.Normal"/>.
    /// </summary>
    public void RebuildNormal()
    {
        if (this.IsAttached())
        {
            foreach (var triangle in PrimitivesOBC.OfType<GLTriangle>())
            {
                foreach (var vertex in triangle.Vertexes)
                    vertex.Normal = ComputeNormal(triangle, vertex);
            }
        }
    }

    protected override GLFigureBase MakeInstance() => new GLTriangleFigure();

    protected override void CopySpecialized(GLFigureBase other)
    {
        base.CopySpecialized(other);

        var sother = (GLTriangleFigure)other;

        Texture2D = sother.Texture2D;
        ComputeNormal = sother.ComputeNormal;
    }

    public override GLFigureBase? Mirror(in Matrix4x4 xyPlane)
    {
        var mirroredPrimitives = new ObservableCollection<GLPrimitiveBase>();
        foreach (var primitive in PrimitivesOBC)
        {
            var q = primitive.Mirror(xyPlane);
            if (q is null) return null;

            mirroredPrimitives.Add(q);
        }

        var copy = (GLTriangleFigure)this.CopyBase();

        copy.PrimitivesOBC = mirroredPrimitives;

        return copy;
    }

    public override GLPrimitiveType PrimitiveType => GLPrimitiveType.Triangle;

    /// <summary>
    /// Enumeration of triangle primitives of this figure.
    /// </summary>    
    public IEnumerable<GLTriangle> Triangles => Primitives.OfType<GLTriangle>();

    /// <summary>
    /// Create a triangle typed figure with the given set of <see cref="GLTriangle"/>.
    /// </summary>    
    public GLTriangleFigure(params GLTriangle[] triangles) : base(triangles) { }

    /// <summary>
    /// Create a triangle typed figure with the given set of <see cref="GLTriangle"/>.
    /// </summary>    
    public GLTriangleFigure(IEnumerable<GLTriangle> triangles) : base(triangles) { }

}
