namespace SearchAThing.OpenGL.Shapes;

public static partial class Constants
{

    /// <summary>
    /// Nr. of divisions for cone shape.
    /// <see cref="Cone"/>
    /// </summary>
    public const int DEFAULT_CONE_DIVISIONS = 8;

    /// <summary>
    /// Nr. of divisions for arrow shape.
    /// <see cref="Arrow"/>
    /// </summary>
    public const int DEFAULT_ARROW_DIVISIONS = 16;

    /// <summary>
    /// Diameter of tip respect to the rod diameter.
    /// </summary>
    public const float DEFAULT_ARROW_TIP_DIAMETER_FACTOR = 2f;

    /// <summary>
    /// How much tip length is included respect to arrow total length.
    /// </summary>
    public const float DEFAULT_ARROW_TIP_LEN_FACTOR = .2f;    

}