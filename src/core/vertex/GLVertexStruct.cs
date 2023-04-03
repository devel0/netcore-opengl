using System.Runtime.InteropServices;

namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Structure used by the vertex manager when register a vertex inside it.<br/>
/// This structure has an explicit layout for unmanaged access by the gpu
/// and its used in <seealso cref="SearchAThing.OpenGL.Render.GLControl"/> during the phase of rendering.<br/>
/// </summary>
/// <seealso cref="SearchAThing.OpenGL.Render.GLVertexArrayObject"/>
[StructLayout(LayoutKind.Explicit, Size = 12 + 12 + 16 + 12 + 8)]
public struct GLVertexStruct
{

    /// <summary>
    /// Vertex position [local].
    /// </summary>
    [FieldOffset(0)]
    public Vector3 Position;

    /// <summary>
    /// Vertex normal [local].
    /// </summary>
    [FieldOffset(12)]
    public Vector3 Normal;

    /// <summary>
    /// Vertex color encoded in vector4 (range 0..1) as (red, green, blue, alpha).<br/>
    /// Alpha=1 (full opaque).<br/>
    /// Alpha=0 (full transparent).
    /// </summary>
    [FieldOffset(12 + 12)]
    public Vector4 MatColor;

    /// <summary>
    /// Describe the strength (0..1) of the ambient, diffuse, specular light effects.
    /// </summary>
    [FieldOffset(12 + 12 + 16)]
    public Vector3 MatProp;

    /// <summary>
    /// Texture mapping position in the range [0,0]...[1,1].
    /// </summary>
    [FieldOffset(12 + 12 + 16 + 12)]
    public Vector2 TextureST;        
    
    public override string ToString() => $"pos:{Position} norm:{Normal} mcol:{MatColor} mprop:{MatProp} textureST:{TextureST}";
}
