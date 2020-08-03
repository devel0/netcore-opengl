using System;
using Silk.NET.OpenGL;

namespace SearchAThing
{

    /// <summary>
    /// Helper to define and enable Vertex Array Object
    /// in order to declare how to access vertex buffer object at i-th packet structure components.    
    /// </summary>    
    /// <typeparam name="PacketStructure">Type of packet structure used to store data (ie.GLVertexWithNormal)</typeparam>
    public class VertexArrayObject<PacketStructure> : IDisposable
        where PacketStructure : unmanaged
    {

        public uint? Handle { get; private set; }
        readonly GL GL;

        public VertexArrayObject(GL GL)
        {
            this.GL = GL;
            Handle = GL.GenVertexArray();
            Bind();
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle.Value);
        }

        /// <summary>
        /// define and enable location to access vertex buffer object at i-th packet structure components        
        /// </summary>
        /// <param name="location">index of attribute location</param>
        /// <param name="offset">offset in bytes from the packet structure begin</param>
        /// <param name="components">nr. of components (vec3=3; vec4=4)</param>
        /// <param name="componentType">component data type</param>        
        /// <param name="normalized">if true normalization will applied</param>        
        /// <param name="stride">sizeof in bytes of packet structure or null to get sizeof packet structure</param>                                
        public unsafe void AttribPointer(uint location, int offset,
            int components = 3,
            VertexAttribPointerType componentType = VertexAttribPointerType.Float,
            bool normalized = false,
            uint? stride = null)
        {
            GL.VertexAttribPointer(location, components, componentType, normalized,
                stride.HasValue ? stride.Value : (uint)sizeof(PacketStructure),
                (void*)offset);
            GL.EnableVertexAttribArray(location);
        }

        public void Dispose()
        {
            if (Handle.HasValue)
            {
                GL.DeleteVertexArray(Handle.Value);
            }
            Handle = null;
        }

        public override string ToString()
        {
            return $"{Handle}";
        }
    }

}