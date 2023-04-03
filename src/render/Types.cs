namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Type of <see cref="GLControl"/> invalidation.
/// </summary>
public enum InvalidateEnum
{

    /// <summary>
    /// Do not render.
    /// </summary>
    None,

    /// <summary>
    /// Render without forcing <see cref="GLModel.BuildModel"/>.
    /// </summary>
    RedrawOnly,

    /// <summary>
    /// Render forcing regeneration of model through <see cref="GLModel.BuildModel"/>.
    /// </summary>
    RebuildModelAndRedraw,

}

/// <summary>
/// Gl text alignment respect given insertion point.
/// </summary>
/// <seealso cref="GLText"/>.
public enum GLTextVHAlignment
{
    TopLeft,
    TopCenter,
    TopRight,

    MiddleLeft,
    MiddleCenter,
    MiddleRight,

    BottomLeft,
    BottomCenter,
    BottomRight,

}

public class ViewNfo
{
    public string Title { get; set; }
    public Matrix4x4 ModelMatrix { get; set; }
    public Vector3 CameraPos { get; set; }
    public Vector3 CameraTarget { get; set; }
    public Vector3 CameraUp { get; set; }
    public bool Wireframe { get; set; }
    public bool Perspective { get; set; }
    public float OrthoZoom { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }
    public bool ShadeWithEdge { get; set; }
    public bool ShowCameraObject { get; set; }
}
