namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Helper to define and enable Vertex Array Object
/// in order to declare how to access vertex buffer object at i-th packet structure components.    
/// </summary>    
/// <typeparam name="PacketStructure">Type of packet structure used to store data <see cref="GLVertexStruct"/></typeparam>
public class GLVertexArrayObject<PacketStructure> : IDisposable
    where PacketStructure : unmanaged
{
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
                if (Handle.HasValue)
                {
                    GL.DeleteVertexArray(Handle.Value);
                }
                Handle = null;
            }
        }
        // Release of unmanaged resources

        disposed = true;
    }

    ~GLVertexArrayObject() => Dispose();

    #endregion

    uint? Handle;

    readonly GL GL;

    /// <summary>
    /// Execute glGenVertexArray() to create vertex array handle and glBindVertexArray on given target.<br/>
    /// It will automatically disposed with glDeleteVertexArray().
    /// </summary>	
    public GLVertexArrayObject(GL GL)
    {
        this.GL = GL;
        Handle = GL.GenVertexArray();
        if (Handle == null) throw new Exception($"GenVertexArray failed");
        GL.BindVertexArray(Handle.Value);
    }

    /// <summary>
    /// Define and enable location to access vertex buffer object at i-th packet structure components        .
    /// </summary>
    /// <param name="location">Index of attribute location.</param>
    /// <param name="offset">Offset in bytes from the packet structure begin.</param>
    /// <param name="components">Nr. of components (vec3=3; vec4=4).</param>
    /// <param name="componentType">Component data type.</param>        
    /// <param name="normalized">If true normalization will applied.</param>        
    /// <param name="stride">Sizeof in bytes of packet structure or null to get sizeof packet structure.</param>                                
    public unsafe void AttribPointer(uint location, int offset,
        int components,
        VertexAttribPointerType componentType,
        bool normalized,
        uint? stride = null)
    {
        GL.VertexAttribPointer(location, components, componentType, normalized,
            stride.HasValue ? stride.Value : (uint)sizeof(PacketStructure),
            (void*)offset);
        GL.EnableVertexAttribArray(location);
    }
   
    public override string ToString()
    {
        return $"{Handle}";
    }
}