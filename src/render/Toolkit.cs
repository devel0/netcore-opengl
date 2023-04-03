using System.Runtime.InteropServices;

namespace SearchAThing.OpenGL.Render;

public static partial class Toolkit
{

    /// <summary>
    /// Helper to overcome ClosedXML font issue when adjust columns contents of an xlsx document.
    /// </summary>
    public static void SetupClosedXMLFonts()
    {
        string? fontName = null;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            fontName = BUILTIN_FONTNAME;

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            fontName = "Verdana";

        if (fontName is not null)
            ClosedXML.Excel.LoadOptions.DefaultGraphicEngine = new ClosedXML.Graphics.DefaultGraphicEngine(fontName);
    }

}