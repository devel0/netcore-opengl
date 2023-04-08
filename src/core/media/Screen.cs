namespace SearchAThing.OpenGL.Core;

public static partial class Ext
{

    /// <summary>
    /// Compute aspect ratio of given <see cref="System.Drawing.Size"/> object.<br/>    
    /// </summary>    
    /// <remarks>
    /// If size height is zero returns 1, width / height elsewhere.
    /// </remarks>
    public static float AspectRatio(this in Size size) =>
        size.Height == 0 ? 1 : (float)size.Width / (float)size.Height;

}