using System;
using Silk.NET.OpenGL;

namespace SearchAThing
{

    /// <summary>
    /// hold some typed data to video card memory
    /// </summary>
    /// <typeparam name="T">type of packet structure</typeparam>
    public class VertexBufferObject<T> : IDisposable
        where T : unmanaged
    {
        public uint? Handle { get; private set; }
        readonly GL GL;
        public BufferTargetARB Target { get; private set; }

        public VertexBufferObject(GL GL, BufferTargetARB target, Span<T> data, BufferUsageARB usage = BufferUsageARB.StaticDraw)
        {
            this.GL = GL;
            this.Target = target;
            Handle = GL.GenBuffer();
            Bind();

            unsafe
            {
                fixed (void* ptr = data)
                {
                    GL.BufferData(target, (uint)(sizeof(T) * data.Length), ptr, usage);
                }
            }
        }

        public void Bind()
        {
            GL.BindBuffer(Target, Handle.Value);
        }

        public void Dispose()
        {
            if (Handle.HasValue)
            {
                GL.DeleteBuffer(Handle.Value);
            }
            Handle = null;
        }

        public override string ToString()
        {
            return $"{Handle}";
        }

    }

}