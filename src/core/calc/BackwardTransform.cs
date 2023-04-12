namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    //          om          mm          vm        pm        (*w)      (size)
    // OBJECT <---- LOCAL <---- WORLD <---- EYE <---- CLIP <---- NDC <------ SCREEN
    //

    /// <summary>
    /// Converts given screen coord to virtual normalized device coord (z=0).<br/>
    /// Actual <see cref="Constants.YAxisOrientation"/> will considered.
    /// </summary>            
    /// <param name="screen">(x,y) screen coord</param>
    /// <param name="size">(x,y) screen size</param>    
    /// <returns>(virtual) normalized device coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ScreenToNDC_virtual(
        in Vector2 screen, in Vector2 size) => YAxisOrientation switch
        {
            YAxisOrientationEnum.YPositiveDown => new Vector3(
                (2 * screen.X) / (size.X - 1) - 1,
                -(2 * screen.Y) / (size.Y - 1),
                0),

            YAxisOrientationEnum.YPositiveUp => new Vector3(
                (2 * screen.X) / (size.X - 1) - 1,
                (2 * screen.Y) / (size.Y - 1) - 1,
                0),

            _ => throw new NotImplementedException($"yaxis orientation {YAxisOrientation} not supported")
        };

    /// <summary>
    /// Converts given normalized device coord to virtual clip space coord (w=1).    
    /// </summary>
    /// <param name="ndc">(x,y,z) ndc coord</param>
    /// <param name="w">Value of 4-th component to set in computed clip space coord ( Default: 1 )</param>    
    /// <returns>(virtual) Clip space coord (x,y,z,w).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 NDCToClip_virtual(
        in Vector3 ndc, float w = 1) => new Vector4(ndc.X, ndc.Y, ndc.Z, w);

    /// <summary>
    /// Converts given clip space coord to the corresponding eye space coord.<br/>
    /// Makes use of the inverse of the given projection matrix.
    /// </summary>
    /// <param name="clip">Clip space coord (x,y,z,w).</param>
    /// <param name="pm">Projection matrix.</param>
    /// <returns>Eye space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ClipToEye(
        in Vector4 clip, in Matrix4x4 pm) => Vector4.Transform(clip, pm.Inverse()).XYZ();

    /// <summary>
    /// Converts given eye space coord to the corresponding world space coord.<br/>
    /// Makes use of the inverse of the given view matrix.
    /// </summary>
    /// <param name="eye">Eye space coord (x,y,z).</param>
    /// <param name="vm">View matrix.</param>
    /// <returns>World space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 EyeToWorld(
        in Vector3 eye, in Matrix4x4 vm) => Vector3.Transform(eye, vm.Inverse());

    /// <summary>
    /// Converts given world space coord to the corresponding local space coord.<br/>
    /// Makes use of the inverse of the given model matrix.
    /// </summary>
    /// <param name="world">World space coord (x,y,z).</param>
    /// <param name="mm">Model matrix.</param>
    /// <returns>Local space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 WorldToLocal(
        in Vector3 world, in Matrix4x4 mm) => Vector3.Transform(world, mm.Inverse());

    /// <summary>
    /// Converts given local space coord to the corresponding object space coord.<br/>
    /// Makes use of the inverse of the given object matrix.
    /// </summary>
    /// <param name="local">Local space coord (x,y,z).</param>
    /// <param name="om">Object matrix.</param>
    /// <returns>Object space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 LocalToObject(
        in Vector3 local, in Matrix4x4 om) => Vector3.Transform(local, om.Inverse());

    //===================================================================
    // backward (combined)
    //===================================================================

    /// <summary>
    /// Converts given screen space coord to corresponding virtual in clip space coord (z=0, w=1).<br/>        
    /// </summary>
    /// <param name="screen">Screen coord (x,y).</param>
    /// <param name="size">Screen size (x,y).</param>
    /// <param name="w">Value of 4-th component to set in computed clip space coord ( Default: 1 ).</param>
    /// <returns>(virtual) Clip space coord (x,y,z,w).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 ScreenToClip_virtual(
        in Vector2 screen, in Vector2 size, float w = 1) =>
        NDCToClip_virtual(ScreenToNDC_virtual(screen, size), w);

    /// <summary>
    /// Converts given screen space coord to corresponding virtual in eye space coord (z=0, w=1, eye_z=-1).<br/>
    /// It makes use of the inverse of given projection matrix.    
    /// </summary>
    /// <param name="screen">Screen coord (x,y).</param>
    /// <param name="size">Screen size (x,y).</param>
    /// <param name="pm">Projection matrix.</param>
    /// <param name="eye_z">Value of 3-th component to set in computed eye space coord ( Default: -1 ).</param>
    /// <returns>(virtual) Eye space coord (x,y,z).</returns>
    public static Vector3 ScreenToEye_virtual(
        in Vector2 screen, in Vector2 size, in Matrix4x4 pm, float eye_z = -1)
    {
        var clip = ScreenToClip_virtual(screen, size);
        var eye = ClipToEye(clip, pm) with { Z = eye_z };

        return eye;
    }

    /// <summary>
    /// Converts given screen space coord to corresponding virtual in local space coord (z=0, w=1, eye_z=-1).<br/>
    /// It makes use of the inverse of given modelview matrix and projection matrix.
    /// </summary>
    /// <param name="screen">Screen coord (x,y).</param>
    /// <param name="size">Screen size (x,y).</param>
    /// <param name="mvm">Modelview matrix (model * view).</param>
    /// <param name="pm">Projection matrix.</param>
    /// <param name="eye_z">Value of 3-th component to set in computed eye space coord ( Default: -1 ).</param>
    /// <param name="w">Value of 4-th component to set in computed clip space coord ( Default: 1 ).</param>
    /// <returns>(virtual) Local space coord (x,y,z).</returns>
    public static Vector3 ScreenToLocal_virtual(
        in Vector2 screen, in Vector2 size, in Matrix4x4 mvm, in Matrix4x4 pm, float eye_z = -1, float w = 1) =>
        ClipToLocal(ScreenToClip_virtual(screen, size, w), mvm, pm);

    /// <summary>
    /// Converts given clip space coord to the corresponding local space coord.<br/>
    /// It makes use of the inverse of given modelview matrix and projection matrix.
    /// </summary>
    /// <param name="clip">Clip space coord (x,y,z,w).</param>
    /// <param name="mvm">Modelview matrix (model * view).</param>
    /// <param name="pm">Projection matrix.</param>
    /// <returns>Local space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ClipToLocal(
        in Vector4 clip, in Matrix4x4 mvm, in Matrix4x4 pm) =>
        EyeToLocal(ClipToEye(clip, pm), mvm);

    /// <summary>
    /// Converts given eye space coord to the corresponding local space coord.<br/>
    /// It makes use of the inverse of given modelview matrix.
    /// </summary>
    /// <param name="eye">Eye space (x,y,z) coord.</param>
    /// <param name="mvm">Modelview matrix (model * view).</param>
    /// <returns>Local space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 EyeToLocal(
        in Vector3 eye, in Matrix4x4 mvm) => Vector3.Transform(eye, mvm.Inverse());

    //===================================================================
    // util
    //===================================================================

    /// <summary>
    /// Create a raycast line [local] suitable to hit test of figure primitives.
    /// </summary>
    /// <param name="screen">Screen coord (x,y) [screen].</param>
    /// <param name="size">Screen size (x,y) [screen].</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="pm">Projection matrix [eye].</param>
    /// <param name="cp">Camera position [world].</param>
    /// <param name="perspective">Perspective mode.</param>        
    /// <returns>Raycast line [local].</returns>
    public static Line RayCastLocal(
        in Vector2 screen, in Vector2 size,
        in Matrix4x4 mm, in Matrix4x4 vm, in Matrix4x4 pm,
        in Vector3 cp, bool perspective
    )
    {
        Line res;

        if (perspective)
            res = Line.FromTo(WorldToLocal(cp, mm), ScreenToLocal_virtual(screen, size, mm * vm, pm));
        else
            res = Line.PointV(ScreenToLocal_virtual(screen, size, mm * vm, pm),
                EyeToLocal(Vector3.UnitZ, mm * vm) - EyeToLocal(Vector3.Zero, mm * vm));

        return res;
    }

}