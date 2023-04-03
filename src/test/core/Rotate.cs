namespace test;

public class RotateTest
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

        CameraRotate(
            sAngleXRad: (float)PI / 100, sAngleYRad: (float)PI / 150,
            camp, camt, camu,
            out var rotCamt, out var rotCamu);

        AssertEqualsTol(tol, new Vector3(-0.079924464f, 0.084935665f, -0.026936531f), rotCamt);
        AssertEqualsTol(tol, new Vector3(0.008394879f, 0.99950653f, 0.025184635f), rotCamu);
    }

}