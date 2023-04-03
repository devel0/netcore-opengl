namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Describe the strength (0..1) of the ambient, diffuse, specular light effects.
/// </summary>
public struct MaterialProperties
{

    /// <summary>
    /// Ambient strength ( 0..1 ).
    /// </summary>
    public float Ambient;

    /// <summary>
    /// Diffuse strength ( 0..1 ).
    /// </summary>
    public float Diffuse;

    /// <summary>
    /// Specular strength ( 0..1 ).
    /// </summary>
    public float Specular;

    public Vector3 ToVector3() => new Vector3(Ambient, Diffuse, Specular);

    /// <summary>
    /// Default material properties.
    /// </summary>    
    public static MaterialProperties Default
    {
        get
        {
            return new MaterialProperties
            {
                Ambient = DEFAULT_MaterialAmbientStrength,
                Diffuse = DEFAULT_MaterialDiffuseStrength,
                Specular = DEFAULT_MaterialSpecularStrength
            };
        }
    }

}

public static partial class Constants
{

    /// <summary>
    /// ( Default ) color.
    /// </summary>
    public static readonly Color DEFAULT_MaterialColor = Color.White;

    /// <summary>
    /// ( Default ) material properties ( ambient:0.8, diffuse:0.6, strength:0.6 ).
    /// </summary>
    public static readonly MaterialProperties DEFAULT_MaterialProperties = MaterialProperties.Default;

    /// <summary>
    /// ( Default ) material ambient strength ( 0.8 ).
    /// </summary>
    public const float DEFAULT_MaterialAmbientStrength = 0.8f;

    /// <summary>
    /// ( Default ) material diffuse strength ( 0.6 ).
    /// </summary>
    public const float DEFAULT_MaterialDiffuseStrength = 0.6f;

    /// <summary>
    /// ( Default ) material specular strength ( 0.6 ).
    /// </summary>
    public const float DEFAULT_MaterialSpecularStrength = 0.6f;

}
