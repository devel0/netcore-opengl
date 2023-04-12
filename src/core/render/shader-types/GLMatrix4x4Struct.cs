using System.Runtime.InteropServices;

namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Structure that contains a matrix4x4.<br/>
/// It will be used by the <see cref="GLControl.RenderVertexManager"/>
/// and loaded into gpu pipeline through <see cref="GLShaderStorageBlock{T}"/>.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = (4 * 4))]
public unsafe struct GLMatrix4x4Struct
{

    [FieldOffset((0))]
    public Matrix4x4 m;

}
