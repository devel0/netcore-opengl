namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// States if given number equals (with tolerance) to the given one.
    /// </summary>    
    public static bool EqualsTol(this float x, float tol, float y) => Math.Abs(x - y) <= tol;

    /// <summary>
    /// States if given number great than (with tolerance) to the given one.
    /// </summary>    
    public static bool GreatThanTol(this float x, float tol, float y) => x > y && !x.EqualsTol(tol, y);

    /// <summary>
    /// States if given number great than or equals (with tolerance) to the given one.
    /// </summary>    
    public static bool GreatThanOrEqualsTol(this float x, float tol, float y) => x > y || x.EqualsTol(tol, y);

    /// <summary>
    /// States if given number less than (with tolerance) to the given one.
    /// </summary>    
    public static bool LessThanTol(this float x, float tol, float y) => x < y && !x.EqualsTol(tol, y);

    /// <summary>
    /// States if given number less than or equals (with tolerance) to the given one.
    /// </summary>    
    public static bool LessThanOrEqualsTol(this float x, float tol, float y) => x < y || x.EqualsTol(tol, y);

    /// <summary>
    /// States if given vector2 equals (with tolerance) to the given one.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsTol(this in Vector2 v, float tol, in Vector2? other)
    {
        if (other is null) return false;

        return
            v.X.EqualsTol(tol, other.Value.X) &&
            v.Y.EqualsTol(tol, other.Value.Y);
    }

    /// <summary>
    /// States if given vector3 equals (with tolerance) to the given one.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsTol(this in Vector3 v, float tol, in Vector3? other)
    {
        if (other is null) return false;

        return
            v.X.EqualsTol(tol, other.Value.X) &&
            v.Y.EqualsTol(tol, other.Value.Y) &&
            v.Z.EqualsTol(tol, other.Value.Z);
    }

    /// <summary>
    /// States if given vector4 equals (with tolerance) to the given one.
    /// </summary>    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsTol(this in Vector4 v, float tol, in Vector4? other)
    {
        if (other is null) return false;

        return
            v.X.EqualsTol(tol, other.Value.X) &&
            v.Y.EqualsTol(tol, other.Value.Y) &&
            v.Z.EqualsTol(tol, other.Value.Z) &&
            v.W.EqualsTol(tol, other.Value.W);
    }

    /// <summary>
    /// Retrieve the maxest component between x, y, z of given vector3.
    /// </summary>    
    public static float Max(this Vector3 v) => System.Math.Max(v.X, System.Math.Max(v.Y, v.Z));

    /// <summary>
    /// Retrieve the magnitude of given number.
    /// </summary>    
    public static int Magnitude(this float value) => ((double)value).Magnitude();

    /// <summary>
    /// Round given number using the multiple basis.
    /// </summary>    
    public static float MRound(this float value, float multiple) => (float)((double)value).MRound(multiple);

    public static string ToString(this float d, int significantDigits)
    {
        var decfmt = "#".Repeat(significantDigits);
        return string.Format(CultureInfo.InvariantCulture, "{0:0." + decfmt + "}", d);
    }

}

public static partial class Constants
{

    /// <summary>
    /// Tolerance to be used in comparision of normalized vectors.
    /// </summary>
    public const float NormalizedTolerance = 1e-6f;

    /// <summary>
    /// Constant used in <see cref="ArbitraryAxisAlghorithm"/>.
    /// </summary>
    public const float aaaSmall = 1.0f / 64;
}