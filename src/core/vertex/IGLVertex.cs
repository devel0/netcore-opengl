namespace SearchAThing.OpenGL.Core;

/// <summary>
/// 32bit vertex flags
/// </summary>
public enum GLVertexFlag
{
    /// <summary>
    /// VERTEX_FLAG_SELECTED : if set the vertex will coloured to appears as selected.
    /// </summary>    
    Selected = (1 << 0),
};

/// <summary>
/// Represent a vertex and its the base object managed by the <see cref="GLVertexManager"/>.<br/>
/// Primitives such as <see cref="GLPoint"/>, <see cref="GLLine"/> and <see cref="GLTriangle"/> store 
/// their vertex information using this type.<br/>
/// When the gl render happens ( see <see cref="GLControl.Render"/> )
/// all the model vertexes are mapped to the gpu ram and figures with their vertex indexes are feed into.<br/>
/// Internally the <see cref="GLVertexManager"/> hold a struct projection of this object, the
/// <see cref="GLVertexStruct"/>.
/// </summary>
/// <remarks>
/// In order to manage operations such as addition and removal of figures with related vertexes
/// the vertex manager doesn't allow to register a gl vertex twice, thus two figures cannot share the 
/// same gl vertex reference.<br/>
/// Note: Internally the vertex manager doesn't duplicate any vertex equals to other existing.
/// </remarks>
[JsonObject(MemberSerialization.OptIn)]
public interface IGLVertex : IGLVertexManagerObject, INotifyPropertyChanged
{

    /// <summary>
    /// Vertex position [local].
    /// </summary>    
    [JsonProperty]
    Vector3 Position { get; set; }

    /// <summary>
    /// Color encoded in vector4 (range 0..1) as (red, green, blue, alpha).<br/>
    /// Alpha=1 (full opaque).<br/>
    /// Alpha=0 (full transparent).
    /// </summary>
    [JsonProperty]
    Vector4 MaterialColor { get; set; }

    /// <summary>
    /// Describe the strength (0..1) of the ambient, diffuse, specular light effects.
    /// </summary>
    [JsonProperty]
    Vector3 MaterialProperties { get; set; }

    /// <summary>
    /// Vertex normal [local].
    /// </summary>    
    [JsonProperty]
    Vector3 Normal { get; set; }

    /// <summary>
    /// Invert the normal flag ( used by <see cref="GLVertex.Mirror"/>.    
    /// </summary>    
    /// <seealso cref="EffectiveNormal"/>
    [JsonProperty]
    bool InvertNormal { get; set; }

    /// <summary>
    /// Vertex normal [local] eventually inverted if flag <see cref="InvertNormal"/> present.
    /// </summary>    
    Vector3 EffectiveNormal { get; }

    /// <summary>
    /// Vertex texture offset ( range [0,0] to [1,1] ).
    /// </summary>    
    [JsonProperty]
    Vector2 TextureST { get; set; }    

    /// <summary>
    /// Primitive referencing this vertex in one of their components.<br/>
    /// Its null if related figure not yet added to the vertex manager.
    /// </summary>    
    IGLPrimitive? ParentPrimitive { get; }

    /// <summary>
    /// Figure referencing this vertex in one of their primitive's components.<br/>
    /// Its null if related figure not yet added to the vertex manager.
    /// </summary>    
    IGLFigure? ParentFigure { get; }

    /// <summary>
    /// States if this vertex is already registed to a vertex manager.
    /// </summary>
    bool IsAttached { get; }

    /// <summary>
    /// <see cref="GLVertexManager"/> vertex index associated to this gl vertex or null if not yet registered to.
    /// </summary>    
    uint? Index { get; }

    /// <summary>
    /// String signature of this vertex, used by <see cref="GLVertexManager"/> to keep a unique vertex array 
    /// of <see cref="GLVertexStruct"/>.
    /// </summary>    
    string Signature { get; }

    /// <summary>
    /// Create a struct representation of this vertex.<br/>
    /// Some information used only by framework itself not pertinent to those used by the shader, such as <see cref="Index"/>
    /// are omitted.
    /// </summary>  
    /// <seealso cref="GLVertexStruct"/>    
    GLVertexStruct ToStruct();

    /// <summary>
    /// Evaluated through parent <see cref="IGLFigure"/> it states if this vertex has to be processed in the
    /// gl shader pipeline within <see cref="Constants.UNINAME_uFigureScreenCoord"/> uniform flag.
    /// </summary>
    /// <seealso cref="GLControl.ScreenModelMatrix"/>
    /// <seealso cref="GLControl.ScreenProjectionMatrix"/>
    bool ScreenCoordMode { get; }

}

public static partial class Ext
{

    /// <summary>
    /// Change the color of the vertex.
    /// </summary>
    /// <param name="vertex">Vertex which change the color.</param>
    /// <param name="color">Color to apply.</param>
    public static IGLVertex SetColor(this IGLVertex vertex, in Color color) =>
        vertex.SetColor(color.ToVector4());

    /// <summary>
    /// Change the color of the vertex.
    /// </summary>
    /// <param name="vertex">Vertex which change the color.</param>
    /// <param name="rgbaColor">Color to apply encoded in vector4 (range 0..1) as (red, green, blue, alpha).<br/>Alpha=1 (full opaque).<br/>Alpha=0 (full transparent).</param>
    public static IGLVertex SetColor(this IGLVertex vertex, in Vector4 rgbaColor)
    {
        vertex.MaterialColor = rgbaColor;
        return vertex;
    }

    /// <summary>
    /// Convert given gl vertex to dxf vector3.
    /// </summary>    
    public static netDxf.Vector3 ToDxfVector3(this IGLVertex vertex) => vertex.Position.ToDxfVector3();

}