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

    public ComputeTriangleNormalDelegate ComputeNormal { get; private set; } = DefaultComputeNormal;

    public bool ComputeNormalMean { get; private set; }

    public void SetupComputeNormal(bool mean = false, ComputeTriangleNormalDelegate? computeNormal = null)
    {
        ComputeNormalMean = mean;

        if (computeNormal is null)
            ComputeNormal = DefaultComputeNormal;

        else
            ComputeNormal = computeNormal;

                RebuildNormal();

        OnPropertyChanged(nameof(ComputeNormal));
        OnPropertyChanged(nameof(ComputeNormalMean));
    }

    #endregion


    /// <summary>
    /// Execute <see cref="ComputeNormal"/> function foreach triangle of this figure updating <see cref="GLVertex.Normal"/>.<br/>
    /// Optionally mean these values if <see cref="ComputeNormalMean"/> was set.
    /// </summary>
    /// <param name="onlyMean">If true, vertex normal will not recomputed, only mean on their values will be set.</param>
    public void RebuildNormal(bool onlyMean = false)
    {
        if (this.IsAttached())
        {
            var tol = ParentVertexManager!.LBBox.TolHint;

            foreach (var triangle in PrimitivesOBC.OfType<GLTriangle>())
            {
                foreach (var vertex in triangle.Vertexes)
                {
                    vertex.Normal = ComputeNormal(triangle, vertex);
                }
            }

            if (ComputeNormalMean)
            {
                this.Vertexes()
                    .GroupBy(vtx => vtx.PositionSignature(tol))
                    .ToList()
                    .ForEach(grp =>
                    {
                        int cnt = 0;
                        var n = Vector3.Zero;
                        foreach (var vtx in grp)
                        {
                            n += vtx.Normal;
                            ++cnt;
                        }
                        n /= cnt;
                        foreach (var vtx in grp)
                            vtx.Normal = n;
                    });
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

    /// <summary>
    /// Retrieve simple cmd for a list of triangles.<br/>
    /// Coordinates of v1,v2,v3 are separated with comma so there are 9 float separated by comma.<br/>
    /// Further triangles are appended by semi-colon separator.
    /// [t ]ax1,ay1,az1,ax2,ay2,az2,ax3,ay3,az3;bx1,by1,bz1,bx2,by2,bz2,bx3,by3,bz3;...
    /// </summary>   
    public override string SimpleCmd() => $"{SIMPLE_CMD_TRIANGLE} " + string.Join(";", Primitives.Select(w => w.SimpleCmd(false)));

}
