namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Helper class to save <see cref="GLControl"/> gl matrixes and a 
/// screen start point mainly used for pan and rotate at the beginning
/// of operation.
/// </summary>
public class MouseStart
{

    public Vector2 StartPt { get; private set; }
    public GLMatrixes GLMatrixes { get; private set; }

    public MouseStart(GLControl glControl, in Vector2 startPt)
    {
        StartPt = startPt;
        GLMatrixes = glControl.GetGLMatrixes();
    }

}
