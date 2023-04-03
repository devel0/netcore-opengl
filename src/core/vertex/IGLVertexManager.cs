namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Generic figure event from the vertex manager.
/// </summary>
public delegate void FigureEvent(IGLVertexManager vertexManager, GLFigureBase figure);

/// <summary>
/// Generic figures event from the vertex manager.
/// </summary>
public delegate void FiguresEvent(IGLVertexManager vertexManager, GLFigureBase[] figures);

/// <summary>
/// Delegate for the <see cref="GLVertexManager.Invalidated"/> event.
/// </summary>
public delegate void VertexManagerInvalidatedDelegate(IGLVertexManager vertexManager);

/// <summary>
/// Vertex manager primary purpose is to keep a linear sequence of distinct <see cref="GLVertexStruct"/> structs
/// that will populated by the gl vertex <see cref="GLVertex"/> objects.<br/>
/// The same <see cref="GLVertex"/> reference cannot be registered twice in vertex manager,
/// these stay connected to their figure primitives until removed from.
/// </summary>
public interface IGLVertexManager : INotifyPropertyChanged
{

    /// <summary>
    /// Actual size of <see cref="GLVertexStruct"/> vertex list.
    /// </summary>    
    int VertexCount { get; }

    /// <summary>
    /// Actual count of gl triangles belonging to added figures.
    /// </summary>    
    int TriangleCount { get; }

    /// <summary>
    /// Enumerates vertex manager registered figures.
    /// </summary>    
    IEnumerable<GLFigureBase> Figures { get; }

    /// <summary>
    /// Actual size of registered figures list.
    /// </summary>    
    int FigureCount { get; }

    /// <summary>
    /// Adds given figure to the vertex manager.<br/>
    /// All vertexes belonging to the figure's primitives will be added.
    /// Emits <see cref="FigureAdded"/> event.
    /// </summary>
    /// <param name="figure">Figure to add.</param>
    void AddFigure(GLFigureBase figure);

    /// <summary>
    /// Remove given figure from the vertex manager.<br/>
    /// All vertexes belonging to the figure's primitives will be removed.
    /// Emits <see cref="FigureRemoved"/> event.
    /// </summary>
    /// <param name="figure">Figure to remove.</param>    
    bool RemoveFigure(GLFigureBase figure);

    /// <summary>
    /// Event emitted when a figure was added.
    /// </summary>
    event FigureEvent FigureAdded;

    /// <summary>
    /// Event emitted when a figure was removed.
    /// </summary>
    event FigureEvent FigureRemoved;

    /// <summary>
    /// Event emitted when all figures cleared.<br/>
    /// The list of figures removed is reported through the event argument of <see cref="FiguresEvent"/> delegate.
    /// </summary>
    event FiguresEvent FiguresCleared;

    /// <summary>
    /// Event emitted when a vertex was added or removed or all vertexes cleared.<br/>    
    /// Its used by the <see cref="SearchAThing.OpenGL.Render.GLModel"/> to track the model changes
    /// and emit a <see cref="SearchAThing.OpenGL.Render.GLModel.ViewInvalidated"/> event.
    /// </summary>
    /// <seealso cref="Clear"/>
    event VertexManagerInvalidatedDelegate Invalidated;

    /// <summary>
    /// Used internally by the <see cref="SearchAThing.OpenGL.Render.GLModel.RenderVertexManager"/>
    /// to render the scene.<br/>    
    /// Contains the list of vertex structs directly mapped to the gpu.<br/>
    /// This array will be built each time a vertex changes from the ToArray() of the list <see cref="GLVertexManager.Vertexes"/>.
    /// To avoid performance penalty in a scenario where you need to change the position of a figure
    /// you can use <see cref="IGLFigure.ObjectMatrix"/> that doesn't affects at all vertexes because is a figure
    /// related information forwarded to the gl pipeline shader through <see cref="SearchAThing.OpenGL.Render.Constants.UNINAME_uObject"/>.
    /// </summary>        
    GLVertexStruct[] GetVboArray();

    /// <summary>
    /// BBox of registered figure vertexes [local].
    /// </summary>    
    BBox LBBox { get; }

    /// <summary>
    /// Flag to state if vertex added to this vertex manager imply expansion of the gl model (Default: true).<br/>
    /// More (custom) vertex manager can be registered into the gl model, may for temporary purpose and
    /// setting this variable to false at the construct of these custom vertex manager allow to unaffect
    /// the gl model bbox.<br/>
    /// Should be used to avoid the zoomfit to evaluate temporary vertex manager figures.<br/>
    /// <seealso cref="GLVertexManager"/>
    /// </summary>
    bool ExpandModelBBox { get; }

    /// <summary>
    /// Clear all vertexes and figures from the vertex manager, and emit
    /// <see cref="IGLVertexManager.FiguresCleared"/> and <see cref="IGLVertexManager.Invalidated"/> event.
    /// </summary>
    void Clear();    

}
