namespace SearchAThing.OpenGL.Core;

public static partial class Constants
{

    #region system

    /// <summary>
    /// Environment variable to set in order to use mesa library in Windows.<br/>
    /// <seealso cref="GLContext"/>
    /// </summary>
    public const string MESA_WIN_LIBRARY_ENV = "OPENGL_LIBRARY_PATH";

    #endregion

    #region assembly resources

    /// <summary>
    /// SearchAThing.OpenGL.Core assembly name (regex).
    /// </summary>
    public const string NETCORE_OPENGL_CORE_ASSEMBLY_NAME = "netcore-opengl-core";

    /// <summary>
    /// SearchAThing.OpenGL.Core (regex).
    /// </summary>
    public static readonly string RESOURCE_NAMESPACE_BASE =
        Regex.Escape("SearchAThing.OpenGL.Core") + "(?<my>(\\.render)?)(?<my>(\\.shaders)?)";

    /// <summary>
    /// Shader "1.depth.vs" resource name (regex).
    /// </summary>
    /// <value></value>
    public static readonly string SHADER_RESOURCE_DEPTH_VERT =
        RESOURCE_NAMESPACE_BASE + Regex.Escape(".1.depth.vs");

    /// <summary>
    /// Shader "2.depth.gs" resource name (regex).
    /// </summary>
    /// <value></value>
    public static readonly string SHADER_RESOURCE_DEPTH_GEOM =
        RESOURCE_NAMESPACE_BASE + Regex.Escape(".2.depth.gs");

    /// <summary>
    /// Shader "3.depth.fs" resource name (regex).
    /// </summary>
    /// <value></value>
    public static readonly string SHADER_RESOURCE_DEPTH_FRAG =
        RESOURCE_NAMESPACE_BASE + Regex.Escape(".3.depth.fs");

    /// <summary>
    /// Shader "4.main.vs" resource name (regex).
    /// </summary>
    /// <value></value>
    public static readonly string SHADER_RESOURCE_MAIN_VERT =
        RESOURCE_NAMESPACE_BASE + Regex.Escape(".4.main.vs");

    /// <summary>
    /// Shader "5.shadewithedge.gs" resource name (regex).
    /// </summary>
    /// <value></value>
    public static readonly string SHADER_RESOURCE_MAIN_GEOM_SHADE_WITH_EDGE =
        RESOURCE_NAMESPACE_BASE + Regex.Escape(".5.shadewithedge.gs");

    /// <summary>
    /// Shader "5.normal.gs" resource name (regex).
    /// </summary>
    /// <value></value>
    public static readonly string SHADER_RESOURCE_MAIN_GEOM_NORMAL =
        RESOURCE_NAMESPACE_BASE + Regex.Escape(".5.normal.gs");

    /// <summary>
    /// Shader "6.main.fs" resource name (regex).
    /// </summary>
    /// <value></value>
    public static readonly string SHADER_RESOURCE_MAIN_FRAG =
        RESOURCE_NAMESPACE_BASE + Regex.Escape(".6.main.fs");

    #endregion

    #region gl shader locations

    /// <summary>
    /// Name of vertex shader attribute ( vertex position [local] ).
    /// </summary>
    public const string ATTNAME_vPos = "vPos";

    /// <summary>
    /// Name of vertex shader attribute ( vertex normal [local] ).
    /// </summary>
    public const string ATTNAME_vNormal = "vNormal";

    /// <summary>
    /// Name of vertex shader attribute ( rgba vertex color ).
    /// </summary>
    public const string ATTNAME_vMatColor = "vMatColor";

    /// <summary>
    /// Name of vertex shader attribute ( Describe the strength (0..1) of the ambient, diffuse, specular light effects. ).
    /// </summary>
    public const string ATTNAME_vMatProp = "vMatProp";

    /// <summary>
    /// Name of vertex shader attribute ( texture coordinate mapping in the range [0,0]...[1,1] ). 
    /// </summary>
    public const string ATTNAME_vTexCoord = "vTexCoord";

    /// <summary>
    /// Name of vertex shader attribute ( flags ).
    /// </summary>
    public const string ATTNAME_vFlags = "vFlags";

    /// <summary>
    /// Name of the uniform shader variable that describe the index of the light to evaluate. ( used in depth shader ).
    /// </summary>
    public const string UNINAME_uLightIdx = "uLightIdx";

    /// <summary>
    /// Name of the uniform shader variable for the object matrix [object].
    /// </summary>
    public const string UNINAME_uObject = "uObject";

    /// <summary>
    /// Name of the uniform shader variable for the model matrix [local].
    /// </summary>
    public const string UNINAME_uModel = "uModel";

    /// <summary>
    /// Name of the uniform shader variable for the view matrix [world].
    /// </summary>
    public const string UNINAME_uView = "uView";

    /// <summary>
    /// Name of the uniform shader variable for the projection matrix [eye].
    /// </summary>
    public const string UNINAME_uProjection = "uProjection";

    /// <summary>
    /// Name of the uniform shader variable that enable screen coord mode vertex evaluation [object].
    /// </summary>
    public const string UNINAME_uFigureScreenCoord = "uFigureScreenCoord";

    /// <summary>
    /// Name of the uniform shader variable for the screen mode projection matrix [eye].
    /// </summary>
    public const string UNINAME_uScreenProjection = "uScreenProjection";

    /// <summary>
    /// Name of the uniform shader variable for the screen mode model matrix [local].
    /// </summary>
    public const string UNINAME_uScreenModel = "uScreenModel";

    /// <summary>
    /// Name of the uniform shader variable for the camera position [world].
    /// </summary>
    public const string UNINAME_uCameraPos = "uCameraPos";

    /// <summary>
    /// Name of the uniform shader variable for the model bbox min vertex [local].<br/>
    /// Used by normal geometry shader to compute normal scaled length.
    /// </summary>
    public const string UNINAME_uBBoxMin = "uBBoxMin";

    /// <summary>
    /// Name of the uniform shader variable for the model bbox max vertex [local].
    /// Used by normal geometry shader to compute normal scale length.
    /// </summary>
    public const string UNINAME_uBBoxMax = "uBBoxMax";

    /// <summary>
    /// Name of the uniform shader variable for the normal length scaling factor respect model bbox size [local].
    /// Used by normal geometry shader to compute normal scale length.
    /// </summary>
    public const string UNINAME_uNormalFactor = "uNormalFactor";

    /// <summary>
    /// Name of the uniform shader variable for the figure highlight flag.
    /// </summary>
    public const string UNINAME_uFigureHighlight = "uFigureHighlight";

    /// <summary>
    /// Name of the uniform shader variable to override vertex material ambient strength (0..1).
    /// </summary>
    public const string UNINAME_uOverrideAmbient = "uOverrideAmbient";

    /// <summary>
    /// Name of the uniform shader variable to enable/disable vertex material ambient override strength.
    /// </summary>
    public const string UNINAME_uOverrideAmbientEnabled = "uOverrideAmbientEnabled";

    /// <summary>
    /// Name of the uniform shader variable to override vertex material diffuse strength (0..1).
    /// </summary>
    public const string UNINAME_uOverrideDiffuse = "uOverrideDiffuse";

    /// <summary>
    /// Name of the uniform shader variable to enable/disable vertex material diffuse override strength.
    /// </summary>
    public const string UNINAME_uOverrideDiffuseEnabled = "uOverrideDiffuseEnabled";

    /// <summary>
    /// Name of the uniform shader variable to override vertex material specular strength (0..1).
    /// </summary>
    public const string UNINAME_uOverrideSpecular = "uOverrideSpecular";

    /// <summary>
    /// Name of the uniform shader variable to enable/disable vertex material specular override strength.
    /// </summary>
    public const string UNINAME_uOverrideSpecularEnabled = "uOverrideSpecularEnabled";

    /// <summary>
    /// Name of the uniform shader variable to consider vertex color from texture.
    /// </summary>
    public const string UNINAME_uUseTexture = "uUseTexture";

    /// <summary>
    /// Name of the uniform shader variable to enable/disable shadow.
    /// </summary>
    public const string UNINAME_uShadows = "uShadows";

    /// <summary>
    /// Name of the uniform shader variable to set far plane in light depth map calc.
    /// </summary>
    public const string UNINAME_uFarPlaneLight = "uFarPlaneLight";

    /// <summary>
    /// Name of the uniform shader variable that enables evaluation of the light. 
    /// </summary>
    public const string UNINAME_uEvalLight = "uEvalLight";

    /// <summary>
    /// Name of the uniform shader variable to enable evluation of color from <see cref="UNINAME_uTextColor"/> variable.
    /// </summary>
    public const string UNINAME_uIsText = "uIsText";

    /// <summary>
    /// Name of the uniform shader variable that specify color for text figure types.
    /// </summary>
    public const string UNINAME_uTextColor = "uTextColor";

    /// <summary>
    /// Name of the uniform shader variable to specify main texture sampler reference.
    /// </summary>
    public const string UNINAME_uMainTexture = "uMainTexture";

    /// <summary>
    /// Name of the uniform shader variable to specify depth texture cube array reference.
    /// </summary>
    public const string UNINAME_uDepthMap = "uDepthMap";

    /// <summary>
    /// Name of the uniform shader variable for the name of points light block buffer.
    /// </summary>
    public const string SSBO_Blockname_PointLightsBlock = "PointLightsBlock";

    /// <summary>
    /// Name of the uniform shader variable for the name of points light matrixes block buffer.
    /// </summary>
    public const string SSBO_Blockname_PointLightsMatrixesBlock = "PointLightsMatrixesBlock";

    #endregion

    #region gl shadow map

    /// <summary>
    /// Width (pixels) of the cube depth shadow.
    /// </summary>
    public static uint SHADOW_WIDTH = 1000;

    /// <summary>
    /// Height (pixels) of the cube depth shadow.
    /// </summary>
    public static uint SHADOW_HEIGHT = 1000;

    #endregion

    #region gl texture & fonts

    /// <summary>
    /// Gl texture unit used for shadow map generation.
    /// </summary>
    public const TextureUnit SHADOW_TEXTURE_UNIT = TextureUnit.Texture1;

    /// <summary>
    /// Gl texture unit used for main figure texture usages.
    /// </summary>
    public const TextureUnit MAIN_TEXTURE_UNIT = TextureUnit.Texture2;

    /// <summary>
    /// Specify gpy internal format of texture used by <see cref="GLTexture2D"/>.
    /// </summary>
    public const InternalFormat DEFAULT_TEXTURE_INTERNAL_FORMAT = InternalFormat.Rgba8;

    /// <summary>
    /// Specify default image buffer format by <see cref="GLTexture2D"/>.
    /// </summary>
    public const PixelFormat DEFAULT_PIXEL_FORMAT = PixelFormat.Rgba;

    /// <summary>
    /// Height (pixel) of font samples rendering.<br/>
    /// Used by the <see cref="GLFontCharManager.GetFontChars"/>
    /// </summary>
    public const int FONT_BITMAP_PIXEL = 300;

    public const string BUILTIN_FONTNAME = "DejaVu Sans Mono";

    /// <summary>
    /// https://fontlibrary.org/en/font/dejavu-sans-mono
    /// </summary>
    public const string BUILTIN_FONTFILE = "DejaVuSansMono.ttf";

    #endregion

    #region view

    /// <summary>
    /// (Default) pathfilename of the view.json used by <see cref="GLControl.SaveView"/>.
    /// </summary>    
    public static string ViewDefaultPathfilename =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "view.json");

    /// <summary>
    /// (Default) perspective mode field of view (deg).
    /// </summary>
    public const float DEFAULT_FovDeg = 28.0f;

    /// <summary>
    /// (Default) orthogonal mode scale factor.
    /// </summary>
    public const float DEFAULT_OrthoZoom = 1;

    /// <summary>
    /// (Default) gl control wireframe enable/
    /// </summary>    
    public const bool DEFAULT_Wireframe = false;

    /// <summary>
    /// (Default) gl control use texture flag.
    /// </summary>
    public const bool DEFAULT_UseTexture = true;

    /// <summary>
    /// (Default) gl control use shadow flag.
    /// </summary>
    public const bool DEFAULT_UseShadows = true;

    /// <summary>
    /// (Default) gl control selection mode flag.
    /// </summary>
    public const bool DEFAULT_SelectionMode = false;

    /// <summary>
    /// (Default) gl control angle view addition in standard cube views (deg).
    /// </summary>
    public const float DEFAULT_StdViewAngleDeg = 45f;

    /// <summary>
    /// (Default) gl control perspective mode.
    /// </summary>
    public const bool DEFAULT_Perspective = true;

    /// <summary>
    /// (Default) gl control pan factor respect the screen size.
    /// </summary>
    public const float PAN_LITTLE_FACTOR = 1f / 10;

    /// <summary>
    /// (Default) gl control rotate model step (deg).
    /// </summary>
    public const float ROTATE_LITTLE_DEG = 5f;

    /// <summary>
    /// (Default) gl control step zoom scale factor.
    /// </summary>
    public const float ZOOM_LITTLE_FACTOR = 0.1f;

    /// <summary>
    /// (Default) gl control camera tilt step (deg).
    /// </summary>
    public const float TILT_LITTLE_DEG = 2f;

    /// <summary>
    /// (Default) gl control invalidate behavior for api.
    /// </summary>
    public const bool DEFAULT_INVALIDATE = true;

    /// <summary>
    /// (Default) trace shader location warning.
    /// </summary>
    public const bool DEFAULT_TRACE_LOCATION_WARNINGS = false;

    /// <summary>
    /// (Default) gl shader <see cref="UNINAME_uNormalFactor"/>.
    /// </summary>
    public const float DEFAULT_NORMAL_FACTOR = .025f;

    /// <summary>
    /// (Default) Used in <see cref="GLControl.BuildScreenProjectMatrix"/>.
    /// </summary>
    public const float SCREEN_ADJUST_W = 0;

    /// <summary>
    /// (Default) Used in <see cref="GLControl.BuildScreenProjectMatrix"/>.
    /// </summary>
    public const float SCREEN_ADJUST_H = 0;

    /// <summary>
    /// (Default) Used in <see cref="GLControl.BuildScreenProjectMatrix"/>.
    /// </summary>
    public const float SCREEN_ADJUST_TX = 0;

    /// <summary>
    /// (Default) Used in <see cref="GLControl.BuildScreenProjectMatrix"/>.
    /// </summary>
    public const float SCREEN_ADJUST_TY = 0;

    /// <summary>
    /// (Default) gl control camera pos [world].
    /// </summary>
    public static readonly Vector3 DEFAULT_CAMERA_POS = Vector3.UnitZ;

    /// <summary>
    /// (Default) gl control camera target [world].
    /// </summary>
    public static readonly Vector3 DEFAULT_CAMERA_TARGET = Vector3.Zero;

    /// <summary>
    /// (Default) gl control camera up [world].
    /// </summary>
    public static readonly Vector3 DEFAULT_CAMERA_UP = Vector3.UnitY;

    /// <summary>
    /// (Default) gl control shade with edge flag.
    /// </summary>
    public const bool DEFAULT_SHADE_WITH_EDGE = false;

    /// <summary>
    /// (Default) gl control show normals flags.
    /// </summary>
    public const bool DEFAULT_SHOW_NORMALS = false;

    #endregion    

    #region gl light

    /// <summary>
    /// (Default) gl point light color.
    /// </summary>
    public static readonly Color DEFAULT_LIGHT_COLOR = Color.White;

    /// <summary>
    /// (Default) gl point light constant attenuation factor.
    /// </summary>
    public const float DEFAULT_LIGHT_CONSTANT = 1.0f;

    /// <summary>
    /// (Default) gl point light linear attenuation factor.
    /// </summary>
    public const float DEFAULT_LIGHT_LINEAR = 0;// 5e-1f;

    /// <summary>
    /// (Default) gl point light quadratic attenuation factor.
    /// </summary>
    public const float DEFAULT_LIGHT_QUADRATIC = 0; //1e-4f;

    /// <summary>
    /// (Default) gl point light show point size (pixel).
    /// </summary>
    public const float DEFAULT_LIGHT_SHOW_POINT_SIZE = 10.0f;

    /// <summary>
    /// (Default) gl point light show point flag.
    /// </summary>
    public const bool DEFAULT_LIGHT_SHOW_POINT = false;

    #endregion

    #region gl colors

    /// <summary>
    /// (Default) <see cref="GLControl.Clear"/> background color.
    /// </summary>
    public static readonly Color DEFAULT_ClearColor = Color.Black;

    /// <summary>
    /// (Default) <see cref="GLText.Color"/>.
    /// </summary>
    public static readonly Color DEFAULT_TextColor = Color.White;

    /// <summary>
    /// (Default) <see cref="MakeFrustum"/> camera object lines color.
    /// </summary>
    public static readonly Color DEFAULT_CameraObjectColor = Color.Green;

    #endregion

}