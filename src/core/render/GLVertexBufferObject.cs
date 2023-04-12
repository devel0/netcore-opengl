namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Manage mapping of given template typed data to gpu memory.
/// </summary>
/// <typeparam name="T">Type of packet structure.</typeparam>
public class GLVertexBufferObject<T> : IDisposable
    where T : unmanaged
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
                    GL.DeleteBuffer(Handle.Value);
                }
                Handle = null;
            }
        }
        // Release of unmanaged resources

        disposed = true;
    }

    ~GLVertexBufferObject() => Dispose();

    #endregion

    uint? Handle;

    readonly GL GL;

    BufferTargetARB Target;

    /// <summary>
    /// Executes gLGenBuffer() to create buffer handle and gLBindBuffer() on given target,
    /// then gLBufferData() to map on given data.<br/>
    /// It will be automatically disposed with the call of glDeleteBuffer().
    /// </summary>        
    public GLVertexBufferObject(GL GL, BufferTargetARB target, Span<T> data, BufferUsageARB usage = BufferUsageARB.DynamicDraw)
    {
        this.GL = GL;
        this.Target = target;
        Handle = GL.GenBuffer();

        GL.BindBuffer(Target, Handle.Value);

        unsafe
        {
            fixed (void* ptr = data)
            {
                GL.BufferData(target, (uint)(sizeof(T) * data.Length), ptr, usage);
            }
        }
    }

    public void ReplaceItems(int offset, Span<T> newdata)
    {
        unsafe
        {
            fixed (void* ptr = newdata)
            {
                GL.BufferSubData(Target, sizeof(T) * offset, (uint)(sizeof(T) * newdata.Length), ptr);
            }
        }
    }

    public override string ToString()
    {
        return $"{Handle}";
    }

}