using Newtonsoft.Json;

namespace SearchAThing.OpenGL.Render;

public partial class GLControl
{

    #region view control     

    /// <summary>
    /// Toggle wireframe mode.
    /// </summary>    
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="Wireframe"/>
    public void ToggleWireframe(bool invalidate = DEFAULT_INVALIDATE)
    {
        Wireframe = !Wireframe;
        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Toggle shade with edge.
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="ShadeWithEdge"/>
    public void ToggleShadeWithEdge(bool invalidate = DEFAULT_INVALIDATE)
    {
        ShadeWithEdge = !ShadeWithEdge;
        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Toggle show normals.
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="ShowNormals"/>
    public void ToggleShowNormals(bool invalidate = DEFAULT_INVALIDATE)
    {
        ShowNormals = !ShowNormals;
        if (invalidate) Invalidate();
    }

    bool initialPerspectiveToggle = true;

    /// <summary>
    /// Toggle perspective mode.
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="Perspective"/>
    public void TogglePerspective(bool invalidate = DEFAULT_INVALIDATE)
    {
        Perspective = !Perspective;
        if (initialPerspectiveToggle)
        {
            initialPerspectiveToggle = false;
            ZoomFit(invalidate);
        }
        else if (invalidate) Invalidate();
    }

    /// <summary>
    /// Toggle use of textures.
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="UseTexture"/>
    public void ToggleUseTexture(bool invalidate = DEFAULT_INVALIDATE)
    {
        UseTexture = !UseTexture;
        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Toggle shadows generation.
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="UseShadow"/>
    public void ToggleUseShadow(bool invalidate = DEFAULT_INVALIDATE)
    {
        UseShadow = !UseShadow;
        if (invalidate) Invalidate();
    }


    /// <summary>
    /// Zoom eye space by given scale factor.
    /// </summary>
    /// <param name="scale">Zoom scale factor.</param>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="PerspectiveZoom"/>
    /// <seealso cref="OrthoZoom"/>
    public void CameraZoom(float scale, bool invalidate = DEFAULT_INVALIDATE)
    {
        if (Perspective)
        {
            PerspectiveZoom(scale, GLModel.LBBox, ModelMatrix, ViewMatrix, CameraPos, CameraTarget,
                out var wScaledCameraPos, out var wScaledCameraTarget);

            CameraPos = wScaledCameraPos;
            CameraTarget = wScaledCameraTarget;
        }
        else
        {
            OrthoZoom(scale, OrthoZoom, out var scaledSOrthoZoom);
            OrthoZoom = scaledSOrthoZoom;
        }
        if (invalidate) this.Invalidate();
    }

    /// <summary>
    /// Zoom in eye space.
    /// </summary>
    /// <param name="scale">Zoom in scale factor.</param>
    /// <param name="invalidate">If true refresh the scene.</param>
    public void CameraZoomIn(float scale = ZOOM_LITTLE_FACTOR, bool invalidate = DEFAULT_INVALIDATE) =>
        CameraZoom(scale, invalidate);

    /// <summary>
    /// Zoom out eye space.
    /// </summary>
    /// <param name="scale">Zoom out scale factor.</param>
    /// <param name="invalidate">If true refresh the scene.</param>
    public void CameraZoomOut(float scale = ZOOM_LITTLE_FACTOR, bool invalidate = DEFAULT_INVALIDATE) =>
        CameraZoom(-scale, invalidate);

    /// <summary>
    /// Retrieve model/view/projection matrixes and camera pos/target/up through a single object.
    /// </summary>    
    public GLMatrixes GetGLMatrixes() =>
        new GLMatrixes(ModelMatrix, ViewMatrix, ProjectionMatrix, CameraPos, CameraTarget, CameraUp);

    /// <summary>
    /// Pan the view by the given screen translation.
    /// </summary>
    /// <param name="screenFrom">Pan screen position from.</param>
    /// <param name="screenTo">Pan screen position to.</param>
    /// <param name="mFrom">(Optional) begin pan matrixes.</param>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="PerspectivePan"/>
    /// <seealso cref="OrthoPan"/>
    /// <seealso cref="GetGLMatrixes"/>
    public void Pan(
        in Vector2 screenFrom, in Vector2 screenTo,
        GLMatrixes? mFrom = null,
        bool invalidate = DEFAULT_INVALIDATE)
    {
        if (mFrom is null) mFrom = GetGLMatrixes();

        var screenSize = Size();

        if (Perspective)
        {
            PerspectivePan(
                refPt: GLModel.LBBox.Middle,
                screenFrom, screenTo, screenSize,
                mFrom.mm, mFrom.vm, mFrom.pm,
                mFrom.cp, mFrom.ct,
                out var cp, out var ct);

            CameraPos = cp;
            CameraTarget = ct;
        }
        else
        {
            OrthoPan(
                screenFrom, screenTo, screenSize,
                mFrom.vm, mFrom.pm,
                mFrom.cp, mFrom.ct,
                out var cp, out var ct);

            CameraPos = cp;
            CameraTarget = ct;
        }

        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Rotate the camera [view] a given rotation center [local].
    /// ( not yet tested )
    /// </summary>
    /// <param name="sAngleXRad"></param>
    /// <param name="sAngleYRad"></param>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="Core.Toolkit.CameraRotate"/>
    public void CameraRotate(float sAngleXRad, float sAngleYRad,
        bool invalidate = DEFAULT_INVALIDATE)
    {
        Core.Toolkit.CameraRotate(sAngleXRad, sAngleYRad,
            CameraPos, CameraTarget, CameraUp,
            out var wRotateCameraTarget, out var wRotatedCameraUp);

        CameraTarget = wRotateCameraTarget;
        CameraUp = wRotatedCameraUp;

        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Rotate the model by given [eye] x,y angles around given [local] rotation center.
    /// </summary>
    /// <param name="angleXRad">Eye space x angle rotation (rad).</param>
    /// <param name="angleYRad">Eye space y angle rotation (rad).</param>
    /// <param name="rot_center">(Optional) Local space (x,y,z) rotation center [local].</param>        
    /// <param name="mFrom">(Optional) gl matrixes at rotation start.</param>
    /// <param name="invalidate">If true refresh the scene.</param>
    public void ModelRotate(
        double angleXRad, double angleYRad,
        Vector3? rot_center = null,
        GLMatrixes? mFrom = null,
        bool invalidate = DEFAULT_INVALIDATE)
    {
        if (mFrom is null) mFrom = GetGLMatrixes();
        if (rot_center is null) rot_center = GLModel.LBBox.Middle;

        Core.Toolkit.ModelRotate(
            (float)angleXRad, (float)angleYRad,
            rot_center.Value, mFrom.mm, mFrom.vm, out var mmRotated);

        ModelMatrix = mmRotated;

        if (invalidate) Invalidate();
    }

    // public static (Vector3 scale, Quaternion rotation, Vector3 translation, bool success)
    // Decompose(this Matrix4x4 m) =>
    //     Matrix4x4.Decompose(m, out var scale, out var rotate, out var translate)
    //     .Fn(success => (scale, rotate, translate, success));

    /// <summary>
    /// Tilt camera [view] by rotating it's up vector around [view] z axis.
    /// </summary>
    /// <param name="angleDeg">Tilt angle (deg) [view].</param>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="Core.Toolkit.CameraTilt"/>
    public void Tilt(float angleDeg = TILT_LITTLE_DEG, bool invalidate = DEFAULT_INVALIDATE)
    {
        Core.Toolkit.CameraTilt(angleDeg.ToRad(), CameraUp, out var cuTilted);

        CameraUp = cuTilted;

        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Retrieve actual view config.
    /// </summary>
    /// <seealso cref="SaveView"/>
    public ViewNfo GetViewNfo() => new ViewNfo
    {
        ModelMatrix = ModelMatrix,
        CameraPos = CameraPos,
        CameraTarget = CameraTarget,
        CameraUp = CameraUp,
        Perspective = Perspective,
        Wireframe = Wireframe,
        OrthoZoom = OrthoZoom,
        Near = Near,
        Far = Far,
        ShadeWithEdge = ShadeWithEdge,
        ShowCameraObject = ShowCameraObject,
        Lights = GLModel.PointLights.ToList()
    };

    /// <summary>
    /// Save actual view info to given pathfilename.<br/>
    /// A Notification <see cref="GLControl.NotificationRequest"/> event emitted.
    /// <seealso cref="ViewDefaultPathfilename"/>    
    /// </summary>    
    public void SaveView(string? pathfilename = null)
    {
        var nfo = GetViewNfo();

        if (pathfilename is null) pathfilename = ViewDefaultPathfilename;

        try
        {
            File.WriteAllText(pathfilename, JsonConvert.SerializeObject(nfo, Formatting.Indented));

            NotificationRequest?.Invoke("GLControl view", $"View saved to\n[{pathfilename}]");
        }
        catch (Exception ex)
        {
            NotificationRequest?.Invoke("GLControl view", $"Error saving to\n[{pathfilename}].\n{ex.Message}",
                GLNotificationType.Error);
        }
    }

    /// <summary>
    /// Restore current view config.
    /// </summary>
    /// <param name="nfo">view config object</param>
    /// <seealso cref="Loadview"/>
    public void SetViewNfo(ViewNfo nfo)
    {
        ModelMatrix = nfo.ModelMatrix;
        CameraPos = nfo.CameraPos;
        CameraTarget = nfo.CameraTarget;
        CameraUp = nfo.CameraUp;
        Perspective = nfo.Perspective;
        Wireframe = nfo.Wireframe;
        OrthoZoom = nfo.OrthoZoom;
        Near = nfo.Near;
        Far = nfo.Far;
        ShadeWithEdge = nfo.ShadeWithEdge;
        ShowCameraObject = nfo.ShowCameraObject;
        if (nfo.Lights is not null)
        {
            GLModel.PointLights.Clear();
            foreach (var light in nfo.Lights)
            {
                GLModel.PointLights.Add(light);
            }
        }

        LastCameraView = CameraViewType.Manual;
    }

    /// <summary>
    /// Restore current view config from given pathfilename.
    /// </summary>
    /// <seealso cref="SetViewNfo"/>
    public void LoadView(string? pathfilename = null)
    {
        if (pathfilename is null) pathfilename = ViewDefaultPathfilename;

        var nfo = JsonConvert.DeserializeObject<ViewNfo>(File.ReadAllText(pathfilename));
        if (nfo is null) return;

        SetViewNfo(nfo);

        Invalidate();
    }

    /// <summary>
    /// Fit gl model into the view.
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    /// <seealso cref="PerspectiveZoomFit"/>
    /// <seealso cref="OrthoZoomFit"/>
    public void ZoomFit(bool invalidate = DEFAULT_INVALIDATE)
    {
        if (GLModel.LBBox.IsEmpty) return;

        var boundsSize = Device.Size;
        if (boundsSize.Width == 0 || boundsSize.Height == 0)
            return;

        if (Perspective)
        {
            PerspectiveZoomFit(
                GLModel.LBBox.Points,
                Size(),
                ModelMatrix, ViewMatrix, ProjectionMatrix,
                CameraPos, CameraTarget, CameraUp,
                FovDeg,
                out var fitCameraPos, out var fitCameraTarget);

            CameraPos = fitCameraPos;
            CameraTarget = fitCameraTarget;
        }

        else
        {
            OrthoZoomFit(
                pts: GLModel.LBBox.Points,
                Size(),
                ModelMatrix, ViewMatrix, ProjectionMatrix,
                OrthoZoom, Near, Far,
                CameraPos, CameraTarget, CameraUp,
                out var fitSOrthoZoom,
                out var panCameraPos, out var panCameraTarget);

            OrthoZoom = fitSOrthoZoom;
            CameraPos = panCameraPos;
            CameraTarget = panCameraTarget;
        }

        if (invalidate) this.Invalidate();
    }

    GLVertexManager? _debugVtxMgr = null;
    GLVertexManager debugVtxMgr
    {
        get
        {
            if (_debugVtxMgr is null)
            {
                _debugVtxMgr = new GLVertexManager(expandModelBBox: false);
                this.GLModel.AddCustomVertexManager(debugVtxMgr);
            }
            return _debugVtxMgr;
        }
    }

    #endregion

    #region little methods

    /// <summary>
    /// Pan the view left of a little factor.
    /// </summary>
    public void PanLeft()
    {
        var scrPos = Size() / 2;

        Pan(new Vector2(scrPos.X, scrPos.Y), new Vector2(scrPos.X - scrPos.X * PAN_LITTLE_FACTOR, scrPos.Y));
    }

    /// <summary>
    /// Pan the view right of a little factor.
    /// </summary>
    public void PanRight()
    {
        var scrPos = Size() / 2;

        Pan(new Vector2(scrPos.X, scrPos.Y), new Vector2(scrPos.X + scrPos.X * PAN_LITTLE_FACTOR, scrPos.Y));
    }

    /// <summary>
    /// Pan the view up of a little factor.
    /// </summary>
    public void PanUp()
    {
        var scrPos = Size() / 2;

        Pan(new Vector2(scrPos.X, scrPos.Y), new Vector2(scrPos.X, scrPos.Y - scrPos.Y * PAN_LITTLE_FACTOR));
    }

    /// <summary>
    /// Pan the view down of a little factor
    /// </summary>
    public void PanDown()
    {
        var scrPos = Size() / 2;

        Pan(new Vector2(scrPos.X, scrPos.Y), new Vector2(scrPos.X, scrPos.Y + scrPos.Y * PAN_LITTLE_FACTOR));
    }

    /// <summary>
    /// Rotate the model left of a little factor.
    /// </summary>
    public void ModelRotateLeft() => ModelRotate(0, -ROTATE_LITTLE_DEG.ToRad());

    /// <summary>
    /// Rotate the model right of a little factor.
    /// </summary>
    public void ModelRotateRight() => ModelRotate(0, ROTATE_LITTLE_DEG.ToRad());

    /// <summary>
    /// Rotate the model up of a little factor.
    /// </summary>
    public void ModelRotateUp() => ModelRotate(-ROTATE_LITTLE_DEG.ToRad(), 0);

    /// <summary>
    /// Rotate the model down of a little factor.
    /// </summary>
    public void ModelRotateDown() => ModelRotate(ROTATE_LITTLE_DEG.ToRad(), 0);

    /// <summary>
    /// Rotate the camera left of little factor.
    /// </summary>
    public void CameraRotateLeft() => CameraRotate(0, -ROTATE_LITTLE_DEG.ToRad());

    /// <summary>
    /// Rotate the camera right of a little factor.
    /// </summary>
    public void CameraRotateRight() => CameraRotate(0, ROTATE_LITTLE_DEG.ToRad());

    /// <summary>
    /// Rotate the camera up of a little factor.
    /// </summary>
    public void CameraRotateUp() => CameraRotate(-ROTATE_LITTLE_DEG.ToRad(), 0);

    /// <summary>
    /// Rotate the camera down of a little factor.
    /// </summary>
    public void CameraRotateDown() => CameraRotate(ROTATE_LITTLE_DEG.ToRad(), 0);

    /// <summary>
    /// Tilt the camera left of a little factor.
    /// </summary>
    public void TiltLeft() => Tilt(TILT_LITTLE_DEG);

    /// <summary>
    /// Titl the camera right of a little factor.
    /// </summary>
    public void TiltRight() => Tilt(-TILT_LITTLE_DEG);

    #endregion

    void ResetView()
    {
        // this.PerspectiveDistance = DEAULT_PERSPECTIVE_CAMERA_DISTANCE;
        _ViewMatrix = null;
        _ProjectionMatrix = null;
        initialPerspectiveToggle = true;
    }

}
