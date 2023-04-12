using System.Runtime.InteropServices;

namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Structure that contains gl point light info.<br/>
/// It will be used by the <see cref="GLControl.RenderVertexManager"/> 
/// and loaded into the gpu pipeline through <see cref="GLShaderStorageBlock{T}"/>
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = (12 + 4) + (12 + 4) + (12 + 4) + (12 + 4))]
public unsafe struct GLPointLightStruct
{

    /// <summary>
    /// Light position [world].
    /// </summary>
    [FieldOffset((0))]
    public Vector3 position;

    /// <summary>
    /// Light constant attenuation (0..1).
    /// <see cref="DEFAULT_LIGHT_CONSTANT"/>
    /// </summary>
    [FieldOffset(12)]
    public float constant;

    /// <summary>
    /// Ambient color.<br/>
    /// <see cref="DEFAULT_LIGHT_COLOR"/>
    /// </summary>
    [FieldOffset((12 + 4))]
    public Vector3 ambient;

    /// <summary>
    /// Light linear attenutation (0..1).
    /// <see cref="DEFAULT_LIGHT_LINEAR"/>
    /// </summary>
    [FieldOffset((12 + 4) + 12)]
    public float linear;

    /// <summary>
    /// Diffuse color.<br/>
    /// <see cref="DEFAULT_LIGHT_COLOR"/>
    /// </summary>
    [FieldOffset((12 + 4) + (12 + 4))]
    public Vector3 diffuse;

    /// <summary>
    /// Light quadratic attenuation (0..1).
    /// <see cref="DEFAULT_LIGHT_QUADRATIC"/>.
    /// </summary>
    [FieldOffset((12 + 4) + (12 + 4) + 12)]
    public float quadratic;

    /// <summary>
    /// Specular color.<br/>
    /// <see cref="DEFAULT_LIGHT_COLOR"/>
    /// </summary>
    [FieldOffset((12 + 4) + (12 + 4) + (12 + 4))]
    public Vector3 specular;

    // pad: 4 

    public override string ToString() => $"{position}";

}