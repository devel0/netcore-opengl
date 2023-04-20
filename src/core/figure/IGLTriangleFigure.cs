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
    GLTexture2D? Texture2D { get; set; }    

    /// <summary>
    /// Set the triangle compute normal function.<br/>
    /// If no argument given a default per triangle normal compute will be done.<br/>    
    /// Changing this property imply <see cref="GLTriangleFigure.RebuildNormal"/> event.
    /// </summary>
    /// <param name="mean">If true a mean of normals triangle vertexes grouped by position will be computed.</param>
    /// <param name="computeNormal">Allow to override default triangle normal function <see cref="GLTriangleFigure.DefaultComputeNormal"/>.</param>
    void SetupComputeNormal(bool mean, ComputeTriangleNormalDelegate? computeNormal);

    /// <summary>
    /// Add given point to the figure primitives set.
    /// </summary>
    void Add(GLTriangle point);

    /// <summary>
    /// Add given point to the figure primitives set.
    /// </summary>
    void Remove(GLTriangle point);

}
