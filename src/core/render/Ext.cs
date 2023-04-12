using ClosedXML.Excel;

namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    #region screen

    /// <summary>
    /// Convert given screen size (width, height) to a vector2 (x, y).
    /// </summary>    
    public static Vector2 ToVector2(this in Size size) => new Vector2(size.Width, size.Height);    

    #endregion

    #region colors 

    /// <summary>
    /// Set alpha channel<br/>
    /// Alpha=1 (full opaque).<br/>
    /// Alpha=0 (full transparent).
    /// </summary>    
    public static Color SetAlphaF(this in Color color, float alpha) =>
        Color.FromArgb((int)(alpha * 255), color.R, color.G, color.B);

    #endregion    

    #region bitmap

    /// <summary>
    /// Helper method to save skia bitmap to output image file.
    /// </summary>    
    public static void SaveTo(this SKBitmap bitmap, string outputPathfilename,
        SKEncodedImageFormat format = SKEncodedImageFormat.Png,
        int quality = 80)
    {
        using (var skImage = SKImage.FromBitmap(bitmap))
        using (var skData = skImage.Encode(format, quality))
        using (var stream = File.OpenWrite(outputPathfilename))
        {
            skData.SaveTo(stream);
        }
    }

    #endregion

    #region font chars

    /// <summary>
    /// Debugging of <see cref="GLFontChar"/> to xlsx.<br/>
    /// Used in <see cref="GLFontCharManager.GetFontChars"/>.
    /// </summary>    
    public static void Describe(this IEnumerable<GLFontChar> fontChars, string xlsxPathfilename)
    {
        SetupClosedXMLFonts();    

        using (var wb = new XLWorkbook())
        {
            var ws = wb.AddWorksheet();

            var r = 1;
            var c = 1;

            IXLCell cell;

            void setBold() => cell.Style.Font.SetBold();
            void alignCenter() => cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            cell = ws.Cell(r, c); cell.Value = "Idx"; setBold(); var colIdx = c++;
            cell = ws.Cell(r, c); cell.Value = "Char"; setBold(); var colChar = c++;

            cell = ws.Cell(r, c); cell.Value = "Advance"; setBold(); var colAdvance = c++;

            cell = ws.Cell(r, c); cell.Value = "TB.Width"; setBold(); var colTB_Width = c++;
            cell = ws.Cell(r, c); cell.Value = "TB.Height"; setBold(); var colTB_Height = c++;
            cell = ws.Cell(r, c); cell.Value = "TB.MidX"; setBold(); var colTB_MidX = c++;
            cell = ws.Cell(r, c); cell.Value = "TB.MidY"; setBold(); var colTB_MidY = c++;

            cell = ws.Cell(r, c); cell.Value = "TB.Left"; setBold(); var colTB_Left = c++;
            cell = ws.Cell(r, c); cell.Value = "TB.Top"; setBold(); var colTB_Top = c++;
            cell = ws.Cell(r, c); cell.Value = "TB.Right"; setBold(); var colTB_Right = c++;
            cell = ws.Cell(r, c); cell.Value = "TB.Bottom"; setBold(); var colTB_Bottom = c++;

            var idx = 0;
            ++r;
            foreach (var fc in fontChars)
            {
                cell = ws.Cell(r, colIdx); cell.Value = idx;
                cell = ws.Cell(r, colChar); cell.Value = $"{fc.c}"; alignCenter();

                cell = ws.Cell(r, colAdvance); cell.Value = fc.Measure;

                cell = ws.Cell(r, colTB_Bottom); cell.Value = fc.TextBounds.Bottom;
                cell = ws.Cell(r, colTB_Height); cell.Value = fc.TextBounds.Height;
                cell = ws.Cell(r, colTB_Left); cell.Value = fc.TextBounds.Left;
                cell = ws.Cell(r, colTB_MidX); cell.Value = fc.TextBounds.MidX;
                cell = ws.Cell(r, colTB_MidY); cell.Value = fc.TextBounds.MidY;
                cell = ws.Cell(r, colTB_Right); cell.Value = fc.TextBounds.Right;
                cell = ws.Cell(r, colTB_Top); cell.Value = fc.TextBounds.Top;
                cell = ws.Cell(r, colTB_Width); cell.Value = fc.TextBounds.Width;

                ++idx; ++r;
            }

            wb.SaveAs(xlsxPathfilename);
        }
    }

    #endregion

    #region GL

    /// <summary>
    /// Retrieve corresponding Silk.NET gl type from netcore-opengl <see cref="GLPrimitiveType"/>.
    /// </summary>    
    public static PrimitiveType GL(this GLPrimitiveType type) => type switch
    {
        GLPrimitiveType.Point => PrimitiveType.Points,
        GLPrimitiveType.Line => PrimitiveType.Lines,
        GLPrimitiveType.Triangle => PrimitiveType.Triangles,
        _ => throw new NotImplementedException($"gl primitive {type}")
    };

    #endregion       

}
