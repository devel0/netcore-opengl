namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    /// <summary>
    /// Rotate the model by given [eye] x,y angles around given [local] rotation center.
    /// </summary>
    /// <param name="angleXRad">Eye space x angle rotation (rad).</param>
    /// <param name="angleYRad">Eye space y angle rotation (rad).</param>
    /// <param name="rot_center">Local space (x,y,z) rotation center [local].</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="mmRotated">Rotated model matrix [local].</param>
    public static void ModelRotate(
        float angleXRad, float angleYRad,
        in Vector3 rot_center,
        in Matrix4x4 mm,
        in Matrix4x4 vm,
        out Matrix4x4 mmRotated)
    {
        var xrotAxis = Vector3.Normalize(
            WorldToLocal(EyeToWorld(Vector3.UnitX, vm), mm) -
            WorldToLocal(EyeToWorld(Vector3.Zero, vm), mm));

        var yrotAxis = Vector3.Normalize(
            WorldToLocal(EyeToWorld(Vector3.UnitY, vm), mm) -
            WorldToLocal(EyeToWorld(Vector3.Zero, vm), mm));

        mmRotated =
            Matrix4x4.CreateTranslation(-rot_center)
            *
            Matrix4x4.CreateFromAxisAngle(yrotAxis, -angleYRad)
            *
            Matrix4x4.CreateFromAxisAngle(xrotAxis, angleXRad)
            *
            Matrix4x4.CreateTranslation(rot_center)
            *
            mm;
    }

    /// <summary>
    /// Tilt camera [view] by rotating it's up vector around [view] z axis.
    /// </summary>
    /// <param name="angleRad">Tilt angle (rad) [view].</param>
    /// <param name="cu">Camera up vector (x,y,z) [view].</param>
    /// <param name="cuTilted">Tilted camera up vector (x,y,z) [view].</param>
    public static void CameraTilt(
        float angleRad,
        in Vector3 cu,
        out Vector3 cuTilted) =>
        cuTilted = Vector3.Transform(cu, Matrix4x4.CreateRotationZ(-angleRad));


    /// <summary>    
    /// Rotate the camera [view] a given rotation center [local].        
    /// </summary>
    /// <param name="sAngleXRad">Camera rotation screen X angle (rad)..</param>
    /// <param name="sAngleYRad">Camera rotation screen Y angle (rad).</param>
    /// <param name="wCameraPos">Actual camera position [world].</param>
    /// <param name="wCameraTarget">Actual camera target [world].</param>
    /// <param name="wCameraUp">Actual camera up [world].</param>
    /// <param name="wRotatedCameraTarget">Rotated camera target [world].</param>
    /// <param name="wRotatedCameraUp">Rotated camera up [world].</param>
    public static void CameraRotate(
        float sAngleXRad, float sAngleYRad,
        in Vector3 wCameraPos, in Vector3 wCameraTarget, in Vector3 wCameraUp,
        out Vector3 wRotatedCameraTarget, out Vector3 wRotatedCameraUp)
    {
        var wCCS = MakeCCS(wCameraPos, wCameraTarget, wCameraUp);

        var mu =
            Matrix4x4.CreateFromAxisAngle(wCCS.BaseX(), sAngleXRad);

        var m =
            Matrix4x4.CreateTranslation(-wCameraPos)
            *
            mu
            *
            Matrix4x4.CreateFromAxisAngle(wCCS.BaseY(), sAngleYRad)
            *
            Matrix4x4.CreateTranslation(wCameraPos);

        wRotatedCameraTarget = Vector3.Transform(wCameraTarget, m);
        wRotatedCameraUp = Vector3.Transform(wCameraUp, mu);
    }

}