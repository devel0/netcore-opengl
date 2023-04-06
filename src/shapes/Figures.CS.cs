namespace SearchAThing.OpenGL.Shapes;

public static partial class Toolkit
{

    /// <summary>
    /// Make a figure that represents a wcs with RGB X:red Y:green Z:blue axes.
    /// </summary>
    /// <param name="size">(Optional) Axes length (Default:1).</param>
    /// <param name="originColor">(Optional) Color of the origin (Default:white).</param>
    /// <param name="setOriginColor">(Optional) Set the color of the origin (Default:true).</param>
    /// <param name="origin">(Optional) Wcs origin (Default:0,0,0).</param>
    /// <param name="solidArrowsDiameterFactor">(Optional) If not null arrows instead lines will created with specified diameter factor.
    /// <see cref="Arrow.Arrow(in System.Numerics.Vector3, in System.Numerics.Vector3, float?)"/> (Default:null).</param>    
    /// <returns>Wcs figure.</returns>   
    public static GLFigureBase MakeWCSFigure(float size = 1, Color? originColor = null,
        bool setOriginColor = true, Vector3? origin = null,
        float? solidArrowsDiameterFactor = null) =>
        MakeCSFigure(WCS.Move(origin), size, originColor, setOriginColor, solidArrowsDiameterFactor);

    /// <summary>
    /// Make a figure that represents a cs with RGB X:red Y:green Z:blue axes.
    /// </summary>
    /// <param name="cs">Coordinate system to represents as a figure.</param>
    /// <param name="size">(Optional) Axes length (Default:1).</param>
    /// <param name="originColor">(Optional) Color of the origin (Default:white).</param>
    /// <param name="setOriginColor">(Optional) Set the color of the origin (Default:true).</param>    
    /// <param name="solidArrowsDiameterFactor">(Optional) If not null arrows instead lines will created with specified diameter factor.
    /// <see cref="Arrow.Arrow(in System.Numerics.Vector3, in System.Numerics.Vector3, float?)"/> (Default:null).</param>    
    /// <returns>cs figure.</returns>   
    public static GLFigureBase MakeCSFigure(Matrix4x4 cs,
        float size = 1, Color? originColor = null, bool setOriginColor = true,
        float? solidArrowsDiameterFactor = null)
    {
        var pos = cs.Origin();
        var basex = cs.BaseX();
        var basey = cs.BaseY();
        var basez = cs.BaseZ();

        var xAxisColorTo = Color.Red;
        var yAxisColorTo = Color.Green;
        var zAxisColorTo = Color.Blue;

        if (originColor is null) originColor = Color.White;
        var xAxisColorFrom = setOriginColor ? originColor.Value : xAxisColorTo;
        var yAxisColorFrom = setOriginColor ? originColor.Value : yAxisColorTo;
        var zAxisColorFrom = setOriginColor ? originColor.Value : zAxisColorTo;

        GLFigureBase fig;

        if (solidArrowsDiameterFactor is not null)
        {
            var xFigs = new Arrow(pos, pos + basex, diameterFactor: solidArrowsDiameterFactor.Value)
                .Triangles().ToList().SetColor(xAxisColorTo);

            var yFigs = new Arrow(pos, pos + basey, diameterFactor: solidArrowsDiameterFactor.Value)
                .Triangles().ToList().SetColor(yAxisColorTo);

            var zFigs = new Arrow(pos, pos + basez, diameterFactor: solidArrowsDiameterFactor.Value)
                .Triangles().ToList().SetColor(zAxisColorTo);

            var tris = xFigs.Union(yFigs).Union(zFigs);

            fig = new GLTriangleFigure(tris);

            fig.ExcludeFromShadeWithEdge = true;
        }

        else
        {
            fig = new GLLineFigure(
                pos.LineV(basex * size).ToGLLine(xAxisColorFrom, xAxisColorTo),
                pos.LineV(basey * size).ToGLLine(yAxisColorFrom, yAxisColorTo),
                pos.LineV(basez * size).ToGLLine(zAxisColorFrom, zAxisColorTo));
        }


        return fig;
    }

}
