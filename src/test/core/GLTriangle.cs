namespace test;

public class GLTriangleTest
{
    [Fact]
    public void Test1()
    {
        var tol = 1e-5f;

        var t1 = new GLTriangle(
            new Vector3(45.823437f, -4.1765623f, 3.3747683f),
            new Vector3(45.823437f, -6.038086f, 2.529329f),
            new Vector3(47.83262f, -6.038086f, 3.2898018f));

        var t2 = new GLTriangle(
            new Vector3(46.25f, -5, 3.015f),
            new Vector3(46.25f, -6.25f, 3.01875f),
            new Vector3(47.5f, -6.25f, 3.0125f));

        var q = t1.Intersect(tol, t2, out var skipped);
        Assert.False(skipped);

        Assert.NotNull(q);
        AssertEqualsTol(tol, q.From, new Vector3(46.25f, -5.322083f, 3.0159664f));
        AssertEqualsTol(tol, q.To, new Vector3(47.10354f, -6.038086f, 3.0138466f));
    }

}
