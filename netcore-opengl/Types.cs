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
    /// used by GLVertexManager to store info about position, normal of vertexes
    /// </summary>
    public class GLVertexWithNormalNfo
    {
        public Vector3D Position;
        public Vector3D Normal;
    }

}