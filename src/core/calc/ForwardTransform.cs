namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    //          om         mm          vm        pm         /w        size
    // OBJECT ----> LOCAL ----> WORLD ----> EYE ----> CLIP ----> NDC ------> SCREEN
    //

    /// <summary>
    /// Converts given object space coord to the corresponding local space coord.<br/>    
    /// </summary>
    /// <param name="obj">Object space coord (x,y,z).</param>
    /// <param name="om">Object matrix.</param>
    /// <returns>Local space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ObjectToLocal(in Vector3 obj, in Matrix4x4 om) =>
        Vector3.Transform(obj, om);

    /// <summary>
    /// Converts given local space coord to the corresponding world space coord.<br/>
    /// </summary>
    /// <param name="local">Local space coord (x,y,z).</param>
    /// <param name="mm">Model matrix.</param>
    /// <returns>World space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 LocalToWorld(in Vector3 local, in Matrix4x4 mm) =>
        Vector3.Transform(local, mm);

    /// <summary>
    /// Converts given world space coord to the corresponding eye space coord.
    /// </summary>
    /// <param name="world">World coord (x,y,z).</param>
    /// <param name="vm">View matrix.</param>
    /// <returns>Eye space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 WordlToEye(in Vector3 world, in Matrix4x4 vm) =>
        Vector3.Transform(world, vm);

    /// <summary>
    /// Converts given world eye coord to the corresponding clip space coord.
    /// </summary>
    /// <param name="eye">Eye coord (x,y,z).</param>
    /// <param name="pm">Projection matrix.</param>
    /// <returns>Clip space coord (x,y,z,w).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4 EyeToClip(in Vector3 eye, in Matrix4x4 pm) =>
        Vector4.Transform(eye, pm);

    /// <summary>
    /// Converts given clip coord to the corresponding ndc space coord.
    /// </summary>
    /// <param name="clip">Clip space coord (x,y,z,w).</param>
    /// <returns>Normalized device coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ClipToNDC(in Vector4 clip) => (clip / clip.W).XYZ();

    /// <summary>
    /// Convert given normalized device coord to the corresponding screen coord.<br/>
    /// Depending on <see cref="SearchAThing.OpenGL.Core.Constants.YAxisOrientation"/> ( default: YPositiveDown ):<br/>
    /// - <b>YPositiveDown</b> : ndc (-1,+1,), ..., (+1,-1,) to screen (0, 0), ..., (w-1, h-1)<br/>
    /// - <b>YPositiveUp</b> : ndc (-1,-1,), ..., (+1,+1,) to screen (0, 0), ..., (w-1, h-1)<br/>
    /// </summary>    
    /// <param name="ndc">Normalized device coord (x,y,z).</param>
    /// <param name="size">Screen size (x,y).</param>        
    /// <returns>Screen coord (x,y).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 NDCToScreen(
        in Vector3 ndc, in Vector2 size) => YAxisOrientation switch
        {
            YAxisOrientationEnum.YPositiveDown => new Vector2(
                (ndc.X + 1) * 0.5f * (size.X - 1),
                (1f - (ndc.Y + 1) * 0.5f) * (size.Y - 1)),

            YAxisOrientationEnum.YPositiveUp => new Vector2(
                (ndc.X + 1) * 0.5f * (size.X - 1),
                (ndc.Y + 1) * 0.5f * (size.Y - 1)),

            _ => throw new NotImplementedException($"yaxis orientation {YAxisOrientation} not supported")
        };

    //===================================================================
    // forward (combined)
    //===================================================================

    /// <summary>
    /// Converts given local space coord to the corresponding eye coord.
    /// </summary>
    /// <param name="local">Local space coord (x,y,z).</param>
    /// <param name="mvm">Modelview matrix ( model * view ).</param>
    /// <returns>Eye space coord (x,y,z).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 LocalToEye(in Vector3 local, in Matrix4x4 mvm) =>
        Vector3.Transform(local, mvm);

    /// <summary>
    /// Converts given local space coord to the corresponding screen coord.
    /// </summary>
    /// <param name="local">Local space coord (x,y,z).</param>
    /// <param name="size">Sceen size (x,y).</param>
    /// <param name="mvpm">ModelViewProjection matrix ( model * view * projection ).</param>
    /// <returns>Screen space coord (x,y).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 LocalToScreen(
        in Vector3 local, in Vector2 size, in Matrix4x4 mvpm) =>
        ClipToScreen(Vector4.Transform(local, mvpm), size);

    /// <summary>
    /// Converts given clip space coord to the corresponding screen coord.
    /// </summary>
    /// <param name="clip">Clip space coord (x,y,z,w).</param>
    /// <param name="size">Screen size (x,y).</param>
    /// <returns>Screen space coord (x,y).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ClipToScreen(
        in Vector4 clip, in Vector2 size) => NDCToScreen(ClipToNDC(clip), size);

}