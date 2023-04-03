namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// Normalize given vector.
    /// </summary>    
    public static Vector3 Normalized(this Vector3 v) => Vector3.Normalize(v);

    /// <summary>
    /// Scale given vector respect given origin by the given scale factor.
    /// </summary>
    /// <param name="v">Vector to scale.</param>
    /// <param name="origin">Reference point.</param>
    /// <param name="factor">Scale factor.</param>
    /// <returns>Scaled vector.</returns>
    public static Vector3 ScaleAbout(this Vector3 v, Vector3 origin, float factor) => origin + (v - origin) * factor;

    /// <summary>
    /// Transform given one within given transformation matrix.
    /// </summary>
    /// <param name="v">Vector to transform.</param>
    /// <param name="transform">Transformation matrix.</param>
    /// <returns>Transformed vector.</returns>
    public static Vector3 Transform(this Vector3 v, Matrix4x4 transform) => Vector3.Transform(v, transform);

    /// <summary>
    /// Transformation given one from wcs toward the given destination coordinate system.
    /// </summary>
    /// <param name="v">Vector to transform.</param>
    /// <param name="cs">Destination coordinate system.</param>
    /// <param name="evalCSOrigin">If true the origin of the coordinate system will be evaluated during transformation ( default: true ).</param>
    /// <returns></returns>
    public static Vector3 ToUCS(this Vector3 v, Matrix4x4 cs, bool evalCSOrigin = true)
    {
        if (evalCSOrigin)
            return v.Transform(cs.Inverse());

        else
            return (v + cs.Origin()).Transform(cs.Inverse());
    }

    /// <summary>
    /// Transformation given one from the coordinate system given toward the wcs.
    /// </summary>
    /// <param name="v">Vector to transform.</param>
    /// <param name="cs">Source coordinate system.</param>
    /// <param name="evalCSOrigin">If true the origin of the coordinate system will be evaluated during transofmration ( default: true ).</param>
    /// <returns></returns>
    public static Vector3 ToWCS(this Vector3 v, Matrix4x4 cs, bool evalCSOrigin = true)
    {
        var res = v.Transform(cs);

        return evalCSOrigin ? res : res - cs.Origin();
    }

    public static string ToString(this Vector3 v, float tol)
    {
        var digits = System.Math.Max(0, -tol.Magnitude());
        return Invariant($"({v.X.MRound(tol).ToString(digits)}, {v.Y.MRound(tol).ToString(digits)}, {v.Z.MRound(tol).ToString(digits)})");
    }

    public static string ToString(this Vector3 v, int digits = 3) =>
        Invariant($"({v.X.ToString(digits)}, {v.Y.ToString(digits)}, {v.Z.ToString(digits)})");

}
