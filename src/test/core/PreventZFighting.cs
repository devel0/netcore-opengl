namespace test;

public class PreventZFightingTest
{

    [Fact]
    public void Test1()
    {
        PreventZFighting(
            new BBox(Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ),
            DeserializeMatrix4x4("1 0 0 0 0 1 0 0 0 0 1 0 -0.5 -0.5 -0.5 1"),
            DeserializeMatrix4x4("1 0 0 0 0 -1 0 0 0 0 1 0 0.00065728877 0.0006576994 -0.4642535 1"),
            cameraPos: new Vector3(-0.00065728877f, 0.0006576994f, 0.4642535f),
            out var near, out var far);

        Assert.False(near < 0);
    }

}