namespace SearchAThing.OpenGL.Core;

public abstract class GLPrimitiveBase : IGLPrimitive
{

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    IGLFigure? _ParentFigure = null;
    public IGLFigure? ParentFigure
    {
        get
        {
            return _ParentFigure;
        }
        internal set
        {
            _ParentFigure = value;
        }
    }

    public GLVertexManager? ParentVertexManager => ParentFigure?.ParentVertexManager;

    public abstract IEnumerable<GLVertex> Vertexes { get; }

    public GLPrimitiveType PrimitiveType { get; private set; }

    internal GLPrimitiveBase(GLPrimitiveType primitiveType)
    {
        PrimitiveType = primitiveType;
    }

    protected abstract GLPrimitiveBase MakeInstance();

    public IGLVertexManagerObject Copy()
    {
        var res = MakeInstance();

        res.Order = Order;

        res.CopyFromSpecialized(this);

        return res;
    }

    protected abstract void CopyFromSpecialized(GLPrimitiveBase other);

    #region Order

    private int _Order = 0;

    public int Order
    {
        get => _Order;
        set
        {
            var changed = value != _Order;
            if (changed)
            {
                _Order = value;
                OnPropertyChanged();
                ParentFigure?.Invalidate();
            }
        }
    }

    #endregion

    public BBox BBox(Matrix4x4? cs = null)
    {
        var res = new BBox(cs);

        foreach (var vertex in Vertexes)
        {
            res.ApplyUnion(vertex.Position);
        }

        return res;
    }

    /// <summary>
    /// Set the color of primitive vertexes.
    /// </summary>    
    /// <param name="color">Color to set on primitive vertexes.</param>
    /// <returns>This primitive.</returns>
    public GLPrimitiveBase SetColor(Color color) => SetColor(color.ToVector4());

    /// <summary>
    /// Set the color of primitive vertexes.
    /// </summary>    
    /// <param name="rgbaColor">Color to set on primitive vertexes.</param>
    /// <returns>This primitive.</returns>
    public GLPrimitiveBase SetColor(Vector4 rgbaColor)
    {
        foreach (var vertex in Vertexes)
            vertex.MaterialColor = rgbaColor;

        return this;
    }

}

public static partial class Ext
{
   
    /// <summary>
    /// Set the color primitives vertexes.
    /// </summary>
    /// <param name="primitives">Gl primitives.</param>
    /// <param name="color">Color to set on primitives vertexes.</param>
    /// <returns>Calling enumerable.</returns>
    public static IEnumerable<T> SetColor<T>(this IEnumerable<T> primitives, Color color)
    where T : GLPrimitiveBase => primitives.SetColor(color.ToVector4());

    /// <summary>
    /// Set the color primitives vertexes.
    /// </summary>
    /// <param name="primitives">Gl primitives.</param>
    /// <param name="rgbaColor">Color to set on primitives vertexes.</param>
    /// <returns>Calling enumerable.</returns>
    public static IEnumerable<T> SetColor<T>(this IEnumerable<T> primitives, Vector4 rgbaColor)
    where T : GLPrimitiveBase
    {
        foreach (var primitive in primitives)
            foreach (var vertex in primitive.Vertexes)
                vertex.MaterialColor = rgbaColor;

        return primitives;
    }

}
