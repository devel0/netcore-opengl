namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Primitive for GL_TRIANGLES opengl element.
/// </summary>
public interface IGLTriangle : IGLPrimitive
{

    /// <summary>
    /// Triangle first vertex.
    /// </summary>    
    GLVertex V1 { get; set; }

    /// <summary>
    /// Triangle second vertex.
    /// </summary>    
    GLVertex V2 { get; set; }

    /// <summary>
    /// Triangle third vertex.
    /// </summary>    
    GLVertex V3 { get; set; }

    /// <summary>
    /// Compute default triangle plane such as:<br/>
    /// - origin = V1<br/>
    /// - normal = Normalize((V2-V1) x (V3-V1))
    /// </summary>    
    /// <seealso cref="Toolkit.DefaultTriangleNormal(IGLTriangle)"/>
    Matrix4x4 Plane { get; }

    /// <summary>
    /// Compute triangle plane such as:<br/>
    /// - origin = V1<br/>
    /// - basez is concordant to owner figure <see cref="GLTriangleFigure.ComputeNormal"/> delegate.
    /// </summary>        
    Matrix4x4 PlaneNZ { get; }

    /// <summary>
    /// States if given point is contained in the triangle, borders included.
    /// </summary>
    /// <param name="point">Point to test.</param>    
    bool Contains(in Vector3 point);   

}

public static partial class Toolkit
{

    /// <summary>
    /// Default triangle compute normal function:<br/>
    /// Normalize((V2-V1) x (V3-V1))
    /// </summary>
    /// <param name="tri">Triangle which compute default normal.</param>
    /// <returns>Triangle default normal.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 DefaultTriangleNormal(this IGLTriangle tri) =>
        Vector3.Normalize(Vector3.Cross(tri.V2.Position - tri.V1.Position, tri.V3.Position - tri.V1.Position));

}

