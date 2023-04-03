namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Primitive for GL_POINTS opengl element.
/// </summary>
public interface IGLPoint : IGLPrimitive
{

    /// <summary>
    /// Vertex of the gl point.
    /// </summary>    
    GLVertex Vertex { get; set; }

}
