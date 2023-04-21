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
[JsonObject(MemberSerialization.OptIn)]
public interface IGLFigure : IGLVertexManagerObject, INotifyPropertyChanged
{

    /// <summary>
    /// This event is managed internally by the <see cref="SearchAThing.OpenGL.Core.GLModel"/> and signal that a model figure changed some of its vertex propertties.<br/>
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
    [JsonProperty]
    Matrix4x4 ObjectMatrix { get; set; }

    /// <summary>
    /// Figure visibility.<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    /// <remarks>
    /// Changing visbility of a figure affects all controls that uses the same model containing this figure instance.
    /// </remarks>
    [JsonProperty]
    bool Visible { get; set; }

    /// <summary>
    /// Figure order appareance ( Default: 0 ).<br/>
    /// Higher order figure appears front to lower order figures.<br/>
    /// From the opengl point of view figures with higher order ( front ) are drawn firstly then lower order ( back ).<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    [JsonProperty]
    int Order { get; set; }

    /// <summary>
    /// Enable handling of figure primitive vertexes as screen coord.<br/>
    /// At vertex manager level these vertexes coord not distinguish from non screen coord vertexes types, these
    /// can share the same memory region, but when render cycles through figures at each a uniform variable inform
    /// the shader to transform within screen scoped projection/model matrixes designed to make a unscaled orthografic projection
    /// with a model translated by (-sw/2, -sh/2) where sw,sh = screen width,height.<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>    
    [JsonProperty]
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
    /// (cached) Bounding box of this figure vertexes [local].
    /// </summary>    
    BBox LBBox { get; }

    /// <summary>
    /// Bounding bbox of this figure vertexes [object].
    /// </summary>    
    BBox OBBox { get; }

    /// <summary>
    /// States if this figure is considered in bbox expansion (Default: true).<br/>
    /// Useful to avoid cs figure or non related to content of interest to decrease view and zoom experience.
    /// </summary>    
    bool ExpandBBox { get; set; }

    /// <summary>
    /// States if this figure has to be considered for building of the shadow map.<br/>
    /// While is true by default in <see cref="GLFigureBase"/>, is overriden to false by <see cref="GLTextCharFigure"/> specialization of <see cref="GLTriangleFigure"/>
    /// in order to avoid dropping shadows of the triangles used by the font textures.
    /// </summary>    
    bool EvalInShadowMap { get; }

    /// <summary>
    /// User object
    /// </summary>
    object? Tag { get; set; }

    /// <summary>
    /// Retrieve e simple cmd representation of this figure.<br/>
    /// Its a textual representation of figure information useful to regen in a separate tool.
    /// </summary>
    string SimpleCmd();

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

    /// <summary>
    /// Sets <see cref="IGLFigure.ScreenCoordMode"/> on given figure.
    /// </summary>    
    public static T SetScreenMode<T>(this T figure, bool screenCoordMode) where T : IGLFigure =>
        figure.Act(fig => fig.ScreenCoordMode = screenCoordMode);

    /// <summary>
    /// Sets <see cref="IGLFigure.ScreenCoordMode"/> on given figures.
    /// </summary>    
    public static IEnumerable<T> SetScreenMode<T>(this IEnumerable<T> figures, bool screenCoordMode) where T : IGLFigure =>
        figures.Act(figs => figs.ForEach(fig => fig.ScreenCoordMode = screenCoordMode));

    /// <summary>
    /// Move the figure by the given delta.<br/>    
    /// </summary>
    /// <remarks>
    /// - No vertex change, but only the object matrix associated to this figure.
    /// - Object matrix will replaced with a single translation.
    /// </remarks>
    /// <param name="figure">Figure to move.</param>
    /// <param name="dx">Delta (x) [object]</param>
    /// <param name="dy">Delta (y) [object]</param>
    /// <param name="dz">Delta (z) [object]</param>
    /// <param name="relative">If true (default) sum the given delta elsewhere replace the object matrix translation.</param>
    /// <returns>This figure reference.</returns>
    public static T Move<T>(this T figure, float dx, float dy, float dz, bool relative = true) where T : IGLFigure =>
        figure.Move(new Vector3(dx, dy, dz), relative);

    /// <summary>
    /// Move the figure by the given delta.<br/>    
    /// </summary>
    /// <remarks>
    /// - No vertex change, but only the object matrix associated to this figure.
    /// - Object matrix will replaced with a single translation.
    /// </remarks>
    /// <param name="figure">Figure to move.</param>
    /// <param name="coord">Delta vector3 [object]</param>
    /// <param name="relative">If true (default) sum the given delta elsewhere replace the object matrix translation.</param>    
    /// <returns>This figure reference.</returns>
    public static T Move<T>(this T figure, in Vector3 coord, bool relative = true) where T : IGLFigure
    {
        figure.ObjectMatrix = figure.ObjectMatrix.SetOrigin(
            relative ? figure.ObjectMatrix.Origin() + coord : coord);

        return figure;
    }

    /// <summary>
    /// Move given figures by the given delta.<br/>    
    /// </summary>
    /// <remarks>
    /// - No vertex change, but only the object matrix associated to this figure.
    /// - Object matrix will replaced with a single translation.
    /// </remarks>
    /// <param name="figures">Figures to move.</param>
    /// <param name="coord">Delta vector3 [object]</param>
    /// <param name="relative">If true (default) sum the given delta elsewhere replace the object matrix translation.</param>    
    /// <returns>This figure reference.</returns>
    public static IEnumerable<T> Move<T>(this IEnumerable<T> figures, Vector3 coord, bool relative = true) where T : IGLFigure =>
        figures.Act(figs => figs.ForEach(fig => fig.Move(coord, relative)));

}