namespace test;

public class ZoomTest
{
    [Fact]
    public void Test1()
    {
        var tol = TEST_TOL;

        TestScene(
            out var om, out var mm, out var vm, out var pm,
            out var camp, out var camt, out var camu,
            out var near, out var far, out var ssize, out var bbox);

        var p = new Vector3(1, 2, 3);

        {
            PerspectiveZoom(
                scale: .1f, bbox,
                mm, vm, camp, camt,
                out var scaledCamp, out var scaledCamt);

            AssertEqualsTol(tol, new Vector3(0.9f, 1.8000001f, 2.7f), scaledCamp);
            AssertEqualsTol(tol, new Vector3(-0.10000002f, -0.19999997f, -0.30000004f), scaledCamt);
        }

        {
            OrthoZoomFit(
                bbox.Points,
                ssize, mm, vm, pm,
                orthoZoom: 1f, near, far,
                camp, camt, camu,
                out var fitOrthoZoom, out var panCamp, out var panCamt);

            AssertEqualsTol(tol, 0.008742874f, fitOrthoZoom);
            AssertEqualsTol(tol, new Vector3(-0.48148966f, 1.6211908f, 3.7463694f), panCamp);
            AssertEqualsTol(tol, new Vector3(-1.4814897f, -0.3788092f, 0.74636936f), panCamt);
        }

        {
            OrthoZoomFit(
                new BBox(
                    new Vector3(-5, -10, 0),
                    new Vector3(5, 10, 5)
                ).Points,
                size: new Vector2(1096, 596),

                mm: DeserializeMatrix4x4("-4.8428774E-08 -1.0000006 0 0 1.0000006 -4.8428774E-08 0 0 0 0 1 0 0 0 -2.5 1"),
                vm: DeserializeMatrix4x4("1 0 0 0 0 -1 0 0 0 0 1 0 0.5004883 0.5008545 -1 1"),
                pm: DeserializeMatrix4x4("0.054379564 0 0 0 0 0.1 0 0 0 0 -0.010257817 0 0 0 -0.003007473 1"),
                orthoZoom: 0.033557046f,
                near: 0.29318842f,
                far: 97.77982f,
                cameraPos: new Vector3(-0.5004883f, 0.5008545f, 1),
                cameraTarget: new Vector3(-0.5004883f, 0.5008545f, 0),
                cameraUp: new Vector3(0, 1, 0),
                out var fitOrthoZoom, out var panCamp, out var panCamt);

            AssertEqualsTol(tol, 0.018248186f, fitOrthoZoom);
            AssertEqualsTol(tol, new Vector3(-0.016794205f, 0.016806602f, 1), panCamp);
            AssertEqualsTol(tol, new Vector3(-0.016794205f, 0.016806602f, 0), panCamt);
        }

        {
            OrthoZoom(scale: 2, orthoZoom: 1.4f, out var scaledSOrthoZoom);

            AssertEqualsTol(tol, -1.4f, scaledSOrthoZoom);
        }

    }

    [Fact]
    public void Test3()
    {
        var screenSize = new Vector2(640, 480);
        var fovDeg = DEFAULT_FovDeg;

        var mm = Matrix4x4.Identity;
        var cp = new Vector3(-0.0013006479f, 0.0013013184f, 5f);
        var ct = new Vector3(-0.0013006479f, 0.0013013184f, 4f);
        var cu = DEFAULT_CAMERA_UP;
        var lbbox = new BBox();

        var lpts = new[] { Vector3.Zero, new Vector3(0, 0, 5) };

        CreateViewMatrix(cp, ct, cu, out var vm);

        CreatePerspectiveProjectionMatrix(DEFAULT_FovDeg,
            screenSize.X / screenSize.Y,
            DEFAULT_Near, DEFAULT_Far, out var pm);

        //

        ComputeSBBox(lpts, screenSize, mm, vm, pm, out var sMin, out var sMax, lbbox);

        AssertEqualsTol(1e-4f, Vector3.Zero, lbbox.Min);
        AssertEqualsTol(1e-4f, lpts[1], lbbox.Max);

        AssertNotNaN(sMin);
        AssertNotNaN(sMax);

        //

        PerspectiveCenter(lpts, screenSize, mm, vm, pm, cp, ct,
            out var newCp, out var newCt, out lbbox);

        Assert.False(float.IsNaN(newCp.X));
        Assert.False(float.IsNaN(newCp.Y));
        Assert.False(float.IsNaN(newCp.Z));

        Assert.False(float.IsNaN(newCt.X));
        Assert.False(float.IsNaN(newCt.Y));
        Assert.False(float.IsNaN(newCt.Z));

        PerspectiveZoomFit(
            lpts,
            screenSize,
            mm, vm, pm,
            cp, ct, cu,
            fovDeg,
            out var fitCameraPos,
            out var fitCameraTarget);

        Assert.False(float.IsNaN(fitCameraPos.X));
        Assert.False(float.IsNaN(fitCameraPos.Y));
        Assert.False(float.IsNaN(fitCameraPos.Z));

        Assert.False(float.IsNaN(fitCameraTarget.X));
        Assert.False(float.IsNaN(fitCameraTarget.Y));
        Assert.False(float.IsNaN(fitCameraTarget.Z));

        PreventZFighting(lbbox, mm, vm, fitCameraPos, out var near, out var far);
        Assert.False(near == 0f);

        ;
    }

}