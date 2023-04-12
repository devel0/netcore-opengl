namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Manage mapping of given template typed data to gpu memory.
/// </summary>
/// <typeparam name="T">Type of packet structure.</typeparam>
public class GLShaderStorageBlock<T> : IDisposable
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
                if (Buffer.HasValue)
                {
                    GL.DeleteBuffer(Buffer.Value);
                }
                Buffer = null;
            }
        }
        // Release of unmanaged resources

        disposed = true;
    }

    ~GLShaderStorageBlock() => Dispose();

    #endregion  

    uint? Buffer;

    readonly GLContext glCtx;

    readonly GL GL;

    string BlockName;

    uint BlockIndex;

    uint BlockBinding;

    public GLEnum Target { get; private set; } = GLEnum.ShaderStorageBuffer;

    /// <summary>
    /// Execute glGenBuffer to create buffer handle and glBindBuffer() on given target, finally glBufferData() to map given data onto block storage.
    /// </summary>
    /// <remarks>
    /// It will dispose automatically with the call of glDeleteBuffer().
    /// </remarks>
    /// <param name="glCtx"></param>
    /// <param name="shader"></param>
    /// <param name="blockName"></param>
    /// <param name="data"></param>
    public GLShaderStorageBlock(GLContext glCtx, GLPipeline shader, string blockName, Span<T> data)
    {
        this.glCtx = glCtx;
        this.GL = glCtx.GL;
        this.BlockName = blockName;

        if (data.Length == 0) return;

        {
            shader.GetStorageBlockBinding(BlockName, out var blockIndex, out var blockBinding);

            BlockIndex = blockIndex;
            BlockBinding = blockBinding;

            if (BlockBinding == 0) throw new Exception($"binding not defined for block named {BlockName}");
        }

        uint buffer = 0;

        unsafe
        {
            fixed (void* ptr = data)
            {
                var used_data_size = (uint)(sizeof(T) * data.Length);

                GL.GenBuffers(1, &buffer);

                GL.BindBuffer(BufferTargetARB.ShaderStorageBuffer, buffer);



                GL.BufferData(BufferTargetARB.ShaderStorageBuffer, used_data_size, ptr, BufferUsageARB.DynamicCopy);

                //var idx = GL.GetProgramResourceIndex(glCtx.Shader, GLEnum.ShaderStorageBlock, BlockName);                

                GL.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, BlockBinding, buffer);

                GL.BindBuffer(BufferTargetARB.ShaderStorageBuffer, 0);
            }
        }

        Buffer = buffer;
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

    public override string ToString() => $"{BlockName} loc:{BlockBinding} buffer:{Buffer}";

}