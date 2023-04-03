namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Plate geometry composed by four vertexes, used primarly to be converted in triangles with 
/// <see cref="SearchAThing.OpenGL.Core.Ext.GetTriangles"/>.
/// </summary>
public class Plate
{

    /// <summary>
    /// First plate point.
    /// </summary>    
    public Vector3 P1 { get; private set; }

    /// <summary>
    /// Second plate point.
    /// </summary>    
    public Vector3 P2 { get; private set; }

    /// <summary>
    /// Third plate point.
    /// </summary>    
    public Vector3 P3 { get; private set; }

    /// <summary>
    /// Fourth plate point.
    /// </summary>    
    public Vector3 P4 { get; private set; }

    /// <summary>
    /// Mean point of the plate.
    /// </summary>    
    public Vector3 Center => (P1 + P2 + P3 + P4) / 4;

    /// <summary>
    /// Create a plate with given four points.
    /// </summary>
    /// <param name="p1">First point.</param>
    /// <param name="p2">Second point.</param>
    /// <param name="p3">Third point.</param>
    /// <param name="p4">Fourth point.</param>
    public Plate(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
        P4 = p4;
    }

    /// <summary>
    /// Create a new plate with point P1, P2, P3, P4 sequence reversed to P4. P3, P2, P1.<br/>
    /// This is useful to invert normals of plate generated triangles (<see cref="SearchAThing.OpenGL.Core.Ext.GetTriangles"/>).
    /// </summary>    
    public Plate Invert() => new Plate(P4, P3, P2, P1);

    /// <summary>
    /// Create a new plate moved by the given delta vector.
    /// </summary>
    /// <param name="delta">Delta vector.</param>
    /// <returns>Moved plate.</returns>
    public Plate Move(Vector3 delta) => new Plate(P1 + delta, P2 + delta, P3 + delta, P4 + delta);

}

public static partial class Ext
{

    /// <summary>
    /// retrieve triangles plate p1,p2,p3 and p3,p4,p1    ;
    /// if quad is degenre triangle a single triangle will returned
    /// </summary>   

    /// <summary>
    /// Create 2 triangles from given plate.<br/>    
    /// Default normal of generated triangles will be in accord between them having:<br/>
    /// - first triangle (P1, P2, P3)
    /// - second triangle (P3, P4, P1)
    /// </summary>
    /// <param name="plate">Plate for which create 2 triangles.</param>
    /// <param name="color">(optional) color to apply to created triangles.</param>
    /// <param name="setTextureST">If true <see cref="GLVertex.TextureST"/> will be set in order to map texture on the plate.</param>
    /// <returns>Two triangles.</returns>
    public static IEnumerable<GLTriangle> GetTriangles(this Plate plate, Color? color = null, bool setTextureST = false)
    {
        var colorVector = color?.ToVector4();

        if (setTextureST)
        {
            var textureST_1 = new Vector2(0, 1);
            var textureST_2 = new Vector2(1, 1);
            var textureST_3 = new Vector2(1, 0);
            var textureST_4 = new Vector2(0, 0);

            if (plate.P1 == plate.P2)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P2, colorVector, textureST: textureST_1),
                    new GLVertex(plate.P3, colorVector, textureST: textureST_2),
                    new GLVertex(plate.P4, colorVector, textureST: textureST_3));
            }

            else if (plate.P2 == plate.P3)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P3, colorVector, textureST: textureST_1),
                    new GLVertex(plate.P4, colorVector, textureST: textureST_2),
                    new GLVertex(plate.P1, colorVector, textureST: textureST_3));
            }

            else if (plate.P3 == plate.P4)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P4, colorVector, textureST: textureST_1),
                    new GLVertex(plate.P1, colorVector, textureST: textureST_2),
                    new GLVertex(plate.P2, colorVector, textureST: textureST_3));
            }

            else if (plate.P4 == plate.P1)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P1, colorVector, textureST: textureST_1),
                    new GLVertex(plate.P2, colorVector, textureST: textureST_2),
                    new GLVertex(plate.P3, colorVector, textureST: textureST_3));
            }

            else
            {

                /*
                   0,0    1,0
                   v4     v3
                   +------+
                   |     /| ^
                   |   /xx| |
                   | /xxxx|
                   +------+
                   v1  -> v2
                   0,1    1,1
               */

                yield return new GLTriangle(
                    new GLVertex(plate.P1, colorVector, textureST: textureST_1),
                    new GLVertex(plate.P2, colorVector, textureST: textureST_2),
                    new GLVertex(plate.P3, colorVector, textureST: textureST_3));

                /*
                   0,0    1,0
                   v4  <- v3
                   +------+
                   |xxxxx/|
                 | |xxx/  |
                 v |x/    |
                   +------+
                   v1     v2
                   0,1    1,1
                 */

                yield return new GLTriangle(
                   new GLVertex(plate.P3, colorVector, textureST: textureST_3),
                   new GLVertex(plate.P4, colorVector, textureST: textureST_4),
                   new GLVertex(plate.P1, colorVector, textureST: textureST_1));

            }
        }

        else
        {
            if (plate.P1 == plate.P2)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P2, colorVector),
                    new GLVertex(plate.P3, colorVector),
                    new GLVertex(plate.P4, colorVector));
            }

            else if (plate.P2 == plate.P3)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P3, colorVector),
                    new GLVertex(plate.P4, colorVector),
                    new GLVertex(plate.P1, colorVector));
            }

            else if (plate.P3 == plate.P4)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P4, colorVector),
                    new GLVertex(plate.P1, colorVector),
                    new GLVertex(plate.P2, colorVector));
            }

            else if (plate.P4 == plate.P1)
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P1, colorVector),
                    new GLVertex(plate.P2, colorVector),
                    new GLVertex(plate.P3, colorVector));
            }

            else
            {
                yield return new GLTriangle(
                    new GLVertex(plate.P1, colorVector),
                    new GLVertex(plate.P2, colorVector),
                    new GLVertex(plate.P3, colorVector));

                yield return new GLTriangle(
                    new GLVertex(plate.P3, colorVector),
                    new GLVertex(plate.P4, colorVector),
                    new GLVertex(plate.P1, colorVector));
            }
        }
    }

}