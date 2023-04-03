namespace test;

public class PanTest
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
            OrthoPan(
                from: new Vector2(10, 10), to: new Vector2(100, 100),
                ssize, vm, pm, camp, camt,
                out var panCamp, out var panCamt);

            AssertEqualsTol(tol, new Vector3(0.93590146f, 1.950548f, 3.0543342f), panCamp);
            AssertEqualsTol(tol, new Vector3(-0.06409854f, -0.049451947f, 0.054334164f), panCamt);
        }

        //

        {
            PerspectivePan(
                refPt: bbox.Middle,
                from: new Vector2(10, 10), to: new Vector2(50, 50),
                ssize, mm, vm, pm, camp, camt,
                out var panCamp, out var panCamt);

            AssertEqualsTol(tol, new Vector3(0.8934066f, 1.9177636f, 3.0903554f), panCamp);
            AssertEqualsTol(tol, new Vector3(-0.10659338f, -0.082236454f, 0.090355285f), panCamt);
        }
    }

}