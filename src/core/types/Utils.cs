namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// Convert given double to float ensuring not exceed the float range.
    /// </summary>
    /// <param name="d">Double which convert to float.</param>
    /// <returns>Float of the given double.</returns>
    public static float ToFloat(this double d)
    {
        if (d < float.MinValue || d > float.MaxValue)
            throw new Exception($"double [{d}] out of bounds the float range [{float.MinValue},{float.MaxValue}]");

        return (float)d;
    }

    /// <summary>
    /// Invariant representation X Y Z
    /// </summary>
    public static string Fmt(this in Vector3 v) => $"{v.X} {v.Y} {v.Z}";

    /// <summary>
    /// Retrieve ColorHelper RGB color from given system drawing color.
    /// </summary>    
    public static ColorHelper.RGB ToRGB(this Color color) => new ColorHelper.RGB(color.R, color.G, color.B);

    /// <summary>
    /// Retrieve ColorHelper HSV color from given system drawing color.
    /// </summary>    
    public static ColorHelper.HSV ToHSV(this Color color) => ColorHelper.ColorConverter.RgbToHsv(color.ToRGB());

    /// <summary>
    /// Retrieve ColorHelper RGB color from given ColorHelper HSV color.
    /// </summary>    
    public static ColorHelper.RGB ToRGB(this ColorHelper.HSV hsv) => ColorConverterPatch.HsvToRgb(hsv);

    /// <summary>
    /// Retrieve system drawing color from given ColorHelper RGB color.
    /// </summary>    
    public static Color ToColor(this ColorHelper.RGB rgb) => Color.FromArgb(rgb.R, rgb.G, rgb.B);

    /// <summary>
    /// Retrieve system drawing color from given ColorHelper HSV color.
    /// </summary>    
    public static Color ToColor(this ColorHelper.HSV hsv) => ColorConverterPatch.HsvToRgb(hsv).ToColor();

}

public static partial class Toolkit
{

    /// <summary>
    /// Retrieve color that is in the given offset in given hue range.<br/>
    /// Default: hue range [240, 300] generating gradients in offeset range [0, 1] traversing following colors (
    /// [blue, cyan, green, yellow, orange, red, fuchsia].<br/>
    /// <br/>
    /// Following some colors hue values:<br/>
    /// - 240 = Blue (0,0,255)<br/>
    /// - 180 = Cyan (0,255,255)<br/>
    /// - 120 = Green (0,255,0)<br/>
    /// - 60 = Yellow (255,255,0)<br/>
    /// - 30 = Orange (255,128,0)<br/>
    /// - 0 = Red (255,0,0)<br/>
    /// - 300 = Fuchsia (255,0,255)<br/>
    /// - 270 = Violet (128,0,255)<br/>    
    /// ref: https://en.wikipedia.org/wiki/HSL_and_HSV        
    /// </summary>       
    /// <param name="offset">Offset in range [0,1] of the color to choose in given hue range.</param>
    /// <param name="hueFrom">Hue of color range from.</param>
    /// <param name="hueTo">Hue of color range to.</param>
    /// <param name="decreaseHue">If true (default) offset traverse hue range decreasing hue starting by "from" toward "to". Otherwise "to" hue will reached by the "from" through increment of hue.</param>
    /// <returns>Color corresponding given range offset.</returns>
    public static Color GetContourColor(float offset, int hueFrom = 240, int hueTo = 300, bool decreaseHue = true)
    {

        // default  [6]                     [0]       [1]         [2]       [3]         [4]        [5]
        //       <------------------------------------------------------------------------------------------- HUE
        // color    Fuchsia     Violet      Blue      Cyan        Green     Yellow      Orange      Red
        // rgb      255.0.255   128.0.255   0.0.255   0.255.255   0.255.0   255.255.0   255.128.0   255.0.0
        // hue      300         270         240       180         120       60          30          0

        int h;

        if (decreaseHue)
        {
            int dh;

            if (hueTo < hueFrom)
                dh = hueFrom - hueTo;

            else
                dh = (360 - hueTo) + hueFrom;

            h = hueFrom - (int)(offset * dh);
            if (h < 0) h += 360;
        }
        else
        {
            int dh;

            if (hueTo > hueFrom)
                dh = hueTo - hueFrom;

            else
                dh = (360 - hueFrom) + hueTo;

            h = (hueFrom + (int)(offset * dh)) % 360;
        }

        var res = (new ColorHelper.HSV(h, 100, 100)).ToColor();

        return res;
    }

}