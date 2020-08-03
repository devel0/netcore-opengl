using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Media;

namespace SearchAThing
{

    public static partial class SciExt
    {

        /// <summary>
        /// create vector3 from vector4 discarding w
        /// </summary>
        /// <param name="v">vector4 input</param>
        /// <returns>vector3</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVector3(this Vector4 v) => new Vector3(v.X, v.Y, v.Z);

        /// <summary>
        /// convert color to vec4 (r,g,b,a)
        /// </summary>
        /// <param name="color">color</param>
        /// <param name="alpha">override default color alpha</param>
        /// <returns>vec4 (r,g,b,a)</returns>
        public static Vector4 ToVector4(this Color color, float? alpha = null) => new Vector4(
            (float)color.R / 255f,
            (float)color.G / 255f,
            (float)color.B / 255f,
            alpha.HasValue ? alpha.Value : (float)color.A / 255f);


        /// <summary>
        /// convert avalonia color to system.drawing.color
        /// </summary>
        /// <param name="color">avalonia color</param>
        /// <returns>system drawing color</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Drawing.Color ToSystemDrawingColor(this Color color) =>
            System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

    }

}