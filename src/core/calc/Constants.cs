namespace SearchAThing.OpenGL.Core;

public static partial class Constants
{

    /// <summary>
    /// CS world with origin=(0,0,0) basex=(1,0,0) basey=(0,1,0) basez=(0,0,1)
    /// </summary>    
    public static readonly Matrix4x4 WCS = MakeWCS();

    /// <summary>
    /// Y axis orientation [screen].<br/>    
    /// </summary>    
    /// <remarks>
    /// TODO: Actually tested only for YPositiveUp. It's a constant hard coded.
    /// </remarks>    
    public const YAxisOrientationEnum YAxisOrientation = YAxisOrientationEnum.YPositiveUp;

    /// <summary>
    /// (Default) Absolute position along [eye] z axis of the near clipping plane.
    /// </summary>
    /// <see cref="SearchAThing.OpenGL.Core.Toolkit.PreventZFighting"/>
    public const float DEFAULT_Near = 0.1f;

    /// <summary>
    /// (Default) Absolute position along [eye] z axis of the far clipping plane.
    /// </summary>
    /// <see cref="SearchAThing.OpenGL.Core.Toolkit.PreventZFighting"/>
    public const float DEFAULT_Far = 50000.0f;

}