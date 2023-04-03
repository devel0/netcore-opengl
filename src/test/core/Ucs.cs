namespace test;

public class UcsTest
{
    [Fact]
    public void Test1()
    {
        var tol = 1e-3f;

        var csOrigin = new Vector3(1, 2, 3);
        var csNormal = Vector3.UnitX;

        var cs = MakeCS(csOrigin, csNormal);

        Assert.Equal(csOrigin, cs.Origin());
        Assert.Equal(Vector3.UnitY, cs.BaseX());
        Assert.Equal(Vector3.UnitZ, cs.BaseY());
        Assert.Equal(csNormal, cs.BaseZ());

        //

        var p = new Vector3(3.3f, 4.1f, 1.0f);
        var p_ = p.ToUCS(cs);

        AssertEqualsTol(tol, new Vector3(2.1f, -2f, 2.3f), p_);
        AssertEqualsTol(tol, p_.ToWCS(cs), p);
    }

}