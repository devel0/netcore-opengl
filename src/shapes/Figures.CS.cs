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
    /// <returns>Wcs figure.</returns>   
    public static GLLineFigure MakeWCSFigure(float size = 1, Color? originColor = null, bool setOriginColor = true, Vector3? origin = null) =>
        MakeCSFigure(WCS.Move(origin), size, originColor, setOriginColor);

    /// <summary>
    /// Make a figure that represents a cs with RGB X:red Y:green Z:blue axes.
    /// </summary>
    /// <param name="cs">Coordinate system to represents as a figure.</param>
    /// <param name="size">(Optional) Axes length (Default:1).</param>
    /// <param name="originColor">(Optional) Color of the origin (Default:white).</param>
    /// <param name="setOriginColor">(Optional) Set the color of the origin (Default:true).</param>    
    /// <returns>cs figure.</returns>   
    public static GLLineFigure MakeCSFigure(Matrix4x4 cs,
        float size = 1, Color? originColor = null, bool setOriginColor = true)
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

        var fig = new GLLineFigure(
            pos.LineV(basex * size).ToGLLine(xAxisColorFrom, xAxisColorTo),
            pos.LineV(basey * size).ToGLLine(yAxisColorFrom, yAxisColorTo),
            pos.LineV(basez * size).ToGLLine(zAxisColorFrom, zAxisColorTo));

        return fig;
    }

}
