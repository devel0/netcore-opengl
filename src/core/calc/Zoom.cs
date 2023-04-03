namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    /// <summary>
    /// Scale by given factor the given [eye] zoom ( Orthogonal mode ).
    /// </summary>
    /// <param name="scale">Zoom scale factor.</param>
    /// <param name="orthoZoom">Eye space ortho zoom.</param>
    /// <param name="scaledOrthoZoom">Computed eye space ortho zoom.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void OrthoZoom(float scale, float orthoZoom, out float scaledOrthoZoom) =>
        scaledOrthoZoom = orthoZoom - orthoZoom * scale;

    /// <summary>
    /// Zoom the view [eye] ( Perspective mode ) by the given scale factor.
    /// </summary>
    /// <param name="scale">Zoom scale factor.</param>
    /// <param name="bbox">Model bbox [local].</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="cameraPos">Camera position (x,y,z) [world].</param>
    /// <param name="cameraTarget">Camera target (x,y,z) [world].</param>
    /// <param name="scaledCameraPos">Computed camera position (x,y,z) [world].</param>
    /// <param name="scaledCameraTarget">Computed camera target (x,y,z) [world].</param>
    public static void PerspectiveZoom(float scale,
        BBox bbox,
        in Matrix4x4 mm, in Matrix4x4 vm,
        in Vector3 cameraPos, in Vector3 cameraTarget,
        out Vector3 scaledCameraPos, out Vector3 scaledCameraTarget)
    {
        var lRefPt = bbox.Middle;

        var wFw = EyeToWorld(-Vector3.UnitZ, vm) - EyeToWorld(Vector3.Zero, vm);
        var w = wFw * Max((LocalToWorld(lRefPt, mm) - cameraPos).Length(), bbox.Size.Max() / 10) * scale;

        scaledCameraPos = cameraPos + w;
        scaledCameraTarget = cameraTarget + w;
    }

    /// <summary>
    /// Center the view ( Perspective mode ) by computing camera [world] position and target.
    /// </summary>
    /// <param name="pts">Local space point that represents entire model [local].</param>
    /// <param name="size">Screen size (x,y) [screen].</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="pm">Projection matrix [eye].</param>
    /// <param name="cameraPos">Camera position (x,y,z) [world].</param>
    /// <param name="cameraTarget">Camera target (x,y,z) [world].</param>
    /// <param name="panCameraPos"></param>
    /// <param name="panCameraTarget"></param>
    /// <param name="bbox"></param>
    public static void PerspectiveCenter(
        IEnumerable<Vector3> pts,
        in Vector2 size,
        in Matrix4x4 mm, in Matrix4x4 vm, in Matrix4x4 pm,
        in Vector3 cameraPos, in Vector3 cameraTarget,
        out Vector3 panCameraPos, out Vector3 panCameraTarget, out BBox bbox
    )
    {
        bbox = new BBox();

        ComputeSBBox(pts, size, mm, vm, pm, out var sMin, out var sMax, bbox);

        var sdx = (sMin.X + sMax.X) / 2 - size.X / 2;
        var sdy = (sMin.Y + sMax.Y) / 2 - size.Y / 2;

        PerspectivePan(
            bbox.Middle,
            new Vector2(sdx, sdy), Vector2.Zero, size,
            mm, vm, pm,
            cameraPos, cameraTarget,
            out panCameraPos, out panCameraTarget);
    }

    /// <summary>
    /// Fit current model view for perspective projection
    /// </summary>
    /// <param name="pts">model or bbox points [local]</param>    
    /// <param name="size">size [screen]</param>
    /// <param name="mm">mm [local]</param>
    /// <param name="vm">vm [world]</param>    
    /// <param name="pm">pm [eye]</param>    
    /// <param name="cameraPos">cameraPos [world]</param>
    /// <param name="cameraTarget">cameraTarget [world]</param>
    /// <param name="cameraUp">cameraUp [world]</param>    
    /// <param name="fovDeg">fovDeg [world]</param>
    /// <param name="fitCameraPos">fitCameraPos [world]</param>
    /// <param name="fitCameraTarget">fitCameraTarget [world]</param>
    /// <param name="LOOP_CNT_MAX">nr of max loop to compute fit</param>
    /// <param name="FACTOR_TOL">break fit loop when factor equals 1 with given factor tolerance</param>
    public static void PerspectiveZoomFit(
        IEnumerable<Vector3> pts,
        in Vector2 size,
        in Matrix4x4 mm, in Matrix4x4 vm, in Matrix4x4 pm,
        in Vector3 cameraPos, in Vector3 cameraTarget, in Vector3 cameraUp,
        float fovDeg,
        out Vector3 fitCameraPos, out Vector3 fitCameraTarget,
        int LOOP_CNT_MAX = 10,
        float FACTOR_TOL = 1e-2f
    )
    {
        var cp = cameraPos;
        var ct = cameraTarget;
        var cfw = Vector3.Normalize(ct - cp);

        var mvm = mm * vm;

        var eBBox = new BBox();
        foreach (var lpt in pts)
        {
            eBBox.ApplyUnion(LocalToEye(lpt, mvm));
        }

        var sAr = size.X / size.Y;

        var tanPhi = (float)Tan(fovDeg.ToRad() / 2);

        var M = EyeToWorld(eBBox.Middle, vm);

        var S = eBBox.Size;
        var SX = S.X;
        var SY = S.Y;

        var d1 = S.Z / 2 + SX / (2 * tanPhi) / sAr;
        var d2 = S.Z / 2 + SY / (2 * tanPhi);

        float d3;

        if (SX > SY * sAr)
            d3 = d1;

        else
            d3 = d2;

        cp = M - cfw * d3;
        ct = cp + cfw;

        // center

        CreateViewMatrix(cp, ct, cameraUp, out var _vm);

        PerspectiveCenter(pts, size, mm, _vm, pm, cp, ct, out cp, out ct, out var bbox);

        // screen scale

        int loop = 0;

        mvm = mm * _vm;
        var wm = LocalToWorld(bbox.Middle, mm);
        var we = LocalToEye(bbox.Middle, mvm);

        var factors = new float[LOOP_CNT_MAX];
        float minFactor = 1;
        float maxFactor = 1;

        while (loop < LOOP_CNT_MAX)
        {
            ComputeSBBox(pts, size, mm, _vm, pm, out var sMin, out var sMax);

            var sBBoxW = sMax.X - sMin.X;
            var sBBoxH = sMax.Y - sMin.Y;

            float factor;

            if (sBBoxW > sBBoxH * sAr)
            {
                factor = size.X / sBBoxW;
            }

            else
            {
                factor = size.Y / sBBoxH;
            }

            //     

            if (Abs(1 - factor) < FACTOR_TOL)
            {
                // Debug.WriteLine($"break with factor");
                break;
            }

            // force to converge

            var _factor = factor;

            if (minFactor < 1 && maxFactor > 1)
            {
                if (factor > 1)
                {
                    factor = 1f + .5f * (Min(factor, maxFactor) - 1);
                }
                else
                {
                    factor = 1f - .5f * (Max(factor, minFactor));
                }
            }

            factors[loop] = factor;
            if (factor < minFactor) minFactor = factor;
            if (factor > maxFactor) maxFactor = factor;

            // Debug.WriteLine($"{loop}: {_factor.ToString("0.00")} -> factor: {factor.ToString("0.00")} [min:{minFactor.ToString("0.00")} max:{maxFactor.ToString("0.00")}]");

            cfw = Vector3.Normalize(ct - cp);
            var cdelta = cfw * Abs(we.Z) * (factor - 1);
            cp = cp + cdelta;
            ct = ct + cdelta;

            // if (loop != LOOP_CNT_MAX - 1) 
            CreateViewMatrix(cp, ct, cameraUp, out _vm);

            ++loop;
        }

        // final center

        CreateViewMatrix(cp, ct, cameraUp, out _vm);

        PerspectiveCenter(pts, size, mm, _vm, pm, cp, ct, out cp, out ct, out bbox);

        //

        fitCameraPos = cp;
        fitCameraTarget = ct;
    }

    /// <summary>
    /// Fit current model view for orthographic projection
    /// </summary>
    /// <param name="pts">model points [local]</param>
    /// <param name="size">size [screen]</param>
    /// <param name="mm">mm [local]</param>
    /// <param name="vm">vm [world]</param>
    /// <param name="pm">pm [eye]</param>
    /// <param name="orthoZoom">orthoZoom [eye]</param>
    /// <param name="near">near [world]</param>
    /// <param name="far">far [world]</param>
    /// <param name="cameraPos">cameraPos [world]</param>
    /// <param name="cameraTarget">cameraTarget [world]</param>
    /// <param name="cameraUp">cameraUp [world]</param>
    /// <param name="fitOrthoZoom">fitOrthoZoom [eye]</param>
    /// <param name="panCameraPos">panCameraPos [world]</param>
    /// <param name="panCameraTarget">panCameraTarget [world]</param>
    public static void OrthoZoomFit(
        IEnumerable<Vector3> pts,
        in Vector2 size,
        in Matrix4x4 mm, in Matrix4x4 vm, in Matrix4x4 pm,
        float orthoZoom, float near, float far,
        Vector3 cameraPos, Vector3 cameraTarget, Vector3 cameraUp,
        out float fitOrthoZoom,
        out Vector3 panCameraPos, out Vector3 panCameraTarget)
    {
        // center

        ComputeSBBox(pts, size, mm, vm, pm, out var sMin, out var sMax);

        var sdx = (sMin.X + sMax.X) / 2 - size.X / 2;
        var sdy = (sMin.Y + sMax.Y) / 2 - size.Y / 2;

        OrthoPan(new Vector2(sdx, sdy), Vector2.Zero,
            size, vm, pm,
            cameraPos, cameraTarget,
            out panCameraPos, out panCameraTarget);

        //

        CreateViewMatrix(panCameraPos, panCameraTarget, cameraUp, out var movedVm);

        // scale

        ComputeEBBox(pts, mm, movedVm, out var eMin, out var eMax);

        var sAr = size.X / size.Y;
        var sBBoxW = sMax.X - sMin.X;
        var sBBoxH = sMax.Y - sMin.Y;

        fitOrthoZoom = 1;

        if (sBBoxW > sBBoxH * sAr)
        {
            var eBBoxW = eMax.X - eMin.X;

            // from CreateOrthogonalProjectionMatrix:
            // size.X * fitOrthoZoom = eBBoxW => fitOrthoZoom = eBBoxW / size.X

            fitOrthoZoom = eBBoxW / size.X;
        }
        else
        {
            var eBBoxH = eMax.Y - eMin.Y;

            // from CreateOrthogonalProjectionMatrix:
            // size.Y * fitOrthoZoom = eBBoxH => fitOrthoZoom = eBBoxH / size.Y

            fitOrthoZoom = eBBoxH / size.Y;
        }
    }

}