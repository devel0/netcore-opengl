namespace test;

public class GLMatrixesTest
{
    [Fact]
    public void Test1()
    {
        var tol = TEST_TOL;

        {
            var m = new Matrix4x4(
                1, 2, 3, 4,
                11, 22, 33, 44,
                111, 222, 333, 444,
                1111, 2222, 3333, 4444);

            var q = Serialize(m);

            Assert.Equal("1 2 3 4 11 22 33 44 111 222 333 444 1111 2222 3333 4444", q);

            Assert.Equal(m, DeserializeMatrix4x4(q));
        }

        CreateModelMatrix(
            scale: new Vector3(.1f, .2f, .3f),
            rotation: Quaternion.CreateFromAxisAngle(new Vector3(1, 2, 3), (float)PI / 100),
            translation: new Vector3(1, 2, 3),
            out var mm);

        Assert.Equal(
            DeserializeMatrix4x4("0.09935853 0.009521916 -0.0061341203 0 -0.018649079 0.19901313 0.0068742796 0 0.019290553 -0.008535038 0.29925984 0 1 2 3 1"),
            mm);

        CreateViewMatrix(
            cameraPos: new Vector3(1, 2, 3),
            cameraTarget: new Vector3(10, 50, 20),
            cameraUp: Vector3.Normalize(new Vector3(4, 2, 0.5f)),
            out var vm);

        Assert.Equal(
            DeserializeMatrix4x4("-0.053909924 -0.98326087 -0.1740451 0 0.342328 0.14553699 -0.92824054 0 -0.9380326 0.10962189 -0.32875186 0 2.1833518 0.36332121 3.0167818 1"),
            vm);

        CreateOrthogonalProjectionMatrix(
            size: new Vector2(1024, 768),
            eOrthoZoom: 5, near: 1, far: 100, vm, out var opm);

        Assert.Equal(
            DeserializeMatrix4x4("0.000390625 0 0 0 0 0.00052083336 0 0 0 0 -0.01010101 0 0 0 -0.01010101 1"),
            opm);

        CreatePerspectiveProjectionMatrix(fovDeg: 28, ar: 16f / 9, near: 1f, far: 100f, out var ppm);
        Assert.Equal(
            DeserializeMatrix4x4("2.2560642 0 0 0 0 4.010781 0 0 0 0 -1.010101 -1 0 0 -1.010101 0"),
            ppm);
    }

    [Fact]
    public void Test2()
    {
        var tol = 1e-7f;
        var pt = new Vector3(1, 0, 0);

        var SC = new Vector3(2, 3, 4);
        var RT = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)30d.ToRad());
        var TR = new Vector3(1, 0, 0);

        var m_s = Matrix4x4.CreateScale(SC);
        var m_t = Matrix4x4.CreateFromQuaternion(RT);
        var m_r = Matrix4x4.CreateTranslation(TR);

        var mSRT = m_s * m_t * m_r;

        var pt_s = Vector3.Transform(pt, m_s);
        AssertEqualsTol(tol, new Vector3(2, 0, 0), pt_s);

        var pt_r = Vector3.Transform(pt, m_t);
        AssertEqualsTol(tol, new Vector3(.8660254f, .5f, 0), pt_r);

        var pt_t = Vector3.Transform(pt, m_r);
        AssertEqualsTol(tol, pt + TR, pt_t);

        var pt_sr = Vector3.Transform(Vector3.Transform(pt, m_s), m_t);
        AssertEqualsTol(tol, new Vector3(1.7320508f, 1, 0), pt_sr);

        var pt_srt = Vector3.Transform(Vector3.Transform(Vector3.Transform(pt, m_s), m_t), m_r);
        AssertEqualsTol(tol, pt_sr + TR, pt_srt);

        var pt_srt_ = Vector3.Transform(pt, mSRT);
        AssertEqualsTol(tol, pt_srt, pt_srt_);

        CreateModelMatrix(SC, RT, TR, out var mm);
        var pt_srt__ = Vector3.Transform(pt, mm);
        AssertEqualsTol(tol, pt_srt, pt_srt__);
    }

}