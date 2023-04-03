namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Object that holds information about gl split view configuration.
/// </summary>
public class ViewLayoutNfo
{

    /// <summary>
    /// Sequence of splits.
    /// </summary>    
    public GridSplitterManagerLayoutItem? Layout { get; set; }
    
    /// <summary>
    /// Dictionary UID view to view config itself.
    /// </summary>    
    public Dictionary<int, ViewNfo> UIDView { get; set; }

    /// <summary>
    /// UID of view focused between all of the layout.
    /// </summary>    
    public int FocusedUIDView { get; set; }

}
