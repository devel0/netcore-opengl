using Avalonia.Threading;

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

        if (GLControl.GLModel.LBBox.IsEmpty) return;

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

    DateTime? lastPressTimestamp = null;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.Handled) return;

        var cp = e.GetCurrentPoint(this);
        var cp_pos = cp.Position.ToVector2();

        if (cp.Properties.IsMiddleButtonPressed)
        {
            if (e.ClickCount == 2)
                GLControl.ZoomFit();

            else
                PanStart = new MouseStart(GLControl, cp_pos);
        }

        else if (cp.Properties.IsLeftButtonPressed)
        {
            OrbitStart = new MouseStart(GLControl, cp_pos);

            lastPressTimestamp = DateTime.Now;
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
        {
            OrbitStart = null;

            if (lastPressTimestamp is not null)
            {
                var pressDurationMs = (DateTime.Now - lastPressTimestamp.Value).TotalMilliseconds;

                // if a single click return
                if (pressDurationMs <= 250 && glModel.SelectionMode)
                {
                    var lraycast = GLControl.RayCastLocal(cp.Position.ToVector2());

                    var tol = glModel.LBBox.TolHint;

                    var mm = GLControl.ModelMatrix;
                    var vm = GLControl.ViewMatrix;

                    var hitNfo = glModel.Figures
                        .Where(fig => fig.Visible)
                        // retrieve nearest figure primitives to the raycast
                        .SelectMany(fig => lraycast.Intersect(tol, fig))
                        // sort camera nearest primitive
                        .Select(hitTest => new
                        {
                            hitTest,
                            eyeHitCoord = WordlToEye(LocalToWorld(ObjectToLocal(hitTest.HitCoord, hitTest.Figure.ObjectMatrix), mm), vm)
                        })
                        .ToList()
                        .OrderByDescending(nfo => nfo.hitTest.Figure.Order)
                        .OrderBy(nfo => nfo.hitTest.Distance)
                        .OrderByDescending(nfo => nfo.eyeHitCoord.Z)
                        .FirstOrDefault();

                    if (hitNfo is not null)
                    {
                        glModel.ToggleSelectPrimitives(new[] { hitNfo.hitTest.Primitive });

                        GLControl.InvalidateAll();
                    }
                }
            }
        }
    }

    DebounceAction<Vector2>? IdentifyCoordDebounce = null;
    GLVertexManager? IdentifyCoordVtxMgr = null;

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (e.Handled) return;

        if (glModel is null) return;

        var bbox = glModel.LBBox;
        if (bbox.IsEmpty) return;

        var mouse_cur_pt = e.GetCurrentPoint(this);

        var mouse_cur = new Vector2((float)mouse_cur_pt.Position.X, (float)mouse_cur_pt.Position.Y);

        GLControl.PointerCoord = mouse_cur;

        var mm = GLControl.ModelMatrix;
        var vm = GLControl.ViewMatrix;
        var pm = GLControl.ProjectionMatrix;
        var size = GLControl.Size();
        var isPerspective = GLControl.Perspective;

        if (mouse_cur_pt.Properties.IsLeftButtonPressed)
            lastPressTimestamp = null; // avoid false click on move

        if (GLControl.IdentifyCoord)
        {
            if (IdentifyCoordDebounce is null)
            {
                var glModel = GLControl.GLModel;

                IdentifyCoordVtxMgr = new GLVertexManager(expandModelBBox: false);
                glModel.AddCustomVertexManager(IdentifyCoordVtxMgr);

                IdentifyCoordDebounce = new DebounceAction<Vector2>(TimeSpan.FromMilliseconds(500), (mouse_coord) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var vtxs = GLControl.GLModel.Vertexes.ToList();

                        var lraycast = GLControl.RayCastLocal(screen: mouse_coord);

                        var tol = glModel.LBBox.TolHint;

                        var hitNfo = glModel.Figures
                            .Where(fig => fig.Visible)
                            // retrieve nearest figure primitives to the raycast
                            .SelectMany(fig => lraycast.Intersect(tol, fig))
                            // sort camera nearest primitive
                            .Select(hitTest => new
                            {
                                hitTest,
                                eyeHitCoord = WordlToEye(LocalToWorld(ObjectToLocal(hitTest.HitCoord, hitTest.Figure.ObjectMatrix), mm), vm)
                            })
                            .ToList()
                            .OrderBy(nfo => nfo.hitTest.Distance)
                            .OrderByDescending(nfo => nfo.eyeHitCoord.Z)
                            .FirstOrDefault();

                        IdentifyCoordVtxMgr.Clear();

                        if (hitNfo is not null)
                        {
                            IdentifyCoordVtxMgr.AddFigure(new GLPointFigure(hitNfo.hitTest.HitCoord)
                            {
                                PointSize = 10,
                                ObjectMatrix = hitNfo.hitTest.Figure.ObjectMatrix
                            }
                                .SetOrder(1)
                                .SetColor(Color.Magenta));
                            GLControl.ControlOverlay1 = $"tol:{tol} ==> {hitNfo.hitTest.HitCoord.Fmt()}";
                        }

                        else
                            GLControl.ControlOverlay1 = "";

                        GLControl.Invalidate();
                    });
                });
            }

            IdentifyCoordDebounce.Hit(mouse_cur);
        }

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
                rot_center: GLControl.RotationCenter,
                mFrom: glMatrixesFrom);
        }
        #endregion
    }

    void StartIdentifyCoord()
    {
        GLControl.ControlOverlay1 = "";
    }

    void StopIdentifyCoord()
    {
        GLControl.ControlOverlay1 = "";
    }

}