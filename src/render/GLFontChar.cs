namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Holds a single font character bitmap and allocated texture.
/// </summary>
/// <seealso cref="GLFontCharManager"/>
public class GLFontChar
{

    /// <summary>
    /// Font character code.
    /// </summary>    
    public char c { get; private set; }

    /// <summary>
    /// Width of the (virtual) bitmap.
    /// </summary>    
    public float Measure { get; private set; }

    /// <summary>
    /// Bounds of the character font rendered at <see cref="FONT_BITMAP_PIXEL"/> pixels.
    /// </summary>    
    public SKRect TextBounds { get; private set; }

    /// <summary>
    /// Bitmap that hold the White on transparent background image of the character.
    /// </summary>    
    public SKBitmap Bitmap { get; private set; }

    /// <summary>
    /// Texture allocated at constructor using given gl context for the character bitmap.
    /// </summary>    
    public IGLTexture2D? Texture { get; private set; }

    /// <summary>
    /// Pointer to the font associated.
    /// </summary>    
    public SKFont Font { get; private set; }

    /// <summary>
    /// Construct a character bitmap and texture object.
    /// </summary>
    /// <param name="glContext">Gl context.</param>
    /// <param name="c">Font character code.</param>
    /// <param name="measure">Width of the (virtual) bitmap.<br/>
    /// This value is computed through SKPaint.MeasureText(), its used by <see cref="SearchAThing.OpenGL.Render.Toolkit.MakeTextFigure"/>
    /// for empty characters such as white space where TextBounds gives no useful information and bitmap is empty.</param>
    /// <param name="textBounds">Bounds of the character font rendered at <see cref="FONT_BITMAP_PIXEL"/> pixels.</param>
    /// <param name="bitmap">Bitmap that hold the White on transparent background image of the character.</param>
    /// <param name="font">Pointer to the font associated.</param>
    public GLFontChar(GLContext glContext,
        char c,
        float measure,
        SKRect textBounds,
        SKBitmap bitmap,
        SKFont font)
    {
        this.c = c;

        this.TextBounds = textBounds;
        this.Font = font;
        this.Measure = measure;

        this.Bitmap = bitmap;

        if (bitmap.Width > 0 && bitmap.Height > 0)
            Texture = new GLTexture2D(glContext, bitmap);
    }

    public override string ToString() => $"Char[{c}]";
}

