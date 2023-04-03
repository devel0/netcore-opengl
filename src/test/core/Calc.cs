namespace test;

public class TransformTest
{

    [Fact]
    public void Test1()
    {
        var TOL = TEST_TOL;

        TestScene(
            out var om, out var mm, out var vm, out var pm,
            out var camp, out var camt, out var camu,
            out var near, out var far, out var ssize, out var bbox);

        //===================================================================
        // 
        //===================================================================

        var cp = new Vector4(.1f, .2f, .9f, 1);

        var ep = ClipToEye(cp, pm);
        var wp = EyeToWorld(ep, vm);
        var lp = WorldToLocal(wp, mm);
        var op = LocalToObject(lp, om);

        var np = ClipToNDC(cp);
        var sp = NDCToScreen(np, ssize);

        //===================================================================
        // forward
        //===================================================================

        AssertEqualsTol(1e-6f, lp, ObjectToLocal(op, om));
        AssertEqualsTol(1e-7f, wp, LocalToWorld(lp, mm));
        AssertEqualsTol(1e-6f, ep, WordlToEye(wp, vm));
        AssertEqualsTol(1e-5f, cp, EyeToClip(ep, pm));
        AssertEqualsTol(TOL, np, ClipToNDC(cp));
        AssertEqualsTol(TOL, sp, NDCToScreen(np, ssize));

        AssertEqualsTol(1e-6f, ep, LocalToEye(lp, mm * vm));
        AssertEqualsTol(1e-3f, sp, LocalToScreen(lp, ssize, mm * vm * pm));
        AssertEqualsTol(TOL, sp, ClipToScreen(cp, ssize));

        //===================================================================
        // backward
        //===================================================================

        AssertEqualsTol(1e-7f, np with { Z = 0 }, ScreenToNDC_virtual(sp, ssize));
        AssertEqualsTol(TOL, cp, NDCToClip_virtual(np));
        AssertEqualsTol(TOL, ep, ClipToEye(cp, pm));
        AssertEqualsTol(TOL, wp, EyeToWorld(ep, vm));
        AssertEqualsTol(TOL, lp, WorldToLocal(wp, mm));
        AssertEqualsTol(TOL, op, LocalToObject(lp, om));

        AssertEqualsTol(1e-7f, cp with { Z = 0 }, ScreenToClip_virtual(sp, ssize));
        AssertEqualsTol(1e-7f, ep, ScreenToEye_virtual(sp, ssize, pm));
        AssertEqualsTol(1e-6f, lp, ScreenToLocal_virtual(sp, ssize, mm * vm, pm));
        AssertEqualsTol(1e-6f, lp, ClipToLocal(cp, mm * vm, pm));
        AssertEqualsTol(1e-6f, lp, EyeToLocal(ep, mm * vm));

        //===================================================================
        // util
        //===================================================================

        var lray = RayCastLocal(sp, ssize, mm, vm, pm, camp, perspective: true);

        AssertEqualsTol(1e-7f, new Vector3(0, 0, 0), lray.From);
        AssertEqualsTol(TOL, new Vector3(-0.22743326f, -0.49707386f, -0.84002554f), lray.V);

    }

}