namespace test;

public class BBoxTest
{
    [Fact]
    public void Test1()
    {
        var bbox = new BBox();

        var p1 = new Vector3(1.2f, 2.1f, 3.3f);
        var p2 = new Vector3(1.0f, 2.2f, 3.0f);

        bbox.ApplyUnion(p1, p2);

        Assert.Equal(new Vector3(1.0f, 2.1f, 3.0f), bbox.Min);
        Assert.Equal(new Vector3(1.2f, 2.2f, 3.3f), bbox.Max);
    }

    [Fact]
    public void Test2()
    {
        var tol = 1e-4f;

        var cubePts = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(456.7627f, 132.6665f, 142.7004f),
            new Vector3(456.7627f, -121.8739f, 379.3429f),
            new Vector3(0, -254.5404f, 236.6424f),

            new Vector3(-159.384f, 254.4057f, 273.6472f),
            new Vector3(297.3787f, 387.0722f,  416.3476f),
            new Vector3(297.3787f, 132.5318f, 652.9900f),
            new Vector3(-159.3840f, -0.1347f, 510.2896f),
        };

        var bbox = new BBox(cubePts);

        Assert.Equal(new Vector3(-159.384f, -254.5404f, 0f), bbox.Min);
        Assert.Equal(new Vector3(456.7627f, 387.0722f, 652.99f), bbox.Max);

        var csCubeAligned = MakeCS(
            origin: new Vector3(0, 0, 0),
            baseX: new Vector3(456.7627f, 132.6665f, 142.7004f).Normalized(),
            baseY: new Vector3(-159.384f, 254.4057f, 273.6472f).Normalized());

        var bboxAligned = new BBox(csCubeAligned, cubePts);

        AssertEqualsTol(tol, Vector3.Zero, bboxAligned.Min);
        AssertEqualsTol(tol, new Vector3(297.37857f, 132.53168f, 652.9901f), bboxAligned.Max);
    }
}