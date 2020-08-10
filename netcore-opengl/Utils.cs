using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia.Media;

namespace SearchAThing
{

    public static partial class OpenGlExt
    {

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
        /// convert given Vector4 rgba into Color type
        /// </summary>
        /// <param name="rgba">color input as Vector4 rgba</param>
        /// <returns>equivalent color of Color type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Drawing.Color ToSystemDrawingColor(this Vector4 rgba) =>
            System.Drawing.Color.FromArgb((byte)(rgba.X * 255f), (byte)(rgba.Y * 255f), (byte)(rgba.Z * 255f), (byte)(rgba.W * 255f));

        /// <summary>
        /// convert avalonia color to system.drawing.color
        /// </summary>
        /// <param name="color">avalonia color</param>
        /// <returns>system drawing color</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Drawing.Color ToSystemDrawingColor(this Color color) =>
            System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);

    }

    public static partial class OpenGlToolkit
    {

        /// <summary>
        /// create a random color between given min,max values as Vector3(0..1, 0..1, 0..1);
        /// </summary>
        /// <param name="min">min color value (0..1)</param>
        /// <param name="max">max color value (0..1)</param>
        /// <returns>color Vector3</returns>
        public static Vector3 RndColor(double min = 0, double max = 1)
        {
            var rnd = new Random();
            float rndColor()
            {
                var r = rnd.NextDouble();
                var res = (float)(min + (max - min) * r);

                return res;
            }

            return new Vector3(rndColor(), rndColor(), rndColor());
        }

    }

}