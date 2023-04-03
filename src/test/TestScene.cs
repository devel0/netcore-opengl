namespace test;

public static partial class Toolkit
{

    public const float TEST_TOL = 1e-30f;

    public static void TestScene(
        out Matrix4x4 om, out Matrix4x4 mm, out Matrix4x4 vm, out Matrix4x4 pm,
        out Vector3 camp, out Vector3 camt, out Vector3 camu,
        out float near, out float far,
        out Vector2 ssize, out BBox bbox
    )
    {
        CreateModelMatrix(Vector3.One,
            Quaternion.CreateFromAxisAngle(new Vector3(1, 2, 3), (float)(PI / 100)),
            new Vector3(1, 2, 3), out mm);

        camp = new Vector3(1, 2, 3);
        camt = Vector3.Zero;
        camu = Vector3.UnitY;

        vm = Matrix4x4.CreateLookAt(camp, camt, camu);

        var _p = new Vector3(1, 2, 3);

        bbox = new BBox(
            _p,
            _p + new Vector3(1, 2, 3),
            _p - new Vector3(1, 2, 3));

        PreventZFighting(bbox, mm, vm, camp, out near, out far);

        CreatePerspectiveProjectionMatrix(DEFAULT_FovDeg, 16f / 9, near, far, out pm);

        om =
            Matrix4x4.CreateRotationZ((float)(PI / 100))
            *
            Matrix4x4.CreateTranslation(new Vector3(1, 2, 3));

        ssize = new Vector2(1024, 768);
    }

}