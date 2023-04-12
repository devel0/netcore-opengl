using SkiaSharp.HarfBuzz;

namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Gl font char manager, allocated through gl context.<br/>
/// It manage the cache of <see cref="GLFontChar"/> avoid there are duplicates of the same gl font char for the same font/character.
/// </summary>
public class GLFontCharManager
{

    public GLContext GLContext { get; private set; }

    /// <summary>
    /// Instantiate gl font char manager.
    /// </summary>
    /// <param name="glContext">Gl context.</param>
    public GLFontCharManager(GLContext glContext)
    {
        this.GLContext = glContext;
    }

    static string FontCharKey(string fontName, char c) => $"{fontName}_{c}";

    Dictionary<string, GLFontChar> font_char_to_nfo = new Dictionary<string, GLFontChar>();
    object lck_font_char_to_nfo = new object();

    /// <summary>
    /// Retrieve the list of <see cref="GLFontChar"/> objects for given text and font.
    /// </summary>
    /// <param name="text">Text string for which retrieve gl font chars.</param>
    /// <param name="font">Font to use in retrieval of gl font chars.</param>    
    /// <seealso cref="Ext.Describe"/>
    public List<GLFontChar> GetFontChars(string text, SKFont font)
    {
        var res = new List<GLFontChar>();
        var textBounds = new SKRect();

        GLFontChar? fcn = null;
        SKPaint? paint = null;
        SKShaper? shaper = null;

        // workaround: example-0017 jumping chars        
        float SMALL_LIMIT = FONT_BITMAP_PIXEL / 60f;

        for (int i = 0; i < text.Length; ++i)
        {
            var c = text[i];

            var c_key = FontCharKey(font.Typeface.FamilyName, c);

            if (!font_char_to_nfo.TryGetValue(c_key, out fcn))
            {
                if (paint is null)
                {
                    paint = new SKPaint
                    {
                        IsAntialias = true,
                        TextSize = FONT_BITMAP_PIXEL,
                        Typeface = font.Typeface,
                        Color = SKColors.White,
                        FilterQuality = SKFilterQuality.High
                    };
                }

                if (shaper is null)
                {
                    shaper = new SKShaper(font.Typeface);
                }

                var measure = paint.MeasureText($"{c}", ref textBounds);
                var w = textBounds.Width;
                var h = textBounds.Height;

                if (w == 0) w = measure;

                var bitmap = new SKBitmap((int)Round(w), (int)Round(h));
                {
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        canvas.Clear(SKColors.Transparent);

                        canvas.DrawShapedText(shaper, $"{c}",
                            x: -textBounds.Left, y: -textBounds.Top,
                            paint);

                        canvas.Flush();
                    }

                    if (Abs(textBounds.Bottom) < SMALL_LIMIT)
                        textBounds.Bottom = 0;

                    fcn = new GLFontChar(GLContext, c, measure, textBounds, bitmap, font);
                    font_char_to_nfo.Add(c_key, fcn);
                }

                // uncomment follow to save png of character
                // char? dbgChar = null;
                // dbgChar = '-';
                // if (dbgChar is not null && c == dbgChar.Value)
                //     bitmap.SaveTo(
                //         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "char.png"));
            }

            res.Add(fcn);
        }

        // uncomment follow to generate fonts.xlsx table on desktop
        // font_char_to_nfo.Values.Describe(
        //     Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "font.xlsx"));

        if (shaper is not null) shaper.Dispose();
        if (paint is not null) paint.Dispose();

        return res;
    }

}