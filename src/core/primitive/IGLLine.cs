namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Primitive for the GL_LINES opengl element.
/// </summary>
public interface IGLLine : IGLPrimitive
{

    /// <summary>
    /// Line from vertex.<br/>    
    /// </summary>    
    /// <remarks>
    /// Changing this property will:<br/>
    /// - checks that the incoming value is a detached vertex ( <see cref="SearchAThing.OpenGL.Core.Ext.EnsureDetached"/> ).<br/>
    /// - use <see cref="GLVertexManager.RemoveVertex"/><br/>
    /// - set the value<br/>
    /// - use <see cref="SearchAThing.OpenGL.Core.GLVertexManager.AddVertex">AddVertex</see><br/>
    /// </remarks>
    GLVertex From { get; set; }

    /// <summary>
    /// Line to vertex.
    /// </summary>    
    /// <remarks>
    /// Changing this property will:<br/>
    /// - checks that the incoming value is a detached vertex ( <see cref="Ext.EnsureDetached"/> ).<br/>
    /// - use <see cref="GLVertexManager.RemoveVertex"/><br/>
    /// - set the value<br/>
    /// - use <see cref="GLVertexManager.AddVertex"/><br/>
    /// </remarks>    
    GLVertex To { get; set; }

}

public static partial class Ext
{

    /// <summary>
    /// Create a gl line primitive from given position and vector extension.
    /// </summary>
    /// <param name="p">From position.</param>
    /// <param name="v">Extension vector.</param>
    /// <param name="color">(Optional) from, to line vertexes color.</param>    
    public static GLLine LineV(this in Vector3 p, in Vector3 v, in Color? color = null) =>
        GLLine.PointV(
            new GLVertex(p, color is not null ? color.Value.ToVector4() : null),
            new GLVertex(v, color is not null ? color.Value.ToVector4() : null));

    /// <summary>
    /// Create a gl line primitive from given two points.
    /// </summary>
    /// <param name="from">From position.</param>
    /// <param name="to">To position.</param>
    /// <param name="color">(Optional) from, to line vertexes color.</param>
    /// <returns></returns>
    public static GLLine LineTo(this in Vector3 from, in Vector3 to, in Color? color = null) =>
        GLLine.FromTo(
            new GLVertex(from, color is not null ? color.Value.ToVector4() : null),
            new GLVertex(to, color is not null ? color.Value.ToVector4() : null));

    /// <summary>
    /// Create a gl line primitive from given line geometry.
    /// </summary>
    /// <param name="line">Line geometry.</param>
    /// <param name="color">(Optional) from, to line vertexes color.</param>
    /// <returns>GL line primitive.</returns>
    public static GLLine ToGLLine(this Line line, in Color? color) =>
        GLLine.FromTo(
            new GLVertex(line.From, color is not null ? color.Value.ToVector4() : null),
            new GLVertex(line.To, color is not null ? color.Value.ToVector4() : null));

    /// <summary>
    /// Create a gl line primitive from given line geometry applying given from, to colors.
    /// </summary>
    /// <param name="line">Line geometry.</param>
    /// <param name="colorFrom">Color to apply the from vertex.</param>
    /// <param name="colorTo">Color to apply the to vertex.</param>
    /// <returns>GL line primitive.</returns>
    public static GLLine ToGLLine(this Line line, in Color colorFrom, in Color colorTo) =>
        GLLine.FromTo(
            new GLVertex(line.From, colorFrom),
            new GLVertex(line.To, colorTo));

}