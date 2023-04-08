namespace SearchAThing.OpenGL.Shapes;

public static partial class Toolkit
{

    /// <summary>
    /// make rectangle by specify center, width, height
    /// </summary>    
    /// 

    /// <summary>
    /// Make a rectangle figure with center in given cs with extensions as cs.basex * w and cs.basey * h.    
    /// </summary>
    /// <param name="csCenter">Coordinate system where rectangle resides. Center of rectangle is the cs origin.</param>
    /// <param name="w">Width of rectangle.</param>
    /// <param name="h">Height of rectangle.</param>
    /// <param name="color">Color.</param>
    /// <returns>Rectangle line figure.</returns>
    public static GLLineFigure MakeRectangle(in Matrix4x4 csCenter, float w, float h,
        Color? color = null)
    {
        var center = csCenter.Origin();
        var baseX = csCenter.BaseX();
        var baseY = csCenter.BaseY();

        var lines = new List<GLLine>();

        var p = center - baseX * w / 2 - baseY * h / 2;

        lines.Add(Line.FromTo(p, p += baseX * w).ToGLLine(color));
        lines.Add(Line.FromTo(p, p += baseY * w).ToGLLine(color));
        lines.Add(Line.FromTo(p, p -= baseX * w).ToGLLine(color));
        lines.Add(Line.FromTo(p, p -= baseY * w).ToGLLine(color));

        return new GLLineFigure(lines);
    }

}