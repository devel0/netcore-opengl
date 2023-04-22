namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Debug object containing a vertex manager to allow create figures and
/// a gl control to allow invalidate the view.<br/>
/// Used for debug purpose.
/// </summary>
public class DebugVtxMgr
{
    public GLVertexManager VtxMgr { get; private set; }

    public GLControl GLControl { get; private set; }

    public void Invalidate() => GLControl.InvalidateAll();

    public DebugVtxMgr(GLVertexManager vtxMgr, GLControl glControl)
    {
        VtxMgr = vtxMgr;
        GLControl = glControl;
    }
}
