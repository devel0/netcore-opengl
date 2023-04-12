namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Handle reference to gl texture.
/// </summary>
public class GLTexture2D : IGLContextObject, IGLTexture2D, IDisposable
{

    #region IGLObject

    /// <summary>
    /// Gl context.
    /// </summary>
    public GLContext GLContext { get; private set; }

    #endregion

    /// <summary>
    /// Gl api.
    /// </summary>
    public GL GL => GLContext.GL;

    /// <summary>
    /// Skia bitmap that hold texture bitmap.
    /// </summary>    
    public SKBitmap Bitmap { get; private set; }

    /// <summary>
    /// Texture id.
    /// </summary>    
    public uint TextureId { get; internal set; }

    /// <summary>
    /// Width of the bitmap (pixels).
    /// </summary>
    public uint Width => (uint)Bitmap.Width;

    /// <summary>
    /// Height of the bitmap (pixels).
    /// </summary>    
    public uint Height => (uint)Bitmap.Height;

    /// <summary>
    /// Gpu internal storage format.
    /// </summary>    
    public InternalFormat InternalFormat { get; private set; }

    /// <summary>
    /// Image buffer format.
    /// </summary>
    public PixelFormat PixelFormat { get; private set; }

    /// <summary>
    /// Create gl texture 2d.
    /// </summary>
    /// <param name="glContext">Gl context.</param>
    /// <param name="bitmap">Skia bitmap that hold texture bitmap.</param>
    /// <param name="internalFormat">Gpu internal storage format.</param>
    /// <param name="pixelFormat">Image buffer format.</param>
    public GLTexture2D(GLContext glContext, SKBitmap bitmap,
        InternalFormat internalFormat = DEFAULT_TEXTURE_INTERNAL_FORMAT,
        PixelFormat pixelFormat = DEFAULT_PIXEL_FORMAT)
    {
        GLContext = glContext;
        Bitmap = bitmap;

        this.InternalFormat = internalFormat;
        this.PixelFormat = pixelFormat;

        GLContext.AutoCleanup();

        CreateTexture();
    }

    #region IDisposable & destructor

    bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Manual release of managed resources                      

                if (TextureId != 0)
                {
                    // mark textureId to be freed at first model rebuild
                    GLContext.AddDisposedTextureId(TextureId);

                    // FOLLOW CALL HERE generate exception
                    //
                    // GL.DeleteTexture(TextureId);
                    //
                    /*
                    Exception has occurred: CLR/Silk.NET.Core.Loader.SymbolLoadingException
An unhandled exception of type 'Silk.NET.Core.Loader.SymbolLoadingException' occurred in Silk.NET.GLFW.dll: 'Native symbol not found (Symbol: glDeleteTextures)'
  at Silk.NET.GLFW.GlfwContext.<GetProcAddress>g__Throw|3_0(String proc)
  at Silk.NET.GLFW.GlfwContext.GetProcAddress(String proc, Nullable`1 slot)
  at Silk.NET.OpenGL.GL._B.get__AJO()
  at Silk.NET.OpenGL.GL.DeleteTextures(UInt32 n, UInt32* textures)
  at Silk.NET.OpenGL.GL.DeleteTexture(UInt32 textures)
  at SearchAThing.OpenGL.Render.GLTexture2D.Dispose(Boolean disposing) in /home/devel0/Documents/opensource/netcore-opengl/src/render/GLTexture2D.cs:line 66
  at SearchAThing.OpenGL.Render.GLTexture2D.Dispose() in /home/devel0/Documents/opensource/netcore-opengl/src/render/GLTexture2D.cs:line 50
  at SearchAThing.OpenGL.Render.GLTexture2D.Finalize() in /home/devel0/Documents/opensource/netcore-opengl/src/render/GLTexture2D.cs:line 76
                    */

                }
            }
        }
        // Release of unmanaged resources

        disposed = true;
    }

    ~GLTexture2D() => Dispose();

    #endregion        

    void CreateTexture()
    {
        TextureId = GL.GenTexture();
        GL.ActiveTexture(MAIN_TEXTURE_UNIT);
        GL.BindTexture(TextureTarget.Texture2D, TextureId);

        // Debug.WriteLine($"Created TextureId {TextureId}");

        ReadOnlySpan<byte> rgba_pixels = Bitmap.Pixels
            .SelectMany(skc => new byte[]
            {
                skc.Red,
                skc.Green,
                skc.Blue,
                skc.Alpha
            })
            .ToArray();

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0, // level                                            
            InternalFormat,
            Width,
            Height,
            0, // border
            PixelFormat,
            PixelType.UnsignedByte,
            rgba_pixels);

        GL.GenerateMipmap(TextureTarget.Texture2D);
    }

}

public static partial class Ext
{

    /// <summary>
    /// Load texture from given bitmap pathfilename.
    /// </summary>
    /// <param name="model">Gl model into which load the texture.</param>
    /// <param name="pathfilename">Texture bitmap pathfilename.</param>
    /// <returns>Gl texture 2d object.</returns>
    public static GLTexture2D LoadTexture(this GLModel model, string pathfilename) =>
        new GLTexture2D(model.GLContext, SKBitmap.Decode(pathfilename));

}
