namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    /// <summary>
    /// Compute the screen space bounding box (x,y) of all local space coordinates (x,y,z) given.
    /// </summary>
    /// <param name="lPts">Local space coordinates (x,y,z) [local] to include in evaluation of screen space bbox.</param>
    /// <param name="size">Screen size (x,y) [screen].</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="pm">Projection matrix [eye].</param>
    /// <param name="sMin">Computed minimum screen space coordinate (x,y) [screen].</param>
    /// <param name="sMax">Computed maximum screen space coordinate (x,y) [screen].</param>
    /// <param name="lBBox">If given a non null BBox, the corresponding local space bbox [local] will emitted.</param>
    public static void ComputeSBBox(
        IEnumerable<Vector3> lPts,
        in Vector2 size,
        in Matrix4x4 mm, in Matrix4x4 vm, in Matrix4x4 pm,
        out Vector2 sMin, out Vector2 sMax,
        BBox? lBBox = null)
    {
        float sxmin = 0, sxmax = 0, symin = 0, symax = 0;
        bool firstAssign = true;

        var mvpm = mm * vm * pm;

        lBBox?.Clear();

        foreach (var lPt in lPts)
        {
            var s = LocalToScreen(lPt, size, mvpm);

            if (firstAssign)
            {
                sxmin = sxmax = s.X;
                symin = symax = s.Y;

                firstAssign = false;
            }
            else
            {
                if (s.X < sxmin) sxmin = s.X;
                else if (s.X > sxmax) sxmax = s.X;

                if (s.Y < symin) symin = s.Y;
                else if (s.Y > symax) symax = s.Y;
            }

            lBBox?.ApplyUnion(lPt);
        }

        if (firstAssign) throw new Exception($"not points");

        sMin = new Vector2(sxmin, symin);
        sMax = new Vector2(sxmax, symax);
    }

    /// <summary>
    /// Compute the eye space bounding box (x,y,z) of all local space coordinates (x,y,z) given.
    /// </summary>
    /// <param name="lPts">Local space coordinates (x,y,z) [local] to include in evaluation of th eye space bbox.</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="eMin">Computed minimum eye space coordinate (x,y,z) [eye],</param>
    /// <param name="eMax">Computed maximum eye space coordinate (x,y,z) [eye].</param>
    public static void ComputeEBBox(
        IEnumerable<Vector3> lPts,
        in Matrix4x4 mm, in Matrix4x4 vm,
        out Vector2 eMin, out Vector2 eMax)
    {
        float xmin = 0, xmax = 0, ymin = 0, ymax = 0;
        bool firstAssign = true;

        var mvm = mm * vm;

        foreach (var lPt in lPts)
        {
            var e = LocalToEye(lPt, mvm);

            if (firstAssign)
            {
                xmin = xmax = e.X;
                ymin = ymax = e.Y;

                firstAssign = false;
            }
            else
            {
                if (e.X < xmin) xmin = e.X;
                else if (e.X > xmax) xmax = e.X;

                if (e.Y < ymin) ymin = e.Y;
                else if (e.Y > ymax) ymax = e.Y;
            }
        }

        if (firstAssign) throw new Exception($"not points");

        eMin = new Vector2(xmin, ymin);
        eMax = new Vector2(xmax, ymax);
    }

    /// <summary>
    /// Compute best fitting near and far values based on local space bbox given.
    /// REVIEW 
    /// </summary>
    /// <param name="bbox">Local space bbox of model for which compute best near, far [local].</param>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="cameraPos">Camera position [world].</param>
    /// <param name="near">Computed absolute position along [eye] z axis of the near clipping plane.</param>
    /// <param name="far">Computed absolute position along [eye] z axis of the far clipping plane.</param>
    public static void PreventZFighting(
        BBox bbox,
        in Matrix4x4 mm, in Matrix4x4 vm,
        in Vector3 cameraPos,
        out float near, out float far
    )
    {
        // https://learnopengl.com/Advanced-OpenGL/Depth-testing

        // prevent z-fighting by set near and far away from the model

        var mvm = mm * vm;

        var q = bbox.Points.Select(lpt => LocalToEye(lpt, mvm)).OrderBy(ept => ept.Z).ToList();

        var eCameraPos = WordlToEye(cameraPos, vm);

        //                 
        //                     eF      eN             eC
        //            F        |   ct  |          N   cp             eZ
        //    --------+--------XXXXXXXXX----------+---+-------------->
        //                         eS
        //

        // z eye point to observer
        var eC = eCameraPos.Z;
        var eN = q.Last().Z;
        var eF = q.First().Z;
        var eS = eN - eF;

        if (eF < eC)
        {
            var qNear = Abs(eC - eN) * 1e-2f;

            if (qNear < DEFAULT_Near && bbox.Size.Max() > 100)
            {
                near = 10 * DEFAULT_Near;
                far = DEFAULT_Far;
            }

            else
            {
                near = qNear;
                far = 3f * (eC - eF);
            }

            if (far < near)
            {
                far = 2f * near;
            }            
        }
        else
        {
            near = DEFAULT_Near;
            far = DEFAULT_Far;
        }
    }

    /// <summary>
    /// Deserialize to a Matrix4x4 from a string of 16 float space separated ( invariant ).<br/>    
    /// </summary>
    /// <param name="txt">Serialized Matrix4x4 text input.</param>    
    /// <seealso cref="SearchAThing.OpenGL.Core.Toolkit.Serialize"/>
    public static Matrix4x4 DeserializeMatrix4x4(string txt)
    {
        var ss = txt.Split(' ');

        if (ss.Length != 16) throw new ArgumentException($"invalid matrix ( must 16 float space separated invariant locale )");

        var v = ss.Select(w => float.Parse(w, CultureInfo.InvariantCulture)).ToArray();

        return new Matrix4x4(
            v[0 * 4 + 0], v[0 * 4 + 1], v[0 * 4 + 2], v[0 * 4 + 3],
            v[1 * 4 + 0], v[1 * 4 + 1], v[1 * 4 + 2], v[1 * 4 + 3],
            v[2 * 4 + 0], v[2 * 4 + 1], v[2 * 4 + 2], v[2 * 4 + 3],
            v[3 * 4 + 0], v[3 * 4 + 1], v[3 * 4 + 2], v[3 * 4 + 3]);
    }

    /// <summary>
    /// Serialize to a string of 16 float space separated ( invariant ).
    /// </summary>
    /// <param name="m"></param>
    /// <see cref="SearchAThing.OpenGL.Core.Toolkit.DeserializeMatrix4x4"/>
    public static string Serialize(Matrix4x4 m) =>
        Invariant($"{m.M11} {m.M12} {m.M13} {m.M14} ") +
        Invariant($"{m.M21} {m.M22} {m.M23} {m.M24} ") +
        Invariant($"{m.M31} {m.M32} {m.M33} {m.M34} ") +
        Invariant($"{m.M41} {m.M42} {m.M43} {m.M44}");

}