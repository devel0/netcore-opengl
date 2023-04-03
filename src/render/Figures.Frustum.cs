namespace SearchAThing.OpenGL.Render;

public static partial class Toolkit
{

    /// <summary>
    /// Generate a gl figure that represents the camera frustum object.
    /// </summary>    
    public static List<GLFigureBase> MakeFrustum(this GLControl glControl) =>
        MakeFrustum(glControl.CameraPos, glControl.CameraTarget, glControl.CameraUp,
            glControl.Near, glControl.Far, glControl.FovDeg.ToRad(), glControl.ModelMatrix);

    /// <summary>
    /// Generate a gl figure that represent the camera frustum object.
    /// </summary>
    /// <param name="cameraPos">Camera position [world].</param>
    /// <param name="cameraTarget">Camera target [world].</param>
    /// <param name="cameraUp">Camera up [world].</param>
    /// <param name="near">Near plane [world].</param>
    /// <param name="far">Far plane [world].</param>
    /// <param name="fov">Field of view (rad).</param>
    /// <param name="mm">Model matrix [world].</param>
    /// <returns>frustum figures</returns>
    public static List<GLFigureBase> MakeFrustum(Vector3 cameraPos, Vector3 cameraTarget, Vector3 cameraUp,
        float near, float far, float fov,
        Matrix4x4 mm)
    {
        var figures = new List<GLFigureBase>();

        var cp = cameraPos;
        var ct = cameraTarget;
        var cu = cameraUp;        

        var lcp = WorldToLocal(cp, mm);
        var lct = WorldToLocal(ct, mm);
        var lfw = Vector3.Normalize(lct - lcp);
        var lcu = Vector3.Normalize(WorldToLocal(cu, mm) - WorldToLocal(Vector3.Zero, mm));
        var lcs = MakeCCS(lcp, lct, lcu);

        var lines = new List<GLLine>();

        var N = lcp + lfw * near;
        var F = lcp + lfw * far;

        lines.Add(GLLine.FromTo(N, F));

        var csx = lcs.BaseX();
        var csy = lcs.BaseY();

        var nfov = near * (float)Sin(fov / 2);
        var Nlt = N - csx * nfov + csy * nfov;
        var Nrt = N + csx * nfov + csy * nfov;
        var Nlb = N - csx * nfov - csy * nfov;
        var Nrb = N + csx * nfov - csy * nfov;

        var ffov = far * (float)Sin(fov / 2);
        var Flt = F - csx * ffov + csy * ffov;
        var Frt = F + csx * ffov + csy * ffov;
        var Flb = F - csx * ffov - csy * ffov;
        var Frb = F + csx * ffov - csy * ffov;

        lines.Add(GLLine.FromTo(Nlt, Flt));
        lines.Add(GLLine.FromTo(Nrt, Frt));
        lines.Add(GLLine.FromTo(Nlb, Flb));
        lines.Add(GLLine.FromTo(Nrb, Frb));

        lines.Add(GLLine.FromTo(Nlt, Nrt));
        lines.Add(GLLine.FromTo(Nrt, Nrb));
        lines.Add(GLLine.FromTo(Nrb, Nlb));
        lines.Add(GLLine.FromTo(Nlb, Nlt));

        lines.Add(GLLine.FromTo(Flt, Frt));
        lines.Add(GLLine.FromTo(Frt, Frb));
        lines.Add(GLLine.FromTo(Frb, Flb));
        lines.Add(GLLine.FromTo(Flb, Flt));

        figures.Add(new GLLineFigure(lines).Act(w => w.SetColor(DEFAULT_CameraObjectColor)));

        figures.Add(new GLPointFigure(new GLPoint(lcp, DEFAULT_CameraObjectColor)));

        return figures;
    }

}
