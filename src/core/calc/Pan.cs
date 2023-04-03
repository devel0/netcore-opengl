namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    /// <summary>
    /// Pan the view ( Orthogonal mode ) by the given screen translation coord generating new camera config.
    /// </summary>
    /// <param name="from">Screen pan from coord (x,y) [screen].</param>
    /// <param name="to">Screen pan "from" coord (x,y) [screen].</param>
    /// <param name="size">Screen size (x,y) [screen].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="pm">Projection matrix [eye].</param>
    /// <param name="cameraPos">Camera position (x,y,z) [world].</param>
    /// <param name="cameraTarget">Camera target (x,y,z) [world].</param>
    /// <param name="panCameraPos">Pan result new camera position (x,y,z) [world].</param>
    /// <param name="panCameraTarget">Pan result new camera target (x,y,z) [world].</param>
    public static void OrthoPan(
        in Vector2 from, in Vector2 to, in Vector2 size,
        in Matrix4x4 vm, in Matrix4x4 pm,
        in Vector3 cameraPos, in Vector3 cameraTarget,
        out Vector3 panCameraPos, out Vector3 panCameraTarget)
    {
        var eFrom = ScreenToEye_virtual(from, size, pm);
        var eTo = ScreenToEye_virtual(to, size, pm);

        var wFrom = EyeToWorld(eFrom, vm);
        var wTo = EyeToWorld(eTo, vm);

        var wDelta = (wTo - wFrom);        

        panCameraPos = cameraPos - wDelta;
        panCameraTarget = cameraTarget - wDelta;
    }

    /// <summary>
    /// Pan the view ( Perspective mode ) by the given screen translation coord generating new camera config.
    /// </summary>
    /// <param name="refPt">A reference point [local] inside model bbox ( generally BBox.Middle ) for relative coord calc.</param>
    /// <param name="from">Screen pan from coord (x,y) [screen].</param>
    /// <param name="to">Screen pan "from" coord (x,y) [screen].</param>
    /// <param name="size">Screen pan "to" coord (x,y) [screen].</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="pm">Projection matrix [eye].</param>
    /// <param name="cameraPos">Camera position (x,y,z) [world].</param>
    /// <param name="cameraTarget">Camera target (x,y,z) [world].</param>
    /// <param name="panCameraPos">Pan result new camera position (x,y,z) [world].</param>
    /// <param name="panCameraTarget">Pan result new camera target (x,y,z) [world].</param>
    public static void PerspectivePan(
        in Vector3 refPt,
        in Vector2 from, in Vector2 to, in Vector2 size,
        in Matrix4x4 mm, in Matrix4x4 vm, in Matrix4x4 pm,
        in Vector3 cameraPos, in Vector3 cameraTarget,
        out Vector3 panCameraPos, out Vector3 panCameraTarget)
    {
        var eFrom = ScreenToEye_virtual(from, size, pm);
        var eTo = ScreenToEye_virtual(to, size, pm);

        var wFrom = EyeToWorld(eFrom, vm);
        var wTo = EyeToWorld(eTo, vm);

        var w = (LocalToWorld(refPt, mm) - cameraPos).Length();

        var wDelta = (wTo - wFrom) * w;

        panCameraPos = cameraPos - wDelta;
        panCameraTarget = cameraTarget - wDelta;
    }

}