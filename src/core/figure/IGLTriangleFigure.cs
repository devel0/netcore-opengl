namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Compute normal function delegate.
/// </summary>
/// <param name="triangle">Triangle primitive which to compute normal.</param>
/// <param name="vertex">Triangle vertex which to compute normal.</param>
/// <returns>Vector that represents the normal of the vertex of the triangle.</returns>
public delegate Vector3 ComputeTriangleNormalDelegate(IGLTriangle triangle, IGLVertex vertex);

/// <summary>
/// Specialization of <see cref="IGLFigure"/> for the type <see cref="GLTriangle"/>.
/// </summary>
public interface IGLTriangleFigure : IGLFigure
{

    /// <summary>
    /// Optional texture 2d object associated to this triangle.<br/>
    /// TextureST vertex coords of triangles vertexes will be used to map the texture.<br/>
    /// Changing this property emits <see cref="IGLFigure.FigureInvalidated"/> event.
    /// </summary>
    IGLTexture2D? Texture2D { get; set; }

    /// <summary>
    /// Compute triangle normal function ( Default: <see cref="GLTriangleFigure.DefaultComputeNormal"/> ).<br/>
    /// Arguments are IGLTriangle, IGLVertex.<br/>
    /// Changing this property imply <see cref="GLTriangleFigure.RebuildNormal"/> event.
    /// </summary>    
    ComputeTriangleNormalDelegate ComputeNormal { get; set; }

    /// <summary>
    /// Add given point to the figure primitives set.
    /// </summary>
    void Add(GLTriangle point);

    /// <summary>
    /// Add given point to the figure primitives set.
    /// </summary>
    void Remove(GLTriangle point);

}
