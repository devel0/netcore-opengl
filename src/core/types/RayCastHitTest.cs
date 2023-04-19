namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Hold raycast hit test info.<br/>
/// <seealso cref="SearchAThing.OpenGL.Core.Ext.Intersect(Line, float, GLFigureBase)"/>
/// </summary>
public class RayCastHitTest
{

    /// <summary>
    /// Figure hitted.
    /// </summary>    
    public GLFigureBase Figure { get; private set; }

    /// <summary>
    /// Figure primitive hitted.
    /// </summary>    
    public GLPrimitiveBase Primitive { get; private set; }

    /// <summary>
    /// Hit coord [object].
    /// </summary>    
    public Vector3 HitCoord { get; private set; }

    /// <summary>
    /// Distance of the ray from hitted primitive.<br/>
    /// For point and line entity this is the distance of the ray projection to entity.<br/>
    /// for triangle entity this value is 0.
    /// </summary>    
    public float Distance { get; private set; }

    /// <summary>
    /// Create ray cast hit test nfo object.
    /// </summary>
    /// <param name="figure">Figure hitted.</param>
    /// <param name="primitive">Figure primitive hitted.</param>
    /// <param name="hitCoord">Hit coord [object].</param>
    /// <param name="distance">Distance of the ray from hitted primitive.</param>
    public RayCastHitTest(GLFigureBase figure, GLPrimitiveBase primitive, Vector3 hitCoord, float distance)
    {
        Figure = figure;
        Primitive = primitive;
        HitCoord = hitCoord;
        Distance = distance;
    }

}