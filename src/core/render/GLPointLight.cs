namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Gl point light.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class GLPointLight : INotifyPropertyChanged
{

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Active

    private bool _Active = true;

    /// <summary>
    /// If false light don't drop any shadow nor affects vertex colours.
    /// </summary>    
    [JsonProperty]
    public bool Active
    {
        get => _Active;
        set
        {
            var changed = value != _Active;
            if (changed)
            {
                _Active = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Position

    private Vector3 _Position = Vector3.Zero;

    /// <summary>
    /// Position [local].
    /// </summary>
    [JsonProperty]
    public Vector3 Position
    {
        get => _Position;
        set
        {
            var changed = value != _Position;
            if (changed)
            {
                _Position = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Ambient

    private Color _Ambient;
    /// <summary>
    /// Ambient light color (Default:White).
    /// </summary>
    [JsonProperty]
    public Color Ambient
    {
        get => _Ambient;
        set
        {
            var changed = value != _Ambient;
            if (changed)
            {
                _Ambient = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Diffuse

    private Color _Diffuse;
    /// <summary>
    /// Diffuse light color (Default:white).
    /// </summary>
    [JsonProperty]
    public Color Diffuse
    {
        get => _Diffuse;
        set
        {
            var changed = value != _Diffuse;
            if (changed)
            {
                _Diffuse = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Specular

    private Color _Specular;
    /// <summary>
    /// Specular light color (Default:white).
    /// </summary>
    [JsonProperty]
    public Color Specular
    {
        get => _Specular;
        set
        {
            var changed = value != _Specular;
            if (changed)
            {
                _Specular = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Constant

    private float _Constant = DEFAULT_LIGHT_CONSTANT;
    /// <summary>
    /// Light attenuation constant factor.
    /// </summary>
    [JsonProperty]
    public float Constant
    {
        get => _Constant;
        set
        {
            var changed = value != _Constant;
            if (changed)
            {
                _Constant = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Linear

    private float _Linear = DEFAULT_LIGHT_LINEAR;
    /// <summary>
    /// Light attenuation linear factor.
    /// </summary>
    [JsonProperty]
    public float Linear
    {
        get => _Linear;
        set
        {
            var changed = value != _Linear;
            if (changed)
            {
                _Linear = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Quadratic

    private float _Quadratic = DEFAULT_LIGHT_QUADRATIC;
    /// <summary>
    /// Light attenuation quadratica factor.
    /// </summary>
    [JsonProperty]
    public float Quadratic
    {
        get => _Quadratic;
        set
        {
            var changed = value != _Quadratic;
            if (changed)
            {
                _Quadratic = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    //

    #region ShowPoint

    private bool _ShowPoint;
    /// <summary>
    /// If true a Gl point will be shown where the light is positioned.<br/>
    /// Colored yellow if light is active, darkgray if inactive.
    /// </summary>
    [JsonProperty]
    public bool ShowPoint
    {
        get => _ShowPoint;
        set
        {
            var changed = value != _ShowPoint;
            if (changed)
            {
                _ShowPoint = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    GLPointLight()
    {
    }

    /// <summary>
    /// Create a gl point light at given coordinates [local].
    /// </summary>
    /// <param name="x">X coordinate [local].</param>
    /// <param name="y">Y coordinate [local].</param>
    /// <param name="z">Z coordinate [local].</param>
    /// <param name="color">Light color (ambient, diffuse, specular).</param>
    /// <param name="showPoint">If true a gl point will displayed where light is.</param>    
    public GLPointLight(float x, float y, float z, in Color? color = null, bool showPoint = DEFAULT_LIGHT_SHOW_POINT) :
        this(new Vector3(x, y, z), color, showPoint)
    {
    }

    /// <summary>
    /// Create a gl point light at given coordinates [local].
    /// </summary>
    /// <param name="pos">Light coordinate [local].</param>
    /// <param name="color">Light color (ambient, diffuse, specular).</param>
    /// <param name="showPoint">If true a gl point will displayed where light is.</param>    
    public GLPointLight(Vector3? pos = null, Color? color = null, bool showPoint = DEFAULT_LIGHT_SHOW_POINT)
    {
        if (pos is null) pos = Vector3.Zero;
        if (color is null) color = DEFAULT_LIGHT_COLOR;

        _Position = pos.Value;
        Ambient = Diffuse = Specular = color.Value;
        ShowPoint = showPoint;
    }

    internal GLPointLightStruct ToStruct(Matrix4x4 modelMatrix) => new GLPointLightStruct
    {
        position = Vector3.Transform((Vector3)Position, modelMatrix),
        ambient = Ambient.ToVector3(),
        diffuse = Diffuse.ToVector3(),
        specular = Specular.ToVector3(),
        constant = Constant,
        linear = Linear,
        quadratic = Quadratic
    };

    /// <summary>
    /// Setup light attenuation factor based on model bbox size.
    /// </summary>
    /// <param name="model">Gl model.</param>
    /// <param name="adjustConstant">Light constant attenuation factor.</param>
    /// <param name="adjustLinear">Light linear attenuation factor.</param>
    /// <param name="adjustQuadratic">Light quadratic attenuation factor.</param>
    public void SetupAttenuation(GLModel model,
        float? adjustConstant = DEFAULT_LIGHT_CONSTANT,
        float? adjustLinear = DEFAULT_LIGHT_LINEAR,
        float? adjustQuadratic = DEFAULT_LIGHT_QUADRATIC)
    {
        var s = model.LBBox.Size.Max();

        var C = adjustConstant.HasValue ? adjustConstant.Value : 1;
        var L = adjustLinear.HasValue ? adjustLinear.Value / s : 0;
        var Q = adjustQuadratic.HasValue ? adjustQuadratic.Value / s : 0;

        foreach (var light in model.PointLights)
        {
            light.Constant = C;
            light.Linear = L;
            light.Quadratic = Q;
        }
    }

}