namespace SearchAThing.OpenGL.Core;

// https://github.com/iamartyom/ColorHelper/pull/17

public static partial class ColorConverterPatch
{

    private static double GetHue(double p, double q, double t)
    {
        double value = p;

        if (t < 0) t++;
        if (t > 1) t--;

        if (t < 1.0 / 6)
        {
            value = p + (q - p) * 6 * t;
        }
        else if (t < 1.0 / 2)
        {
            value = q;
        }
        else if (t < 2.0 / 3)
        {
            value = p + (q - p) * (2.0 / 3 - t) * 6;
        }

        return value;
    }

    public static ColorHelper.RGB HsvToRgb(ColorHelper.HSV hsv)
    {
        return HslToRgb(ColorHelper.ColorConverter.HsvToHsl(hsv));
    }

    public static ColorHelper.RGB HslToRgb(ColorHelper.HSL hsl)
    {
        double modifiedH, modifiedS, modifiedL, r = 1, g = 1, b = 1, q, p;

        modifiedH = hsl.H / 360.0;
        modifiedS = hsl.S / 100.0;
        modifiedL = hsl.L / 100.0;

        q = (modifiedL < 0.5) ? modifiedL * (1 + modifiedS) : modifiedL + modifiedS - modifiedL * modifiedS;
        p = 2 * modifiedL - q;

        if (modifiedL == 0)  // if the lightness value is 0 it will always be black
        {
            r = 0;
            g = 0;
            b = 0;
        }
        else if (modifiedS != 0)
        {
            r = GetHue(p, q, modifiedH + 1.0 / 3);
            g = GetHue(p, q, modifiedH);
            b = GetHue(p, q, modifiedH - 1.0 / 3);
        }
        else // ensure greys are not converted to white
        {
            r = modifiedL;
            g = modifiedL;
            b = modifiedL;
        }

        return new ColorHelper.RGB(
            (byte)Math.Round(Math.Round(r * 255, 1)),
            (byte)Math.Round(Math.Round(g * 255, 1)),
            (byte)Math.Round(Math.Round(b * 255, 1)));
    }

}