namespace test;

public class LineTest
{
    [Fact]
    public void Test1()
    {
        var ln = Line.FromTo(
            new Vector3(-1170.44732601f, 56.8487018f, 107.33511396f),
            new Vector3(-1162.08423312f, -1383.31979866f, 4895.42917621f));

        var li = Line.FromTo(
            new Vector3(1735.49082563f, 1219.83250032f, 0),
            new Vector3(1053.00779331f, 1310.63166144f, 1219.08724723f));

        var plane = MakeCS(ln.From, Vector3.Normalize(ln.V));

        var ip = li.Intersect(plane);

        Assert.NotNull(ip);

        AssertEqualsTol(1e-4f, new Vector3(1476.353f, 1254.3088f, 462.88556f), ip.Value);

        var q = ip.Value.ToUCS(plane);

        AssertEqualsTol(1e-4f, 0, q.Z);

    }

    [Fact]
    public void Test2()
    {
        var line = Line.FromTo(
            new Vector3(-7070.68194984f, -2641.19835418f, -2576.00295896f),
            new Vector3(9888.19787171f, 6913.09108639f, 3953.11622307f));

        Assert.NotNull(line.Contains(1e-3f, line.MidPoint, out var _));
        Assert.Null(line.Contains(1e-3f, line.MidPoint + 1e-2f * Vector3.UnitX, out var _));
        Assert.Null(line.Contains(1e-3f, line.MidPoint + 1e-2f * Vector3.UnitY, out var _));
        Assert.Null(line.Contains(1e-3f, line.MidPoint + 1e-2f * Vector3.UnitZ, out var _));
    }

}