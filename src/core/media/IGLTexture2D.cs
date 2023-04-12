namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Interface that describe the object hold a reference to a gl texture 2d.
/// </summary>
public interface IGLTexture2D
{

    /// <summary>
    /// Id of gl texture created through <see cref="GLTexture2D"/>
    /// </summary>    
    uint TextureId { get; }

}