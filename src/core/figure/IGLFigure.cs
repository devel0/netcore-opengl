namespace SearchAThing.OpenGL.Core;

public delegate void FigureInvalidatedDelegate(IGLFigure figure);

/// <summary>
/// This is the direct child of a vertex manager.<br/>
/// A figure contains primitives and primitive contains vertexes.
/// The figure is useful to group together primitives and handle
/// properties such as Visibile, Order, ObjectMatrix, ScreenCoord mode.
/// </summary>
/// <remarks>
/// All properties are INotifyPropertyChanged managed and allow to be ui reactive.
/// </remarks>
public interface IGLFigure : IGLVertexManagerObject, INotifyPropertyChanged
{

    /// <summary>
    /// This event is managed internally by the <see cref="SearchAThing.OpenGL.Render.GLModel"/> and signal that a model figure changed some of its vertex propertties.<br/>
    /// When a model figure emit this event the model emit ViewInvalidated.
    /// </summary>        
    event FigureInvalidatedDelegate? FigureInvalidated;

    /// <summary>
    /// Invoke <see cref="FigureInvalidated"/>.<br/>
    /// Used for internal purpose by the <see cref="GLPrimitiveBase"/>.
    /// </summary>
    void Invalidate();

    /// <summary>
    /// A figure can contains primitives that in turn can contains vertexes.
    /// </summary>    
    ReadOnlyObservableCollection<GLPrimitiveBase> Primitives { get; }

    /// <summary>
    /// Enum that describe which type underlying to the instance.
    /// </summary>   
    GLPrimitiveType PrimitiveType { get; }

    /// <summary>
    /// If true figure will rendered w/out "shade with edge" shader regardless
    /// the option is active in the gl control. (Default: false)
    /// </summary>    
    bool ExcludeFromShadeWithEdge { get; }

    /// <summary>
    /// Object coord [object] to local space coord [local] transformation matrix.<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    /// <remarks>
    /// While each control has separate model/view/projection matrixes, the figure object matrix
    /// is common to all controls that uses the same model containing this figure instance.
    /// </remarks>
    Matrix4x4 ObjectMatrix { get; set; }

    /// <summary>
    /// Figure visibility.<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    /// <remarks>
    /// Changing visbility of a figure affects all controls that uses the same model containing this figure instance.
    /// </remarks>
    bool Visible { get; set; }

    /// <summary>
    /// Figure order appareance ( Default: 0 ).<br/>
    /// Higher order figure appears front to lower order figures.<br/>
    /// From the opengl point of view figures with higher order ( front ) are drawn firstly then lower order ( back ).<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    int Order { get; set; }

    /// <summary>
    /// Enable handling of figure primitive vertexes as screen coord.<br/>
    /// At vertex manager level these vertexes coord not distinguish from non screen coord vertexes types, these
    /// can share the same memory region, but when render cycles through figures at each a uniform variable inform
    /// the shader to transform within screen scoped projection/model matrixes designed to make a unscaled orthografic projection
    /// with a model translated by (-sw/2, -sh/2) where sw,sh = screen width,height.<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    bool ScreenCoordMode { get; set; }

    /// <summary>
    /// Enable highlight of a figure with a color ( Default: Yellow ).<br/>    
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    bool Highlight { get; set; }

    /// <summary>
    /// List of vertex manager vertex indexes used by this figure.<br/>
    /// Used in the final render phase where GL.DrawElements take place to 
    /// mapped as opengl ElementArrayBuffer.
    /// </summary>    
    IEnumerable<uint> Indexes { get; }

    /// <summary>
    /// Object space bbox of this figure vertexes.
    /// </summary>
    /// <param name="cs">Optional coordinate system to use in bbox detection ( Default: <see cref="WCS"/> ).</param>    
    BBox OBBox(in Matrix4x4? cs = null);

    /// <summary>
    /// States if this figure has to be considered for building of the shadow map.<br/>
    /// While is true by default in <see cref="GLFigureBase"/>, is overriden to false by <see cref="SearchAThing.OpenGL.Render.GLTextCharFigure"/> specialization of <see cref="GLTriangleFigure"/>
    /// in order to avoid dropping shadows of the triangles used by the font textures.
    /// </summary>    
    bool EvalInShadowMap { get; }

    /// <summary>
    /// User object
    /// </summary>
    object? Tag { get; set; }

}

public static partial class Constants
{

    /// <summary>
    /// (Default) <see cref="IGLFigure.ObjectMatrix"/> of a figure.
    /// </summary>
    public static readonly Matrix4x4 DEFAULT_ObjectMatrix = Matrix4x4.Identity;

    /// <summary>
    /// (Default) <see cref="IGLFigure.ScreenCoordMode"/> of a figure.
    /// </summary>
    public const bool DEFAULT_ScreenCoord = false;

    /// <summary>
    /// (Default) <see cref="IGLFigure.Visible"/> of a figure.
    /// </summary>
    public const bool DEFAULT_FigureVisible = true;

    /// <summary>
    /// (Default) <see cref="IGLFigure.Order"/> of a figure.
    /// </summary>
    public const int DEFAULT_FigureOrder = 0;

}

public static partial class Ext
{    

    /// <summary>
    /// Mean of figure vertexes.
    /// </summary>    
    public static Vector3 Center(this IGLFigure figure) =>
        figure.Vertexes().Select(w => w.Position).Mean();

}