using System.Numerics;
using System.Runtime.InteropServices;

namespace SearchAThing
{

    /// <summary>
    /// contains a vector3
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLVertex
    {
        public Vector3 Position;
    }

    /// <summary>
    /// structure used by GLVertexManager when BuildPoints called
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct GLVertexWithNormal
    {
        public Vector3 Position;
        public Vector3 Normal;
    }

    /// <summary>
    /// used by GLVertexManager to store info about position
    /// </summary>
    public class GLVertexNfo
    {
        public Vector3 Position;
        /// <summary>
        /// convert this object to a struct suitable for GL operations
        /// </summary>        
        public GLVertex ToGLVertex() => new GLVertex { Position = this.Position };
    }

    /// <summary>
    /// used by GLVertexManager to store info about position, normal of vertexes
    /// </summary>
    public class GLVertexWithNormalNfo : GLVertexNfo
    {
        public Vector3 Normal;
        /// <summary>
        /// convert this object to a struct suitable for GL operations
        /// </summary>
        public GLVertexWithNormal ToGLVertexWithNormal() => new GLVertexWithNormal
        {
            Position = this.Position,
            Normal = this.Normal
        };
    }

}