namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Enum that specify which type of primitive belong to a figure.
/// </summary>
public enum GLPrimitiveType
{

    /// <summary>
    /// Corresponds to GL_POINTS opengl primitive.<br/>
    /// Its described by 1 GLVertex.<br/>
    /// A point primitive can have a screen pixel size.
    /// </summary>
    Point,

    /// <summary>
    /// Corresponds to GL_LINES opengl primitive.<br/> 
    /// Its described by 2 GLVertex.<br/>
    /// A line primitive will rasterized with 1 pixel screen size.
    /// </summary>
    Line,

    /// <summary>
    /// Corresponds to GL_TRIANGLES opengl primitive.<br/>
    /// Its described by 3 GLVertex.<br/>
    /// A triangle primitive can be rendered in wireframe or shade mode.<br/>
    /// The option ShadeWidthEdge allow to overlay triangles edges with lines
    /// generated at runtime through geometry shader.<br/>
    /// </summary>
    Triangle
}

/// <summary>
/// A primitive such a <see cref="GLPoint"/>, <see cref="GLLine"/> or <see cref="GLTriangle"/> can be
/// added to a figure through its add method.<br/>
/// A primitive can consists of 1, 2 or 3 <see cref="GLVertex"/> that will be registered into
/// <see cref="GLVertexManager"/> when the primitive added to the figure and figure to the model
/// through <see cref="GLModel.AddFigure"/>.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public interface IGLPrimitive : IGLVertexManagerObject, INotifyPropertyChanged
{

    /// <summary>
    /// Reference to the parent figure.
    /// </summary>
    IGLFigure? ParentFigure { get; }

    /// <summary>
    /// Enumerate primitive gl vertexes.
    /// </summary>    
    IEnumerable<GLVertex> Vertexes { get; }

    /// <summary>
    /// Enum that describe which type underlying to the instance.
    /// </summary>   
    GLPrimitiveType PrimitiveType { get; }

    /// <summary>
    /// Primitive order appareance ( Default: 0 ).<br/>
    /// Higher order primitives appears front to lower order primitives of the same figure.<br/>
    /// From the opengl point of view figures with higher order ( front ) are drawn firstly then lower order ( back ).<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    [JsonProperty]
    int Order { get; set; }

    /// <summary>
    /// (cached) Bounding box of the primitive [local].
    /// </summary>    
    BBox LBBox { get; }

    /// <summary>
    /// Retrieve e simple cmd representation of this primtiive.<br/>
    /// Its a textual representation of primitive information useful to regen in a separate tool.
    /// </summary>    
    /// <param name="includeHeader">If true header will prepended to form a complete command.</param>    
    string SimpleCmd(bool includeHeader = true);

    /// <summary>
    /// States if primitive is actually selected.<br/>
    /// It true the primitive is listed in model selected primitives <see cref="GLModel.SelectedPrimitives"/>.
    /// </summary>    
    bool Selected { get; }

}
