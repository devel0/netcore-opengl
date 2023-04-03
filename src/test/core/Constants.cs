namespace test;

public class ConstantsTest
{

    [Fact]
    public void Test1()
    {
        var TOL = TEST_TOL;

        var cs = WCS.Move(new Vector3(1, 2, 3));

        Assert.Equal(DeserializeMatrix4x4("1 0 0 0 0 1 0 0 0 0 1 0 1 2 3 1"), cs);
        Assert.Equal(DeserializeMatrix4x4("1 0 0 0 0 1 0 0 0 0 1 0 0 0 0 1"), WCS);

        Assert.Equal(WCS, MakeWCS());
    }

}
