namespace SearchAThing.OpenGL.Render;

/// <summary>
/// General inteface for objects that carrier a reference to Gl context.
/// </summary>
/// <seealso cref="GLModel"/>
/// <seealso cref="GLPipeline"/>
/// <seealso cref="GLTexture2D"/>
public interface IGLContextObject
{

    GLContext GLContext { get; }

}