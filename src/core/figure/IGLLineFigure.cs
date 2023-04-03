namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Specialization of <see cref="IGLFigure"/> for the type <see cref="GLLine"/>.
/// </summary>
public interface IGLLineFigure : IGLFigure, INotifyPropertyChanged
{

    /// <summary>
    /// Add given line to the figure primitives set.
    /// </summary>
    void Add(GLLine line);

    /// <summary>
    /// Remove given line from the figure primitives set.
    /// </summary>    
    void Remove(GLLine line);

}
