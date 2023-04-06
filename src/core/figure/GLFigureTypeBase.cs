namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Template typed <see cref="GLPrimitiveBase"/> figure base operations such as Add and Remove primitives from the figure.
/// </summary>
public abstract class GLFigureTypeBase<P> : GLFigureBase where P : GLPrimitiveBase
{

    ReadOnlyObservableCollection<GLPrimitiveBase>? _Primitives = null;
    public override ReadOnlyObservableCollection<GLPrimitiveBase> Primitives
    {
        get
        {
            if (_Primitives is null) _Primitives = new ReadOnlyObservableCollection<GLPrimitiveBase>(PrimitivesOBC);

            return _Primitives;
        }
    }

    /// <summary>
    /// Observable collection of primitives in this figure.<br/>    
    /// </summary>            
    protected ObservableCollection<GLPrimitiveBase> PrimitivesOBC = new ObservableCollection<GLPrimitiveBase>();

    /// <summary>
    /// Add given primitive to this figure.
    /// </summary>
    /// <param name="primitive">Primitive such as <see cref="GLPoint"/>, <see cref="GLLine"/> or <see cref="GLTriangle"/> to be added.</param>
    public void Add(P primitive)
    {
        primitive.EnsureDetached();

        PrimitivesOBC.Add(primitive);        

        ParentVertexManager?.AddPrimitive(primitive, this);
    }

    /// <summary>
    /// Remove existing primitive from this figure.
    /// </summary>
    /// <param name="primitive">Primitive such as <see cref="GLPoint"/>, <see cref="GLLine"/> or <see cref="GLTriangle"/> to remove.</param>
    public void Remove(P primitive)
    {
        PrimitivesOBC.Remove(primitive);

        ParentVertexManager?.RemovePrimitive(primitive);
    }

    /// <summary>
    /// Create an empty figure.
    /// </summary>
    public GLFigureTypeBase()
    {
    }

    /// <summary>
    /// Create a figure with the set of given primitives all of the same type.
    /// </summary>    
    public GLFigureTypeBase(IEnumerable<P> primitives)
    {
        foreach (var primitive in primitives) Add(primitive);
    }

    /// <summary>
    /// Create a figure with the set of given primitives all of the same type.
    /// </summary>    
    public GLFigureTypeBase(params P[] primitives) : this(primitives.AsEnumerable()) { }

    protected override void CopySpecialized(GLFigureBase other)
    {
        var sother = (GLFigureTypeBase<P>)other;

        PrimitivesOBC = new ObservableCollection<GLPrimitiveBase>(
            sother.PrimitivesOBC.Select(w => (GLPrimitiveBase)w.Copy()));
    }

}
