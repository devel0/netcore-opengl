namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Mouse wheel changes are handlded through <see cref="GLControl.CameraZoomIn"/>
/// and <see cref="GLControl.CameraZoomOut"/>.<br/>
/// Mouse middle button press can start pan through <see cref="GLControl.Pan"/>.<br/>
/// Mouse middle double click activate <see cref="GLControl.ZoomFit"/>.<br/>
/// Mouse left pointer start model rotate through <see cref="GLControl.ModelRotate"/>.<br/>
/// </summary>
public partial class AvaloniaGLControl
{

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if (e.Handled) return;

        if (e.Delta.Y < 0)
            GLControl.CameraZoomOut();
        else
            GLControl.CameraZoomIn();
    }

    MouseStart? PanStart = null;

    MouseStart? _OrbitStart = null;
    MouseStart? OrbitStart
    {
        get => _OrbitStart;
        set
        {
            _OrbitStart = value;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled) return;

        var cp = e.GetCurrentPoint(this);
        var cp_pos = cp.Position.ToVector2();

        if (cp.Properties.IsMiddleButtonPressed)
        {
            if (e.ClickCount == 2)
            {
                GLControl.ZoomFit();
                GLControl.Invalidate();
            }
            else
            {
                PanStart = new MouseStart(GLControl, cp_pos);
            }
        }

        else if (cp.Properties.IsLeftButtonPressed)
        {
            // TODO: gesture orbit

            OrbitStart = new MouseStart(GLControl, cp_pos);

        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Handled) return;

        var cp = e.GetCurrentPoint(this);

        if (PanStart is not null && !cp.Properties.IsMiddleButtonPressed)
            PanStart = null;

        if (OrbitStart is not null && !cp.Properties.IsLeftButtonPressed)
            OrbitStart = null;
    }

    // HashSet<GLFigure<IGLPrimitive>> highlightEntities = new HashSet<GLFigure<IGLPrimitive>>();

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Handled) return;

        if (model is null) return;

        var bbox = model.LBBox;
        if (bbox.IsEmpty) return;

        var mouse_cur_pt = e.GetCurrentPoint(this);

        var mouse_cur = new Vector2((float)mouse_cur_pt.Position.X, (float)mouse_cur_pt.Position.Y);

        GLControl.PointerCoord = mouse_cur;

        var mm = GLControl.ModelMatrix;
        var vm = GLControl.ViewMatrix;
        var pm = GLControl.ProjectionMatrix;
        var size = GLControl.Size();
        var isPerspective = GLControl.Perspective;

        #region pan
        if (PanStart is not null)
        {
            GLControl.Pan(
                PanStart.StartPt,
                mouse_cur,
                PanStart.GLMatrixes);

            // PanStart = null;
        }
        #endregion

        #region orbit
        else if (OrbitStart is not null)
        {
            var cp = mouse_cur_pt.Position.ToVector2();
            var sp = OrbitStart.StartPt;

            var diam = Min(Bounds.Width, Bounds.Height);

            var moveDelta = cp - sp;
            var dx = moveDelta.X;
            var dy = moveDelta.Y;

            var angleXRad = PI * dy / diam;
            var angleYRad = PI * dx / diam;

            var glMatrixesFrom = OrbitStart.GLMatrixes;

            GLControl.ModelRotate(
                angleXRad, angleYRad,
                rot_center: bbox.Middle,
                mFrom: glMatrixesFrom);
        }
        #endregion
    }

}