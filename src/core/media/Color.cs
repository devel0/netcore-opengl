namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// Convert given vector4 (red, green, blue, alpha) color to <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="rgbaColor">Color encoded in vector4 (range 0..1) as (red, green, blue, alpha).<br/>Alpha=1 (full opaque).<br/>Alpha=0 (full transparent).</param>    
    public static Color ToColor(this in Vector4 rgbaColor) =>
       Color.FromArgb((int)(rgbaColor.W * 255), (int)(rgbaColor.X * 255), (int)(rgbaColor.Y * 255), (int)(rgbaColor.Z * 255));

    /// <summary>
    /// Convert given <see cref="System.Drawing.Color"/> to vector4 (red, green, blue, alpha).
    /// </summary>
    /// <param name="color">Color to convert.</param>
    /// <param name="alpha">(optional) Alpha override.<br/>Alpha=1 (full opaque).<br/>Alpha=0 (full transparent).<br/>If not specified given color alpha will taken.</param>
    public static Vector4 ToVector4(this in Color color, float? alpha = null) => new Vector4(
        (float)color.R / 255f,
        (float)color.G / 255f,
        (float)color.B / 255f,
        alpha.HasValue ? alpha.Value : (float)color.A / 255f);

    /// <summary>
    /// Convert given <see cref="System.Drawing.Color"/> to vector3 (red, green, blue) discarding alpha information.
    /// </summary>
    /// <param name="color">Color to convert.</param>    
    public static Vector3 ToVector3(this in Color color) => new Vector3(
        (float)color.R / 255f,
        (float)color.G / 255f,
        (float)color.B / 255f);

}