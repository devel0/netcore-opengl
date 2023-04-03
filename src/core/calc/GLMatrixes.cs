namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    /// <summary>
    /// Create model matrix to convert [local] space to [world] space.<br/>
    /// It's a SRT ( Scale-Rotate-Translate ) transformations matrix.<br/>
    /// It's used for one touch rotation of the model space objects.<br/>
    /// </summary>
    /// <param name="scale">Scale factor (x,y).</param>
    /// <param name="rotation">Rotation quaternion (x,y,z,w).</param>
    /// <param name="translation">Translation (x,y,z).</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.ModelRotate"/>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateModelMatrix(
        in Vector3 scale, in Quaternion rotation, in Vector3 translation,
        out Matrix4x4 mm)
    {
        mm =
            Matrix4x4.CreateScale(scale)
            *
            Matrix4x4.CreateFromQuaternion(rotation)
            *
            Matrix4x4.CreateTranslation(translation);
    }

    /// <summary>
    /// Create view matrix to convert [world] to [eye] space.<br/>
    /// It's used for pan, camera movements functions and either for scale function in perspective mode.
    /// </summary>
    /// <param name="cameraPos">Camera position (x,y,z) [world].</param>
    /// <param name="cameraTarget">Camera target (x,y,z) [world].</param>
    /// <param name="cameraUp">Camera up (x,y,z) [world].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.OrthoPan"/>        
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.PerspectivePan"/>        
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.CameraRotate"/>        
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.CameraTilt"/>    
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.PerspectiveZoom"/>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateViewMatrix(
        in Vector3 cameraPos, in Vector3 cameraTarget, in Vector3 cameraUp,
        out Matrix4x4 vm)
    {
        var sm = YAxisOrientation switch
        {
            YAxisOrientationEnum.YPositiveDown => Matrix4x4.Identity,
            YAxisOrientationEnum.YPositiveUp => Matrix4x4.CreateScale(1, -1, 1),
            _ => throw new ArgumentException($"invalid YAxisOrientation {YAxisOrientation}")
        };

        vm =
            Matrix4x4.CreateLookAt(cameraPos, cameraTarget, cameraUp)
            *
            sm;
    }

    /// <summary>
    /// Create projection matrix ( Perspective mode ) to convert [eye] to [clip] space.<br/>
    /// It's used to create perspective field of view in perspective mode.
    /// </summary>
    /// <param name="fovDeg">Angle of view [deg].</param>
    /// <param name="ar">Projection screen aspection ratio (x/y).</param>
    /// <param name="near">Absolute position along [eye] z axis of the near clipping plane.</param>
    /// <param name="far">Absolute position along [eye] z axis of the far clipping plane.</param>
    /// <param name="pm">Projection matrix [eye].</param>
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.PreventZFighting"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreatePerspectiveProjectionMatrix(
        float fovDeg, float ar, float near, float far,
        out Matrix4x4 pm) =>
        pm = Matrix4x4.CreatePerspectiveFieldOfView(fovDeg.ToRad(), ar, near, far);

    /// <summary>
    /// Create projection matrix ( Orthogonal mode ) to convert [eye] to [clip] space.<br/>
    /// </summary>
    /// <param name="size">screen size (x,y) [screen].</param>    
    /// <param name="eOrthoZoom">[eye] zoom factor about screen size used as a reference.</param>
    /// <param name="near">Absolute position along [eye] z axis of the near clipping plane.</param>
    /// <param name="far">Absolute position along [eye] z axis of the far clipping plane.</param>
    /// <param name="vm"></param>
    /// <param name="pm"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CreateOrthogonalProjectionMatrix(
        in Vector2 size,
        float eOrthoZoom, float near, float far,
        in Matrix4x4 vm,
        out Matrix4x4 pm) =>
        pm = Matrix4x4.CreateOrthographic(size.X * eOrthoZoom, size.Y * eOrthoZoom, near, far);

}