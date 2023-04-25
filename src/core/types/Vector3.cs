namespace SearchAThing.OpenGL.Core;

public static partial class Toolkit
{

    /// <summary>
    /// Depending on given pointCnt argument make an invariant parse:
    /// - (pointCnt=1) p1x,p1y,p1z;...
    /// - (pointCnt=2) p1x,p1y,p1z,p2x,p2y,p2z;...
    /// - (pointCnt=3) p1x,p1y,p1z,p2x,p2y,p2z,p3x,p3y,p3z;...
    /// </summary>
    public static IEnumerable<Vector3> ParseVector3(string vstr, int pointCnt = 1)
    {
        var ss = vstr.Split(SIMPLE_CMD_POINT_SEP);

        foreach (var s in ss)
        {
            var sss = s.Split(SIMPLE_CMD_COORD_SEP);

            if (sss.Length % 3 != 0) throw new ArgumentException($"invalid coord (must multiple 3) is {sss.Length} cnt");

            for (int i = 0; i < sss.Length; i += 3)
            {
                var x = float.Parse(sss[i]);
                var y = float.Parse(sss[i+1]);
                var z = float.Parse(sss[i+2]);

                yield return new Vector3(x, y, z);
            }
        }
    }

}

public static partial class Ext
{


    /// <summary>
    /// Invariant representation X Y Z
    /// </summary>
    public static string Fmt(this in Vector3 v) => $"{v.X} {v.Y} {v.Z}";

}