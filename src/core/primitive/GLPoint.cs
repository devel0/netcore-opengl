namespace SearchAThing.OpenGL.Core;

public class GLPoint : GLPrimitiveBase, IGLPoint
{

    #region Vertex

    private GLVertex _Vertex = new GLVertex();
    /// <summary>
    /// Point vertex.
    /// </summary>
    public GLVertex Vertex
    {
        get => _Vertex;
        set
        {
            value.EnsureDetached();

            var changed = value != _Vertex;
            if (changed)
            {
                if (ParentFigure is not null)
                    ParentVertexManager?.RemoveVertex(_Vertex);

                _Vertex = value;

                if (ParentFigure is not null)
                    ParentVertexManager?.AddVertex(value);

                OnPropertyChanged();
            }
        }
    }

    #endregion

    GLPoint() : base(GLPrimitiveType.Point) { }

    /// <summary>
    /// Create an opengl point by given vertex.
    /// </summary>
    /// <param name="v">Point vertex [object].</param>    
    public GLPoint(GLVertex v) : base(GLPrimitiveType.Point)
    {
        Vertex = v;
    }

    /// <summary>
    /// Create an opengl point by given coordinate and optional vertex color.
    /// </summary>
    /// <param name="coord">Point position [object].</param>
    /// <param name="color">Point color.</param>    
    public GLPoint(Vector3 coord, Color? color = null) : this(new GLVertex(coord, color)) { }

    protected override GLPrimitiveBase MakeInstance() => new GLPoint();

    protected override void CopyFromSpecialized(GLPrimitiveBase other)
    {
        var sother = (GLPoint)other;

        Vertex = (GLVertex)sother.Vertex.Copy();
    }

    public override IEnumerable<GLVertex> Vertexes
    {
        get
        {
            yield return Vertex;
        }
    }
}