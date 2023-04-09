namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// Convert given double to float ensuring not exceed the float range.
    /// </summary>
    /// <param name="d">Double which convert to float.</param>
    /// <returns>Float of the given double.</returns>
    public static float ToFloat(this double d)
    {
        if (d < float.MinValue || d > float.MaxValue)
            throw new Exception($"double [{d}] out of bounds the float range [{float.MinValue},{float.MaxValue}]");

        return (float)d;
    }

    /// <summary>
    /// Invariant representation X Y Z
    /// </summary>
    public static string Fmt(this in Vector3 v) => $"{v.X} {v.Y} {v.Z}";

}