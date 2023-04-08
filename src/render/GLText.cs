namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Gl text object.
/// </summary>
public class GLText : INotifyPropertyChanged
{

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region CS

    private Matrix4x4 _CS;

    /// <summary>
    /// Coordinate system which origin represents the text insertion point and XY plane where text resides.
    /// </summary>    
    public Matrix4x4 CS
    {
        get => _CS;
        set
        {
            var changed = value != _CS;
            if (changed)
            {
                _CS = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Text

    private string _Text;

    /// <summary>
    /// Text string.<br/>
    /// Supports multiline with '\n' newline character.
    /// </summary>    
    public string Text
    {
        get => _Text;
        set
        {
            var changed = value != _Text;
            if (changed)
            {
                _Text = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Height

    private float _Height = 1;

    /// <summary>
    /// Text height.
    /// </summary>
    public float Height
    {
        get => _Height;
        set
        {
            var changed = value != _Height;
            if (changed)
            {
                _Height = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion       

    #region Alignment

    private GLTextVHAlignment _Alignment = GLTextVHAlignment.BottomLeft;

    /// <summary>
    /// Text alignment.
    /// </summary>
    /// <seealso cref="GLTextVHAlignment"/>.
    public GLTextVHAlignment Alignment
    {
        get => _Alignment;
        set
        {
            var changed = value != _Alignment;
            if (changed)
            {
                _Alignment = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region BuiltinFontPathfilename
    static string? _BuiltinFontPathfilename = null;

    /// <summary>
    /// Builtin font pathfilename.
    /// </summary>
    /// <seealso cref="BUILTIN_FONTFILE"/>    
    public static string BuiltinFontPathfilename
    {
        get
        {
            // 

            if (_BuiltinFontPathfilename is null)
            {
                var fontPathname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fonts");
                var fontPathfilename = Path.Combine(fontPathname, BUILTIN_FONTFILE);

                if (!File.Exists(fontPathfilename))
                {
                    var resourceName = $"{RESOURCE_NAMESPACE_BASE}.fonts.{BUILTIN_FONTFILE}";

                    var buf = GetEmbeddedResourceNames(GLContext.netcore_opengl_render_assembly)
                        .First(w => w.Contains(resourceName))
                        .Fn(name => GetEmbeddedFileContentBytes(GLContext.netcore_opengl_render_assembly, name));

                    if (buf is null) throw new Exception($"can't read embedded resource [{resourceName}] from assembly [{GLContext.netcore_opengl_render_assembly.FullName}]");

                    if (!Directory.Exists(fontPathname)) Directory.CreateDirectory(fontPathname);

                    File.WriteAllBytes(fontPathfilename, buf);
                }

                _BuiltinFontPathfilename = fontPathfilename;
            }

            return _BuiltinFontPathfilename;
        }
    }
    #endregion

    #region Font

    private SKFont? _Font = null;

    /// <summary>
    /// Skia font object with this font typeface.
    /// </summary>    
    public SKFont Font
    {
        get
        {
            if (_Font is null)
            {
                using (var tf = SKTypeface.FromFile(BuiltinFontPathfilename))
                {
                    _Font = new SKFont(tf);
                }
            }
            return _Font;
        }
        set
        {
            var changed = value != _Font;
            if (changed)
            {
                _Font = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Color

    private Color _Color = DEFAULT_TextColor;

    /// <summary>
    /// Text color.
    /// </summary>
    public Color Color
    {
        get => _Color;
        set
        {
            var changed = value != _Color;
            if (changed)
            {
                _Color = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region SpacingBetweenLines

    private float _SpacingBetweenLines = 1;

    /// <summary>
    /// Space between lines (em).
    /// </summary>
    public float SpacingBetweenLines
    {
        get => _SpacingBetweenLines;
        set
        {
            var changed = value != _SpacingBetweenLines;
            if (changed)
            {
                _SpacingBetweenLines = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Create a gl text.
    /// </summary>
    /// <param name="cs">Coordinate system which origin represents the text insertion point and XY plane where text resides.
    /// Use <see cref="MakeScreenCS"/> for screenMode text.</param>
    /// <param name="text">Text string.<br/>Supports multiline with '\n' newline character.</param>
    /// <param name="height">Text height.</param>
    /// <param name="color">Text color.</param>
    /// <param name="alignment">Text alignment.</param>    
    public GLText(
        in Matrix4x4 cs,
        string text,
        float? height = null,
        Color? color = null,
        GLTextVHAlignment? alignment = null)
    {
        _CS = cs;
        _Text = text;

        if (height is not null) Height = height.Value;

        if (color is not null) Color = color.Value;

        if (alignment is not null) Alignment = alignment.Value;
    }

}