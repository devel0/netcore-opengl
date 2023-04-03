namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Specialization of <see cref="IGLFigure"/> for the type <see cref="GLPoint"/>.
/// </summary>
public interface IGLPointFigure : IGLFigure, INotifyPropertyChanged
{

    /// <summary>
    /// Size of the point in pixels [screen] ( Default: 10 ).<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary> 
    float PointSize { get; set; }

    /// <summary>
    /// Add given point to the figure primitives set.
    /// </summary>
    void Add(GLPoint point);

    /// <summary>
    /// Remove given point from the figure primitives set.
    /// </summary>    
    void Remove(GLPoint point);

}

public static partial class Constants
{

    /// <summary>
    /// (Default) <see cref="IGLPointFigure.PointSize"/> of a point figure.
    /// </summary>
    public const float DEFAULT_PointSize = 10f;

}