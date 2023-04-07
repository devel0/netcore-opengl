namespace SearchAThing.OpenGL.Render;

/// <summary>
/// Helper class to store model/view/projection and camera pos/target/up in a single object.
/// </summary>
public class GLMatrixes
{

    /// <summary>
    /// Model matrix [local].
    /// </summary>
    public Matrix4x4 mm { get; private set; }

    /// <summary>
    /// View matrix [world].
    /// </summary>    
    public Matrix4x4 vm { get; private set; }

    /// <summary>
    /// Projection matrix [eye].
    /// </summary>    
    public Matrix4x4 pm { get; private set; }

    /// <summary>
    /// Camera position [world].
    /// </summary>
    public Vector3 cp { get; private set; }

    /// <summary>
    /// Camera target [world].
    /// </summary>
    public Vector3 ct { get; private set; }

    /// <summary>
    /// Camera up [world].
    /// </summary>    
    public Vector3 cu { get; private set; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="mm">Model matrix [local].</param>
    /// <param name="vm">View matrix [world].</param>
    /// <param name="pm">Projection matrix [eye].</param>
    /// <param name="cp">Camera position [world].</param>
    /// <param name="ct">Camera target [world].</param>
    /// <param name="cu">Camera up [world].</param>
    public GLMatrixes(in Matrix4x4 mm, in Matrix4x4 vm, in Matrix4x4 pm, in Vector3 cp, in Vector3 ct, in Vector3 cu)
    {
        this.mm = mm;
        this.vm = vm;
        this.pm = pm;

        this.cp = cp;
        this.ct = ct;
        this.cu = cu;
    }

}
