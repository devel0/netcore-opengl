namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Primitive for GL_POINTS opengl element.
/// </summary>
public interface IGLPoint : IGLPrimitive
{

    /// <summary>
    /// Vertex of the gl point.
    /// </summary>    
    GLVertex Vertex { get; set; }

}

public static partial class Ext
{

    /// <summary>
    /// Create dxf point from given gl point.
    /// </summary>    
    public static netDxf.Entities.Point ToDxfPoint(this IGLPoint point) =>
        new netDxf.Entities.Point(point.Vertex.Position.ToDxfVector3())
        { Color = point.Vertex.MaterialColor.ToColor().ToDxfColor() };

}
