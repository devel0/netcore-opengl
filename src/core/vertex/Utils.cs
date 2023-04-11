namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// Retrieve vector3 position signature within given tolerance.
    /// </summary>    
    public static string PositionSignature(this Vector3 position, float tol) =>
        Invariant($"{position.X.MRound(tol)}_{position.Y.MRound(tol)}_{position.Z.MRound(tol)}");

    /// <summary>
    /// Retrieve vertex position signature within given tolerance.
    /// </summary>    
    public static string PositionSignature(this IGLVertex glVertex, float tol) =>
        glVertex.Position.PositionSignature(tol);

    /// <summary>
    /// From given figure create a vertex position based dictionary suitable to be queries through vertex
    /// <see cref="PositionSignature"/>.
    /// </summary>
    /// <param name="figure">Gl figure which retrieve vertexes.</param>
    /// <param name="tol">Vertex position tolerance to group them.</param>
    /// <returns>Dictionary gl vertex position signature to list of gl vertexes within that position.</returns>
    public static Dictionary<string, List<GLVertex>> BuildVertexPosDict(this IGLFigure figure, float tol) =>
        new[] { figure }.BuildVertexPosDict(tol);

    /// <summary>
    /// From given list of figures create a vertex position based dictionary suitable to be queries through vertex
    /// <see cref="PositionSignature"/>.
    /// </summary>
    /// <param name="figures">Gl figures which retrieve vertexes.</param>
    /// <param name="tol">Vertex position tolerance to group them.</param>
    /// <returns>Dictionary gl vertex position signature to list of gl vertexes within that position.</returns>
    public static Dictionary<string, List<GLVertex>> BuildVertexPosDict(this IEnumerable<IGLFigure> figures, float tol) =>
        figures
            .SelectMany(fig => fig.Vertexes())
            .Select(vtx => new
            {
                vtx,
                vtxPosSignature = vtx.PositionSignature(tol)
            })
            .GroupBy(grp => grp.vtxPosSignature)
            // distinct gl vertex by ref ( content uniqueness already ensure by gl vertex manager from where they come )
            .ToDictionary(grp => grp.Key, grp => grp.Distinct().Select(g => g.vtx).ToList());

    /// <summary>
    /// Convert given system numerics vector3 to dxf vector3
    /// </summary>
    public static netDxf.Vector3 ToDxfVector3(this in Vector3 v) => new netDxf.Vector3(v.X, v.Y, v.Z);
}