namespace SearchAThing.OpenGL.Shapes;

/// <summary>
/// Box shape.
/// </summary>
public class Box
{

    /// <summary>
    /// Coordinate system for the box construction.
    /// </summary>        
    public Matrix4x4 CS { get; private set; }

    /// <summary>
    /// If true <see cref="CS"/> represents the middle of the box.<br/>
    /// If false it represents the cs placed in the middle of lower face.
    /// </summary>    
    public bool CSBaseIsMiddle { get; private set; }

    /// <summary>
    /// Size of the box.
    /// </summary>    
    public Vector3 CSSize { get; private set; }

    /// <summary>
    /// Elevation of the box as CSSize.Z.
    /// </summary>    
    public float Elevation => Abs(CSSize.Z);

    public GLTriangleFigure SideTop { get; private set; }

    public GLTriangleFigure SideBottom { get; private set; }

    public GLTriangleFigure SideLeft { get; private set; }

    public GLTriangleFigure SideRight { get; private set; }

    public GLTriangleFigure SideFront { get; private set; }

    public GLTriangleFigure SideBack { get; private set; }

    public Vector3 Center { get; private set; }

    /// <summary>
    /// Enumerates box sides figures ( BOTTOM, TOP, LEFT, RIGHT, FRONT, BACK ).<br/>
    /// Sides figures are initially tagged with the first letter of the side name.    
    /// </summary>
    /// <seealso cref="GLFigureBase.Tag"/>.
    public IEnumerable<GLTriangleFigure> Sides
    {
        get
        {
            yield return SideBottom;
            yield return SideTop;

            yield return SideLeft;
            yield return SideRight;

            yield return SideFront;
            yield return SideBack;
        }
    }

    /// <summary>
    /// Create a box figure.
    /// </summary>
    /// <param name="cs">Coordinate system for the box construction.</param>
    /// <param name="csSize">Size of the box.</param>
    /// <param name="color">Color of box sides.</param>
    /// <param name="csBaseIsMiddle">If true <see cref="CS"/> represents the middle of the box.<br/>
    /// If false it represents the cs placed in the middle of lower face.</param>
    /// <param name="setTextureST">If true textureST coordinates will applied to box sides triangles.</param>
    /// <seealso cref="SearchAThing.OpenGL.Core.Ext.GetTriangles"/>.
    public Box(in Matrix4x4 cs, in Vector3 csSize, Color? color = null,
        bool csBaseIsMiddle = false,
        bool setTextureST = false)
    {
        CSBaseIsMiddle = csBaseIsMiddle;

        if (CSBaseIsMiddle)
        {
            CS = cs.Move(-Vector3.UnitZ * csSize.Z / 2);
            CSSize = csSize;
            Center = cs.Origin();
        }
        else
        {
            CS = cs;
            CSSize = csSize;
            Center = CS.Origin() + CS.BaseZ() * Elevation / 2;
        }

        var origin = CS.Origin();
        var basex = CS.BaseX();
        var basey = CS.BaseY();
        var basez = CS.BaseZ();

        var halfCSXSize = CSSize.X / 2;
        var halfCSYSize = CSSize.Y / 2;

        var boxCenter = Center;

        var plateBase = new Plate(
            origin - basex * halfCSXSize - basey * halfCSYSize,
            origin + basex * halfCSXSize - basey * halfCSYSize,
            origin + basex * halfCSXSize + basey * halfCSYSize,
            origin - basex * halfCSXSize + basey * halfCSYSize);

        SideBottom = new GLTriangleFigure(plateBase.Invert().GetTriangles(color, setTextureST));
        SideBottom.Tag = "O"; // TODO: figure property

        SideTop = new GLTriangleFigure(plateBase.Move(basez * Elevation).GetTriangles(color, setTextureST));
        SideTop.Tag = "T";

        var plateLeft = new Plate(
            origin - basex * halfCSXSize + basey * halfCSYSize,
            origin - basex * halfCSXSize - basey * halfCSYSize,
            origin - basex * halfCSXSize - basey * halfCSYSize + basez * Elevation,
            origin - basex * halfCSXSize + basey * halfCSYSize + basez * Elevation);

        SideLeft = new GLTriangleFigure(plateLeft.GetTriangles(color, setTextureST));
        SideLeft.Tag = "L";

        SideRight = new GLTriangleFigure(plateLeft.Invert().Move(basex * CSSize.X).GetTriangles(color, setTextureST));
        SideRight.Tag = "R";

        var plateFront = new Plate(
            origin - basex * halfCSXSize - basey * halfCSYSize,
            origin + basex * halfCSXSize - basey * halfCSYSize,
            origin + basex * halfCSXSize - basey * halfCSYSize + basez * Elevation,
            origin - basex * halfCSXSize - basey * halfCSYSize + basez * Elevation);

        SideFront = new GLTriangleFigure(plateFront.GetTriangles(color, setTextureST));
        SideFront.Tag = "F";

        SideBack = new GLTriangleFigure(plateFront.Invert().Move(basey * CSSize.Y).GetTriangles(color, setTextureST));
        SideBack.Tag = "B";
    }

}
