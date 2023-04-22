namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Provides basic opengl abstraction to create entities such as <see cref="GLPoint"/>, <see cref="GLLine"/> and <see cref="GLTriangle"/>.
/// It provides high level functions to manage camera view and related pan/zoom functions.<br/>
/// The <see cref="Invalidate"/> emit a <see cref="GLControl.RenderInvalidated"/> event that 
/// <see cref="SearchAThing.OpenGL.GUI.AvaloniaGLControl"/> listen to invalidte its visual.<br/>
/// <a href="Render.svg">image</a>
/// \image html Render.svg width=100%    
/// </summary>
public partial class GLControl : INotifyPropertyChanged
{

    /// <summary>
    /// Render device associateed to this control.
    /// </summary>    
    public IRenderDevice Device { get; private set; }

    /// <summary>
    /// Get render surface size (pixels).
    /// </summary>    
    public Vector2 Size() => Device.Size.Fn(sizeint => new Vector2(sizeint.Width, sizeint.Height));

    DateTime dtLastRenderCompleted = DateTime.Now;

    /// <summary>
    /// Create a new gl control attached to given model and rendering to the given rendere device.
    /// </summary>
    /// <param name="glModel">Gl model.</param>
    /// <param name="device">Target render device.</param>
    public GLControl(GLModel glModel, IRenderDevice device)
    {
        _Model = glModel;
        Device = device;
        ID = $"{IDs++}";

        IsInitial = true;

        ListenModelChanges();
    }

    /// <summary>
    /// Create a new gl control using the same control properties of the given template.
    /// </summary>
    /// <param name="template">Gl control template to config this new one created.</param>
    /// <param name="device">Target render device.</param>
    /// <seealso cref="SearchAThing.OpenGL.GUI.AvaloniaGLControlSplit"/>
    public GLControl(GLControl template, IRenderDevice device)
    {
        _Model = template.GLModel;
        Device = device;
        ID = $"{IDs++}";

        _ModelMatrix = template.ModelMatrix;
        _ViewMatrix = template.ViewMatrix;
        _ProjectionMatrix = template.ProjectionMatrix;

        // _CameraCS = template.CameraCS;
        // _CameraPos = template.CameraPos;
        // _CameraTarget = template.CameraTarget;
        // _CameraUp = template.CameraUp;

        _Perspective = template.Perspective;
        _OrthoZoom = template.OrthoZoom;

        _FovDeg = template.FovDeg;
        _Near = template.Near;
        _Far = template.Far;

        _Wireframe = template.Wireframe;
        _UseTexture = template.UseTexture;

        ListenModelChanges();
    }

    void ListenModelChanges()
    {
        GLModel.PointLights.ItemsAdded += (a, items) =>
        {
            foreach (var light in items.OfType<GLPointLight>())
                light.PropertyChanged += ModelPointLightChanged;
        };

        GLModel.PointLights.ItemsRemoved += (a, items) =>
        {
            foreach (var light in items.OfType<GLPointLight>())
                light.PropertyChanged -= ModelPointLightChanged;
        };

        GLModel.PropertyChanged += (sender, e) =>
        {
            if (
                e.PropertyName == nameof(GLModel.OverrideAmbient) ||
                e.PropertyName == nameof(GLModel.OverrideAmbientEnabled) ||

                e.PropertyName == nameof(GLModel.OverrideDiffuse) ||
                e.PropertyName == nameof(GLModel.OverrideDiffuseEnabled) ||

                e.PropertyName == nameof(GLModel.OverrideSpecular) ||
                e.PropertyName == nameof(GLModel.OverrideSpecularEnabled)
                )
                IsControlInvalidated = true;
        };

        foreach (var light in GLModel.PointLights)
            light.PropertyChanged += ModelPointLightChanged;
    }

    private void ModelPointLightChanged(object? sender, PropertyChangedEventArgs e) => IsControlInvalidated = true;

    void DebugFramebufferStatus(string msg)
    {
        var q = (FramebufferStatus)GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (q != FramebufferStatus.Complete)
        {
            Debug.WriteLine($"{msg} FRAMEBUFFER STATUS ===> {q}");
            Debugger.Break();
        }
    }

    /// <summary>
    /// Capture an image of current control using offscreen render device and actual rendere device size.
    /// </summary>
    /// <param name="offscreenPathfilename">Pathfilename where to save the capture.</param>
    public void CaptureImage(string offscreenPathfilename) =>
        Render(new OffscreenRenderDevice(offscreenPathfilename, Size().Fn(s => new Size((int)s.X, (int)s.Y))));

    /// <summary>
    /// Capture an image of current control using offscreen render device with given render device size.
    /// </summary>
    /// <param name="offscreenPathfilename">Pathfilename where to save the capture.</param>
    /// <param name="w">Width of capture image (pixels).</param>
    /// <param name="h">Height of capture image (pixels).</param>
    public void CaptureImage(string offscreenPathfilename, int w, int h) =>
        Render(new OffscreenRenderDevice(offscreenPathfilename, new Size(w, h)));

    /// <summary>
    /// Clear screen
    /// </summary>
    void Clear(bool shadowMode, Color? color = null)
    {
        if (color is null) color = DEFAULT_ClearColor;

        GL.ClearColor(color.Value);
        if (!UseShadow || !shadowMode)
            GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        else
            GL.Clear((uint)(ClearBufferMask.DepthBufferBit));
    }

    /// <summary>
    /// Event emitted when <see cref="Invalidate"/> called.<br/>
    /// Handled by avalonia gl control to execute the <see cref="Render"/> and transfer gl pixels to the device target surface.
    /// </summary>
    public event EventHandler? RenderInvalidated = null;

    /// <summary>
    /// Regenerate the model with <see cref="GLModel.BuildModel"/> if this is the first invalidation
    /// or the invalidate argument is of type <see cref="InvalidateEnum.RebuildModelAndRedraw"/>.<br/>
    /// Emits a <see cref="RenderInvalidated"/> event that avalonia gl control will intercept to update the render surface.
    /// </summary>
    /// <param name="invalidate">Type of invalidation (Default:<see cref="InvalidateEnum.RedrawOnly"/>).</param>
    public void Invalidate(InvalidateEnum invalidate = InvalidateEnum.RedrawOnly)
    {
        if (invalidate == InvalidateEnum.None || IsRendering) return;

        GLContext.AutoCleanup();

        IsRenderInvalidated = true;

        if (invalidate == InvalidateEnum.RebuildModelAndRedraw)
        {
            GLModel.InvalidateModel();
        }

        RenderInvalidated?.Invoke(this, new EventArgs());
    }

    public event EventHandler InvalidateAllRequest;

    /// <summary>
    /// All gl split control invalidation.
    /// </summary>
    public void InvalidateAll() => InvalidateAllRequest.Invoke(this, EventArgs.Empty);

    public delegate void RenderPreviewDelegate(GLControl glControl);

    /// <summary>
    /// Event generated at first non bbox empty render generated.
    /// </summary>
    public event RenderPreviewDelegate? NotEmptyRenderPreview;

    bool isInitialRender = true;

    /// <summary>
    /// States if rendering is in progress.<br/>
    /// Its used by <see cref="Render"/> and <see cref="Invalidate"/> to avoid re-entrance from <see cref="RenderInvalidated"/> event callback.
    /// </summary>
    public bool IsRendering { get; private set; } = false;

    #region IsRenderInvalidated

    private bool _IsRenderInvalidated = true;
    /// <summary>
    /// States if last render transferred is actually invalidated respect to the gl model or this gl control view/preferences.<br/>
    /// Evaluated in <see cref="Render"/> to avoid re-entrance.
    /// </summary>
    public bool IsRenderInvalidated
    {
        get => _IsRenderInvalidated;
        private set
        {
            var changed = value != _IsRenderInvalidated;
            if (changed)
            {
                _IsRenderInvalidated = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region IsControlInvalidated

    private bool _IsControlInvalidated = true;

    /// <summary>
    /// Triggered by control view/preferences and by model lights changes.
    /// </summary>    
    public bool IsControlInvalidated
    {
        get => _IsControlInvalidated;
        private set
        {
            var changed = value != _IsControlInvalidated;
            if (changed)
            {
                _IsControlInvalidated = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Draw figures:<br/>
    /// - Map shadow transform ( if not null ) view matrixes, point lights to gl shader storage blocks.<br/>
    /// - Map vertexes to gl vertex/array buffer.<br/>
    /// - Loop through vertex manager figures matching given predicate and invoke GL DrawElements.
    /// </summary>
    /// <param name="figureMatch">Predicate to match figure included in rendering.</param>
    /// <param name="shader">Shader to use in this gl rendering stage.</param>
    /// <param name="vtxMgr">Vertex manager to render.</param>
    /// <param name="ptLights">List of point lights.</param>
    /// <param name="shadowTransform">(Optional) list of shadow map cube view matrixes.</param>
    void RenderVertexManager(
        Func<GLFigureBase, bool> figureMatch,
        GLPipeline shader,
        GLVertexManager vtxMgr,
        GLPointLightStruct[] ptLights,
        GLMatrix4x4Struct[]? shadowTransform)
    {
        bool restoreWireframeMode = false;
        bool restoreIsText = false;

        GL.PolygonMode(GLEnum.FrontAndBack, Wireframe ? PolygonMode.Line : PolygonMode.Fill);
        shader.SetBool(UNINAME_uIsText, false);

        var VboAry = vtxMgr.GetVboArray();

        GLShaderStorageBlock<GLMatrix4x4Struct>? pLSbo = null;

        if (shadowTransform is not null)
            pLSbo = new GLShaderStorageBlock<GLMatrix4x4Struct>(GLContext, shader, SSBO_Blockname_PointLightsMatrixesBlock, shadowTransform);

        using (var pLbo = new GLShaderStorageBlock<GLPointLightStruct>(GLContext, shader, SSBO_Blockname_PointLightsBlock, ptLights))
        {

            var shadowMapMode = shadowTransform is not null;

            using (var Vbo = new GLVertexBufferObject<GLVertexStruct>(GL, BufferTargetARB.ArrayBuffer, VboAry))
            using (var Vao = new GLVertexArrayObject<GLVertexStruct>(GL))
            {

                // VERTEX 1                                                               | VERTEX 2           | ...
                // Position   Normal     MatColor   MatProp            TextureST   Flags  | ...                | ...
                // X Y Z      Nx Ny Nz   R G B A    lAmb lDiff lSpec   S T         (uint) | ...                | ...

                var off = 0;
                var compCnt = 3;
                var compSize = sizeof(float);

                uint? loc = null;

                // Position(Vector3)
                compCnt = 3;
                loc = shader.GetAttributeLocation(ATTNAME_vPos);
                if (loc is not null)
                {
                    Vao.AttribPointer(loc.Value,
                        offset: off,
                        components: compCnt,
                        componentType: VertexAttribPointerType.Float,
                        normalized: false);
                }
                off += compCnt * compSize;

                //--

                // Normal(Vector3)
                compCnt = 3;
                loc = shader.GetAttributeLocation(ATTNAME_vNormal);
                if (loc is not null)
                {
                    Vao.AttribPointer(loc.Value,
                        offset: off,
                        components: compCnt,
                        componentType: VertexAttribPointerType.Float,
                        normalized: true);
                }
                off += compCnt * compSize;

                //--

                // MatColor(Vector4)
                compCnt = 4;
                loc = shader.GetAttributeLocation(ATTNAME_vMatColor);
                if (loc is not null)
                {
                    Vao.AttribPointer(loc.Value,
                        offset: off,
                        components: compCnt,
                        componentType: VertexAttribPointerType.Float,
                        normalized: false);
                }
                off += compCnt * compSize;

                //--

                // MatProp(Vector3)
                compCnt = 3;
                loc = shader.GetAttributeLocation(ATTNAME_vMatProp);
                if (loc is not null)
                {
                    Vao.AttribPointer(loc.Value,
                        offset: off,
                        components: compCnt,
                        componentType: VertexAttribPointerType.Float,
                        normalized: false);

                }
                off += compCnt * compSize;

                //--

                // TextureST(Vector2)
                compCnt = 2;
                loc = shader.GetAttributeLocation(ATTNAME_vTexCoord);
                if (loc is not null)
                {
                    Vao.AttribPointer(loc.Value,
                        offset: off,
                        components: compCnt,
                        componentType: VertexAttribPointerType.Float,
                        normalized: false);
                }
                off += compCnt * compSize;

                //--               

                // Flags(uint)
                compSize = sizeof(uint);
                compCnt = 1;
                loc = shader.GetAttributeLocation(ATTNAME_vFlags);
                if (loc is not null)
                {
                    Vao.AttribIPointer(loc.Value,
                        offset: off,
                        components: compCnt,
                        componentType: VertexAttribIType.UnsignedInt);
                }
                off += compCnt * compSize;

                //------------------------------------------------------------------------------
                // FIGURES LOOP
                //------------------------------------------------------------------------------

                IEnumerable<IGLFigure> figures = vtxMgr.Figures
                    .Where(r => figureMatch(r) && r.Visible && (!shadowMapMode || r.EvalInShadowMap))
                    .OrderByDescending(w => w.Order);

                foreach (var fig in figures)
                {
                    if (!shadowMapMode)
                    {
                        shader.SetBool(UNINAME_uFigureHighlight, fig.Highlight);
                        shader.SetBool(UNINAME_uFigureSelected, fig.Selected);
                        if (fig.Alpha.HasValue)
                            shader.SetFloat(UNINAME_uFigureAlpha, fig.Alpha.Value);
                        else
                            shader.SetFloat(UNINAME_uFigureAlpha, -1);
                    }
                    else
                        shader.SetFloat(UNINAME_uFigureAlpha, -1);

                    var glChar = fig as GLTextCharFigure;

                    {
                        if (restoreWireframeMode)
                        {
                            GL.PolygonMode(GLEnum.FrontAndBack, Wireframe ? PolygonMode.Line : PolygonMode.Fill);
                            restoreWireframeMode = false;
                        }

                        if (restoreIsText)
                        {
                            shader.SetBool(UNINAME_uIsText, false);
                            restoreIsText = false;
                        }

                    }

                    shader.SetMat4(UNINAME_uObject, fig.ObjectMatrix);

                    var figIdxs = fig.Indexes.ToArray();

                    using (var Ebo = new GLVertexBufferObject<uint>(GL, BufferTargetARB.ElementArrayBuffer, figIdxs))
                    {

                        if (!shadowMapMode)
                        {
                            shader.SetBool(UNINAME_uFigureScreenCoord, fig.ScreenCoordMode);

                            if (UseTexture && fig is GLTriangleFigure triFig && triFig.Texture2D is not null)
                            {
                                shader.SetBool(UNINAME_uUseTexture, true);

                                // https://learnopengl.com/Getting-started/Textures                                                       

                                if (glChar is not null && glChar.Texture2D is not null)
                                {
                                    if (Wireframe)
                                    {
                                        GL.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
                                        restoreWireframeMode = true;
                                    }

                                    shader.SetBool(UNINAME_uIsText, true);
                                    restoreIsText = true;

                                    var textColor = glChar.GLText.Color.ToVector4();
                                    shader.SetVec4(UNINAME_uTextColor, textColor);

                                    GL.BindTexture(TextureTarget.Texture2D, glChar.Texture2D.TextureId);
                                }
                                else
                                {
                                    GL.BindTexture(TextureTarget.Texture2D, triFig.Texture2D.TextureId);
                                }
                            }
                            else
                                shader.SetBool(UNINAME_uUseTexture, false);

                            if (fig is GLPointFigure ptFig)
                            {
                                GL.PointSize(ptFig.PointSize);
                                shader.SetBool(UNINAME_uEvalLight, false);
                                shader.SetBool(UNINAME_uUseTexture, false);
                            }

                            else if (fig.PrimitiveType == GLPrimitiveType.Line)
                            {
                                shader.SetBool(UNINAME_uEvalLight, false);
                            }

                            else
                            {
                                shader.SetBool(UNINAME_uEvalLight, true);
                            }
                        }

                        unsafe
                        {
                            GL.DrawElements(fig.PrimitiveType.GL(),
                                (uint)figIdxs.Length,
                                DrawElementsType.UnsignedInt, (void*)0);
                        }
                    }

                }

            }

        }

        if (pLSbo is not null)
            pLSbo!.Dispose();
    }

    /// <summary>
    /// Render vertex managers:<br/>
    /// - temporary vertex manager if <see cref="GLControl.ShowModelBBox"/> or <see cref="GLControl.ShowCameraObject"/>.<br/>
    /// - custom vertex managers ( <see cref="GLModel.AddCustomVertexManager"/> ).<br/>
    /// - main vertex manager ( <see cref="GLModel.GLVertexManager"/> ).<br/>
    /// </summary>
    /// <param name="figureMatch">Predicate to match figure included in rendering.</param>
    /// <param name="shader">Shader to use in this gl rendering stage.</param>
    /// <param name="ptLightStructs">List of point lights.</param>
    /// <param name="shadowTransform">(Optional) list of shadow map cube view matrixes.</param>
    /// <param name="clear">Clear the scene ( Default: true ).</param>
    void RenderScene(
        Func<GLFigureBase, bool> figureMatch,
        GLPipeline shader,
        GLPointLightStruct[] ptLightStructs,
        GLMatrix4x4Struct[]? shadowTransform = null,
        bool clear = true)
    {
        var shadowMapMode = shadowTransform is not null;

        shader.Use();

        if (clear)
            Clear(shadowMapMode, ClearColor);

        if (GLModel.LBBox.IsEmpty) return;

        if (!shadowMapMode)
        {
            var tmpVtxMgr = new GLVertexManager();

            if (ShowModelBBox)
                tmpVtxMgr.AddFigure(new GLLineFigure(GLModel.LBBox.Lines()));

            foreach (var showCameraCtl in GLModel.ShowCameraObjectControls)
                tmpVtxMgr.AddFigure(showCameraCtl.MakeFrustum());

            {
                var pts = GLModel.PointLights
                    .Where(r => r.ShowPoint)
                    .Select(light => new GLPoint(
                        new GLVertex(light.Position, (light.Active ? Color.Yellow : Color.Gray).ToVector4())))
                    .ToArray();

                if (pts.Length > 0)
                {
                    var lightPoints = new GLPointFigure(pts);
                    lightPoints.PointSize = DEFAULT_LIGHT_SHOW_POINT_SIZE;

                    tmpVtxMgr.AddFigure(lightPoints);
                }
            }

            if (tmpVtxMgr.Figures.Any())
                RenderVertexManager(figureMatch,
                    shader,
                    tmpVtxMgr,
                    ptLightStructs,
                    shadowTransform);
        }

        foreach (var customVtxMgr in GLModel.CustomVertexManagers)
            RenderVertexManager(figureMatch,
                shader,
                customVtxMgr,
                ptLightStructs,
                shadowTransform);

        RenderVertexManager(figureMatch,
            shader,
            GLModel.GLVertexManager,
            ptLightStructs,
            shadowTransform);
    }

    /// <summary>
    /// Use gl shader, setting gl control and model uniform related variables and issue a <see cref="RenderScene"/>.
    /// </summary>
    /// <param name="figureMatch">Predicate to match figure included in rendering.</param>
    /// <param name="shader">Shader to use in this gl rendering stage.</param>
    /// <param name="ptLightStructs">List of point lights.</param>    
    /// <param name="clear">Clear the scene ( Default: true ).</param>
    void DoShader(Func<GLFigureBase, bool> figureMatch, GLPipeline shader, GLPointLightStruct[] ptLightStructs, bool clear = true)
    {
        shader.Use();

        shader.SetMat4(UNINAME_uModel, ModelMatrix);
        shader.SetMat4(UNINAME_uView, ViewMatrix);
        shader.SetMat4(UNINAME_uProjection, ProjectionMatrix);
        shader.SetBool(UNINAME_uFigureScreenCoord, false);

        shader.SetMat4(UNINAME_uScreenProjection, ScreenProjectionMatrix);
        shader.SetMat4(UNINAME_uScreenModel, ScreenModelMatrix);
        shader.SetBool(UNINAME_uFigureHighlight, false);
        shader.SetFloat(UNINAME_uOverrideAmbient, GLModel.OverrideAmbient);
        shader.SetBool(UNINAME_uOverrideAmbientEnabled, GLModel.OverrideAmbientEnabled);
        shader.SetFloat(UNINAME_uOverrideDiffuse, GLModel.OverrideDiffuse);
        shader.SetBool(UNINAME_uOverrideDiffuseEnabled, GLModel.OverrideDiffuseEnabled);
        shader.SetFloat(UNINAME_uOverrideSpecular, GLModel.OverrideSpecular);
        shader.SetBool(UNINAME_uOverrideSpecularEnabled, GLModel.OverrideSpecularEnabled);

        shader.SetVec3(UNINAME_uCameraPos, CameraPos);
        if (GLModel.LBBox.IsEmpty)
        {
            shader.SetVec3(UNINAME_uBBoxMin, Vector3.Zero);
            shader.SetVec3(UNINAME_uBBoxMax, Vector3.Zero);
        }
        else
        {
            shader.SetVec3(UNINAME_uBBoxMin, GLModel.LBBox.Min);
            shader.SetVec3(UNINAME_uBBoxMax, GLModel.LBBox.Max);
        }
        shader.SetFloat(UNINAME_uNormalFactor, GLModel.NormalFactor);

        shader.SetBool(UNINAME_uUseTexture, UseTexture);
        shader.SetFloat(UNINAME_uFarPlaneLight, Far);

        shader.SetBool(UNINAME_uShadows, UseShadow);
        GL.ActiveTexture(MAIN_TEXTURE_UNIT);
        shader.SetInt(UNINAME_uMainTexture, MAIN_TEXTURE_UNIT - TextureUnit.Texture0);
        if (shadow_texture is not null)
            GL.BindTexture(TextureTarget.TextureCubeMapArray, shadow_texture.Value);

        shader.SetInt(UNINAME_uDepthMap, SHADOW_TEXTURE_UNIT - TextureUnit.Texture0);
        shader.SetBool(UNINAME_uEvalLight, true);
        shader.SetBool(UNINAME_uIsText, false);

        DebugFramebufferStatus(shader.FriendlyName);

        RenderScene(figureMatch, shader, ptLightStructs, clear: clear);
    }

    /// <summary>
    /// Main gl render process.
    /// </summary>
    /// <param name="device">Target render surface.</param>
    public void Render(IRenderDevice? device = null)
    {
        if (!IsRenderInvalidated || IsRendering) return;

        if (device is null) device = Device;

        IsRendering = true;

        var ps = device.Size;

        var sizeChanged = false;

        if (glControlLastKnownSize is null ||
            glControlLastKnownShadowSize is null ||
            !glControlLastKnownSize.Value.Equals(ps) ||
            glControlLastKnownShadowSize.Value.w != ShadowWidth ||
            glControlLastKnownShadowSize.Value.h != ShadowHeight)
        {
            InvalidateProjectionMatrix();

            glControlLastKnownSize = ps;
            glControlLastKnownShadowSize = (ShadowWidth, ShadowHeight);

            sizeChanged = true;
        }

        var w = (uint)glControlLastKnownSize!.Value.Width;
        var h = (uint)glControlLastKnownSize!.Value.Height;

        GLModel.EnsureModel(this);

        var lBBox = GLModel.LBBox;
        if (lBBox.IsEmpty)
        {
            IsRendering = false;
            return;
        }

        if (isInitialRender)
        {
            NotEmptyRenderPreview?.Invoke(this);

            isInitialRender = false;
        }

        if (AutoAdjustNearFar && Perspective && !lBBox.IsEmpty)
        {
            PreventZFighting(lBBox, ModelMatrix, ViewMatrix, CameraPos, out var near, out var far);

            Near = near;
            Far = far;
        }

        var ptLightStructs = GLModel.PointLights
            .Where(r => r.Active)
            .Select(w => w.ToStruct(ModelMatrix))
            .ToArray();

        var shadowTransform = ptLightStructs.SelectMany(light =>
        {
            var lp = light.position;

            if (Near < 0 || Far < 0 || Near == 0 || Near > Far)
            {
                // Debug.WriteLine($"Invalid Near:{Near} Far:{Far}");
                Near = DEFAULT_Near;
                Far = DEFAULT_Far;
            }

            var prjMatrix = Matrix4x4.CreatePerspectiveFieldOfView((float)90d.ToRad(),
                (float)ShadowWidth / (float)ShadowHeight,
                nearPlaneDistance: Near,
                farPlaneDistance: Far);

            return new[]
            {
                new GLMatrix4x4Struct { m = Matrix4x4.CreateLookAt(lp, lp + Vector3.UnitX, -Vector3.UnitY) * prjMatrix },
                new GLMatrix4x4Struct { m = Matrix4x4.CreateLookAt(lp, lp - Vector3.UnitX, -Vector3.UnitY) * prjMatrix },
                new GLMatrix4x4Struct { m = Matrix4x4.CreateLookAt(lp, lp + Vector3.UnitY, Vector3.UnitZ) * prjMatrix },
                new GLMatrix4x4Struct { m = Matrix4x4.CreateLookAt(lp, lp - Vector3.UnitY, -Vector3.UnitZ) * prjMatrix },
                new GLMatrix4x4Struct { m = Matrix4x4.CreateLookAt(lp, lp + Vector3.UnitZ, -Vector3.UnitY) * prjMatrix },
                new GLMatrix4x4Struct { m = Matrix4x4.CreateLookAt(lp, lp - Vector3.UnitZ, -Vector3.UnitY) * prjMatrix }
            };
        }).ToArray();

        #region release previously allocated GL resources
        if (sizeChanged)
        {
            if (shadow_fb is not null)
            {
                if (shadow_texture is not null)
                {
                    GL.DeleteTexture(shadow_texture.Value);
                    shadow_texture = null;
                }

                GL.DeleteFramebuffer(shadow_fb.Value);

                shadow_fb = null;
            }

            if (main_fb is not null)
            {
                if (render_color is not null)
                {
                    GL.DeleteRenderbuffer(render_color.Value);
                    render_color = null;
                }

                if (render_depth is not null)
                {
                    GL.DeleteRenderbuffer(render_depth.Value);
                    render_depth = null;
                }

                GL.DeleteFramebuffer(main_fb.Value);

                main_fb = null;
            }
        }
        #endregion

        GL.PolygonMode(GLEnum.FrontAndBack, Wireframe ? PolygonMode.Line : PolygonMode.Fill);

        //--------------------------------------------------------------
        // SHADOW render
        //--------------------------------------------------------------

        if (GLModel.PointLights.Any(r => r.Active))
        {

            #region shadow (init)

            if (shadow_fb is null)
            {
                shadow_fb = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadow_fb.Value);

                GL.ActiveTexture(SHADOW_TEXTURE_UNIT);
                shadow_texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMapArray, shadow_texture.Value);

                GL.TexParameterI(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
                GL.TexParameterI(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
                GL.TexParameterI(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
                GL.TexParameterI(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
                GL.TexParameterI(TextureTarget.TextureCubeMapArray, TextureParameterName.TextureWrapR, (int)GLEnum.ClampToEdge);

                unsafe
                {
                    GL.TexImage3D(TextureTarget.TextureCubeMapArray,
                        0, // level
                        InternalFormat.DepthComponent,
                        ShadowWidth, ShadowHeight,
                        (uint)(6 * ptLightStructs.Length), // the number of layers in a texture array
                        0, // border
                        PixelFormat.DepthComponent,
                        PixelType.Float,
                        (void*)0);
                }

                // attach image to shadow framebuffer
                GL.FramebufferTexture(FramebufferTarget.Framebuffer,
                    FramebufferAttachment.DepthAttachment,
                    shadow_texture.Value,
                    0); // level         

                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);

                DebugFramebufferStatus("SHADOW INIT");
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadow_fb.Value);

                GL.ActiveTexture(SHADOW_TEXTURE_UNIT);
                GL.BindTexture(TextureTarget.TextureCubeMapArray, shadow_texture!.Value);
            }

            #endregion

            #region shadow render

            if (UseShadow)
            {
                DepthShader.Use();
                DepthShader.SetMat4(UNINAME_uModel, ModelMatrix);
                DepthShader.SetFloat(UNINAME_uFarPlaneLight, Far);

                DebugFramebufferStatus("SHADOW");

                GL.Viewport(0, 0, ShadowWidth, ShadowHeight);

                for (int lightIdx = 0; lightIdx < ptLightStructs.Length; ++lightIdx)
                {
                    DepthShader.SetInt(UNINAME_uLightIdx, lightIdx);

                    RenderScene(
                        fig => fig.PrimitiveType == GLPrimitiveType.Triangle,
                        DepthShader,
                        ptLightStructs,
                        shadowTransform,
                        clear: lightIdx == 0);
                }
            }
            #endregion

        }

        //--------------------------------------------------------------
        // MAIN render
        //--------------------------------------------------------------

        #region main init

        if (main_fb is null)
        {
            main_fb = GL.GenFramebuffer();

            // Debug.WriteLine($"---> main_fb: {main_fb}");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, main_fb.Value);

            // //---                

            render_color = GL.GenRenderbuffer();

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, render_color.Value);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.Rgba, w, h);
            GL.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                RenderbufferTarget.Renderbuffer,
                render_color.Value);

            // //---

            render_depth = GL.GenRenderbuffer();

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, render_depth.Value);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat.DepthComponent, w, h);
            GL.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                RenderbufferTarget.Renderbuffer,
                render_depth.Value);
        }
        else
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, main_fb.Value);
        }
        #endregion

        #region main render        
        if (main_fb is not null)
        {
            GL.Viewport(0, 0, w, h);

            // MainShader.Use();
            // Clear(shadowMode: false, ClearColor);

            DoShader(
                fig =>
                    fig.PrimitiveType != GLPrimitiveType.Triangle &&
                    (ControlFigureVisible is null || ControlFigureVisible(this, fig)),
                MainShader,
                ptLightStructs);

            if (!Wireframe && ShadeWithEdge)
                DoShader(
                    fig =>
                        fig.PrimitiveType == GLPrimitiveType.Triangle &&
                        !fig.ExcludeFromShadeWithEdge &&
                        (ControlFigureVisible is null || ControlFigureVisible(this, fig)),
                    ShadeWithEdgeShader,
                    ptLightStructs,
                    clear: false);

            if (ShowNormals)
                DoShader(
                    fig =>
                        fig.PrimitiveType == GLPrimitiveType.Triangle &&
                        (ControlFigureVisible is null || ControlFigureVisible(this, fig)),
                    NormalShader,
                    ptLightStructs,
                    clear: false);

            DoShader(
                fig =>
                    fig.PrimitiveType == GLPrimitiveType.Triangle &&
                    (ControlFigureVisible is null || ControlFigureVisible(this, fig)),
                MainShader,
                ptLightStructs,
                clear: false);
        }
        #endregion

        // GL.Flush(); 

        //--------------------------------------------------------------
        // save GL pixels
        //--------------------------------------------------------------

        device.TransferGLPixels(GL);

        {
            LastRenderTime = (float)(DateTime.Now - dtLastRenderCompleted).TotalSeconds;
            dtLastRenderCompleted = DateTime.Now;

            FrameRate = 1f / LastRenderTime;
        }

        IsRenderInvalidated = false;

        IsControlInvalidated = false;

        ++RenderCount;
        // Debug.WriteLine($"render cnt:{RenderCount}");

        IsRendering = false;
    }

    /// <summary>
    /// Toggle identify vertex coord.
    /// </summary>    
    public void ToggleIdentifyCoord() => IdentifyCoord = !IdentifyCoord;

    /// <summary>
    /// Toggle display of model bounding box ( generated at runtime through a temporary vertex manager ).
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    public void ToggleShowModelBBox(bool invalidate = DEFAULT_INVALIDATE)
    {
        ShowModelBBox = !ShowModelBBox;
        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Toggle display of camera frustum object ( generated at runtime through a temporary vertex manager ).
    /// </summary>
    /// <param name="invalidate">If true refresh the scene.</param>
    public void ToggleCameraObject(bool invalidate = DEFAULT_INVALIDATE)
    {
        ShowCameraObject = !ShowCameraObject;
        if (invalidate) Invalidate();
    }

    /// <summary>
    /// Create a raycast Line suitable for hit test of figure primitives.
    /// </summary>
    /// <param name="screen">Screen coord (x,y) [screen].</param>
    /// <example>
    /// [<b>Example</b>](https://github.com/devel0/netcore-opengl/blob/master/examples/example-0007/Program.cs)
    /// \snippet{'trimleft'} example-0007/Program.cs RayCast
    /// </example>
    /// <returns></returns>
    public Line RayCastLocal(in Vector2 screen) =>
        Core.Toolkit.RayCastLocal(screen, Size(), ModelMatrix, ViewMatrix, ProjectionMatrix, CameraPos, Perspective);
}
