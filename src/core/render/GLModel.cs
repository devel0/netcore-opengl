namespace SearchAThing.OpenGL.Core;

/// <summary>
/// Use for <see cref="GLModel.ViewInvalidated"/> event emitted when a vertex of the model change.
/// </summary>
/// <param name="model">GL model reference.</param>
public delegate void ModelViewInvalidated(GLModel model);

/// <summary>
/// Build model action that will executed each time the model requires a full rebuild because invalidated.
/// </summary>
/// <param name="glControl">GL control belonging to model that requires rebuild.</param>
/// <param name="initialCall">True is this is the first-est call to the build model.</param>
public delegate void GLBuildModelDelegate(GLControl glControl, bool initialCall);

/// <summary>
/// Used by the avalonia GL control to listen for notification coming from the model.<br/>
/// These notification will displayed using gui notification manager.
/// </summary>
/// <param name="title">Title of notification.</param>
/// <param name="msg">Message of notification.</param>
/// <param name="type">Icon/color type of notification.</param>
public delegate void NotificationDelegate(string title, string msg,
    GLNotificationType type = GLNotificationType.Information);

/// <summary>
/// Gl model.<br/>
/// - contains the associated <see cref="GLVertexManager"/><br/>
/// - the list of model figures <see cref="GLFigureBase"/><br/>
/// - point lights <see cref="GLPointLight"/>.<br/>
/// - custom vertex managers <see cref="AddCustomVertexManager"/><br/>
/// - contains the default build model action.<br/>
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public partial class GLModel : IGLContextObject
{

    #region property changed

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region IGLObject

    /// <summary>
    /// Gl context.
    /// </summary>    
    public GLContext GLContext { get; private set; }

    #endregion

    /// <summary>
    /// Event emitted when a vertex of the model changes.<br/>
    /// Useful to keep in sync the view calling the <see cref="GLControl.Invalidate"/>.
    /// See [<b>example-0016</b>](https://github.com/devel0/netcore-opengl/blob/master/examples/example-0016/Views/MainWindow.axaml.cs)
    /// </summary>
    public event ModelViewInvalidated? ViewInvalidated;

    [JsonProperty]
    ObservableCollection<GLFigureBase> FiguresOBC { get; set; } = new ObservableCollection<GLFigureBase>();

    #region Figures
    ReadOnlyObservableCollection<GLFigureBase>? _Figures = null;

    /// <summary>
    /// Figures (included those from custom vertex managers)
    /// </summary>    
    public ReadOnlyObservableCollection<GLFigureBase> Figures
    {
        get
        {
            if (_Figures is null)
                _Figures = new ReadOnlyObservableCollection<GLFigureBase>(FiguresOBC);

            return _Figures;
        }
        private set
        {
            var changed = value != _Figures;
            if (changed)
            {
                _Figures = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion    

    #region SelectedFigures

    ObservableCollection<GLFigureBase> SelectedFiguresOBC { get; set; } =
        new HSObservableCollection<GLFigureBase>();

    ReadOnlyObservableCollection<GLFigureBase>? _SelectedFigures = null;

    /// <summary>
    /// Selected figures.
    /// </summary>    
    public ReadOnlyObservableCollection<GLFigureBase> SelectedFigures
    {
        get
        {
            if (_SelectedFigures is null)
                _SelectedFigures = new ReadOnlyObservableCollection<GLFigureBase>(SelectedFiguresOBC);

            return _SelectedFigures;
        }
    }

    #endregion

    #region SelectedPrimitives

    ObservableCollection<GLPrimitiveBase> SelectedPrimitiveOBC { get; set; } =
        new HSObservableCollection<GLPrimitiveBase>();

    ReadOnlyObservableCollection<GLPrimitiveBase>? _SelectedPrimitives = null;

    /// <summary>
    /// Selected primitives.
    /// </summary>    
    public ReadOnlyObservableCollection<GLPrimitiveBase> SelectedPrimitives
    {
        get
        {
            if (_SelectedPrimitives is null)
                _SelectedPrimitives = new ReadOnlyObservableCollection<GLPrimitiveBase>(SelectedPrimitiveOBC);

            return _SelectedPrimitives;
        }
    }

    #endregion

    /// <summary>
    /// Event emitted when gl control would to notify something. This will handled by <see cref="SearchAThing.OpenGL.GUI.AvaloniaGLControl"/>.
    /// </summary>
    public NotificationDelegate NotificationRequest;

    /// <summary>
    /// Send notification to frontend that manage this gl control.
    /// </summary>
    /// <param name="title">Title of notification.</param>
    /// <param name="msg">Message to display, it can contains at most 1 newlines.</param>
    /// <param name="notifyType">Level type of notification (Default:Information).</param>
    public void SendNotification(string title, string msg, GLNotificationType notifyType = GLNotificationType.Information) =>
        NotificationRequest?.Invoke(title, msg, notifyType);

    #region GLVertexManager

    private GLVertexManager? _VtxMgr = null;

    /// <summary>
    /// Gl vertex manager.
    /// </summary>    
    public GLVertexManager GLVertexManager
    {
        get
        {
            if (_VtxMgr is null)
            {
                _VtxMgr = new GLVertexManager();
                _VtxMgr.FigureAdded += VtxMgr_FigureAdded;
                _VtxMgr.FigureRemoved += VtxMgr_FigureRemoved;
                _VtxMgr.FiguresCleared += VtxMgr_FiguresCleared;
            }
            return _VtxMgr;
        }
    }

    #endregion

    #region OverrideAmbient

    private float _OverrideAmbient = DEFAULT_MaterialProperties.Ambient;
    /// <summary>
    /// Override ambient strength (default:0.8).    
    /// </summary>
    /// <remarks>
    /// Requires <see cref="OverrideAmbient"/> to be effective.
    /// </remarks>
    /// <seealso cref="OverrideLightStrengths"/>    
    public float OverrideAmbient
    {
        get => _OverrideAmbient;
        set
        {
            var changed = value != _OverrideAmbient;
            if (changed)
            {
                _OverrideAmbient = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region OverrideAmbientEnabled

    private bool _OverrideAmbientEnabled = false;
    /// <summary>
    /// Override ambient strength enabled.    
    /// </summary>
    public bool OverrideAmbientEnabled
    {
        get => _OverrideAmbientEnabled;
        set
        {
            var changed = value != _OverrideAmbientEnabled;
            if (changed)
            {
                _OverrideAmbientEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region OverrideDiffuse

    private float _OverrideDiffuse = DEFAULT_MaterialProperties.Diffuse;
    /// <summary>
    /// Override diffuse strength (default:0.6).    
    /// </summary>
    /// <remarks>
    /// Requires <see cref="OverrideDiffuse"/> to be effective.
    /// </remarks>
    /// <seealso cref="OverrideLightStrengths"/>    
    public float OverrideDiffuse
    {
        get => _OverrideDiffuse;
        set
        {
            var changed = value != _OverrideDiffuse;
            if (changed)
            {
                _OverrideDiffuse = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region OverrideDiffuseEnabled

    private bool _OverrideDiffuseEnabled = false;
    /// <summary>
    /// Override diffuse strength enabled.
    /// </summary>
    public bool OverrideDiffuseEnabled
    {
        get => _OverrideDiffuseEnabled;
        set
        {
            var changed = value != _OverrideDiffuseEnabled;
            if (changed)
            {
                _OverrideDiffuseEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region OverrideSpecular

    private float _OverrideSpecular = DEFAULT_MaterialProperties.Specular;
    /// <summary>
    /// Override specular strength (default:0.6).    
    /// </summary>
    /// <remarks>
    /// Requires <see cref="OverrideSpecular"/> to be effective.
    /// </remarks>
    /// <seealso cref="OverrideLightStrengths"/>  
    public float OverrideSpecular
    {
        get => _OverrideSpecular;
        set
        {
            var changed = value != _OverrideSpecular;
            if (changed)
            {
                _OverrideSpecular = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region OverrideSpecularEnabled

    private bool _OverrideSpecularEnabled = false;
    /// <summary>
    /// Override specular strength enabled.
    /// </summary>
    public bool OverrideSpecularEnabled
    {
        get => _OverrideSpecularEnabled;
        set
        {
            var changed = value != _OverrideSpecularEnabled;
            if (changed)
            {
                _OverrideSpecularEnabled = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region IsInvalidated

    private bool _IsInvalidated = true;
    /// <summary>
    /// States if model is invalidated thus <see cref="BuildModel"/> will be automatically invoked at first render.
    /// </summary>
    public bool IsInvalidated
    {
        get => _IsInvalidated;
        set
        {
            var changed = value != _IsInvalidated;
            if (changed)
            {
                _IsInvalidated = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region NormalFactor

    private float _NormalFactor = DEFAULT_NORMAL_FACTOR;
    /// <summary>
    /// Factor respect to bbox size used when normal geometry shader generates normals.    
    /// </summary>
    /// <seealso cref="GLControl.ShowNormals"/>
    public float NormalFactor
    {
        get => _NormalFactor;
        set
        {
            var changed = value != _NormalFactor;
            if (changed)
            {
                _NormalFactor = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region SelectionMode

    private CursorMode _CursorMode = DEFAULT_CursorMode;
    /// <summary>
    /// Current cursor mode.<br/>    
    /// </summary>    
    /// <seealso cref="CursorMode"/>
    public CursorMode CursorMode
    {
        get => _CursorMode;
        set
        {
            var changed = value != _CursorMode;
            if (changed)
            {
                _CursorMode = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    List<GLVertexManager> _CustomVertexManagers = new List<GLVertexManager>();

    /// <summary>
    /// Enumerates custom vertex managers registered into this model.
    /// </summary>
    public IEnumerable<GLVertexManager> CustomVertexManagers => _CustomVertexManagers;

    /// <summary>
    /// Attach given custom vertex manager to the model.
    /// </summary>
    /// <param name="vertexManager">Gl vertex manager to attach.</param>
    public void AddCustomVertexManager(GLVertexManager vertexManager)
    {
        _CustomVertexManagers.Add(vertexManager);
        vertexManager.FigureAdded += VtxMgr_FigureAdded;
        vertexManager.FigureRemoved += VtxMgr_FigureRemoved;
        vertexManager.FiguresCleared += VtxMgr_FiguresCleared;
    }

    /// <summary>
    /// Detach given custom manager from the model.
    /// </summary>
    /// <param name="vertexManager">Gl vertex manager to detach.</param>
    public void RemoveCustomVertexManager(GLVertexManager vertexManager)
    {
        _CustomVertexManagers.Remove(vertexManager);
        vertexManager.FigureAdded -= VtxMgr_FigureAdded;
        vertexManager.FigureRemoved -= VtxMgr_FigureRemoved;
        vertexManager.FiguresCleared -= VtxMgr_FiguresCleared;
    }

    /// <summary>
    /// Observable collection of model point lights.
    /// </summary>
    /// <seealso cref="GLPointLight"/>
    [JsonProperty]
    public ObservableCollection2<GLPointLight> PointLights { get; private set; } =
        new ObservableCollection2<GLPointLight>();

    /// <summary>
    /// bbox of model vertexes [local].
    /// </summary>
    public BBox LBBox
    {
        get
        {
            if (_LBbox == null)
            {
                if (GLVertexManager.LBBox.IsEmpty) return new BBox();

                if (_CustomVertexManagers.Count > 0)
                    _LBbox = new BBox(new[] { GLVertexManager.LBBox }
                        .Union(CustomVertexManagers.Where(r => r.ExpandModelBBox).Select(r => r.LBBox)));

                else
                    _LBbox = GLVertexManager.LBBox;

                OnPropertyChanged();
            }

            return _LBbox;
        }
        private set
        {
            _LBbox = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Invalidate and recalc bounding box [local].<br/>
    /// Used internally after delete of figures/primitives.
    /// </summary>
    public void RecomputeLBBox()
    {
        LBBox = GLVertexManager.RecomputeLBBox();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="glContext">Gl context.</param>
    public GLModel(GLContext glContext)
    {
        GLContext = glContext;

        // Debug.WriteLine($"GLModel (hash:{GetHashCode()})");

        GLVertexManager.Invalidated += (vtxMgr) => InvalidateView();
    }

    private void VtxMgr_FigureAdded(IGLVertexManager vertexManager, GLFigureBase figure)
    {
        FiguresOBC.Add(figure);
        figure.FigureInvalidated += Figure_Invalidated;
        InvalidateBBox();
    }

    private void Figure_Invalidated(IGLFigure figure) => InvalidateView();

    private void VtxMgr_FigureRemoved(IGLVertexManager vertexManager, GLFigureBase figure)
    {
        FiguresOBC.Remove(figure);
        figure.FigureInvalidated -= Figure_Invalidated;
        InvalidateBBox();
    }

    private void VtxMgr_FiguresCleared(IGLVertexManager vertexManager, GLFigureBase[] figures)
    {
        foreach (var figure in figures) FiguresOBC.Remove(figure);
        InvalidateBBox();
    }

    /// <summary>
    /// Override vertex material properties.<br/>
    /// Has no effect if no light points present.<br/>    
    /// </summary>
    /// <param name="ambient">Ambient light strength (Default:0.8).</param>
    /// <param name="diffuse">Diffuse light strength (Default:0.6).</param>
    /// <param name="specular">Specular light strength (Default:0.6).</param>
    public void OverrideLightStrengths(float? ambient = null, float? diffuse = null, float? specular = null)
    {
        if (ambient is not null)
        {
            OverrideAmbient = ambient.Value;
            OverrideAmbientEnabled = true;
        }

        if (diffuse is not null)
        {
            OverrideDiffuse = diffuse.Value;
            OverrideDiffuseEnabled = true;
        }

        if (specular is not null)
        {
            OverrideSpecular = specular.Value;
            OverrideSpecularEnabled = true;
        }
    }

    /// <summary>
    /// Resets and create predefined lights
    /// </summary>
    public void ResetLight()
    {
        PointLights.Clear();

        var lbbox = LBBox;

        var color1 = ColorTranslator.FromHtml("#ffffff");

        var light1 = new GLPointLight(
            lbbox.Max.X + lbbox.Size.X / 3,
            lbbox.Min.Y,
            lbbox.Max.Z + 2.5f * lbbox.Size.Z,
            color1)
        { Constant = 1f };

        PointLights.Add(light1);
    }

    /// <summary>
    /// Configure light constant, linear, quadratic attenuation based on the model size.
    /// </summary>
    /// <param name="adjustConstant">Gl point light constant attenuation (Default:1f).</param>
    /// <param name="adjustLinear">Gl point light linear attenuation (Default:5e-1f).</param>
    /// <param name="adjustQuadratic">Gl point light quadratic attenuation (Default:1e-4).</param>
    public void SetupLightAttenuation(
        float? adjustConstant = DEFAULT_LIGHT_CONSTANT,
        float? adjustLinear = DEFAULT_LIGHT_LINEAR,
        float? adjustQuadratic = DEFAULT_LIGHT_QUADRATIC) =>
        PointLights.Foreach(light => light.SetupAttenuation(this, adjustConstant, adjustLinear, adjustQuadratic));

    BBox? _LBbox = null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InvalidateBBox()
    {
        _LBbox = null;
        OnPropertyChanged();
    }

    /// <summary>
    /// Clear the model removing all entities from vertex manager, deregistering custom vertex manager and clearing point lights if specified.
    /// </summary>
    /// <param name="clearPointLights">If true clear point lights (Default:false).</param>
    public void Clear(bool clearPointLights = false)
    {
        GLVertexManager.Clear();
        var customVtxMgrs = CustomVertexManagers.ToList();
        foreach (var customVtxMgr in customVtxMgrs) RemoveCustomVertexManager(customVtxMgr);

        if (clearPointLights) PointLights.Clear();
    }

    /// <summary>
    /// Adds given figures to the vertex manager.
    /// </summary>
    /// <param name="figures">Gl figures to add.</param>
    /// <seealso cref="GLVertexManager.AddFigure(IEnumerable{GLFigureBase})"/>
    public void AddFigure(IEnumerable<GLFigureBase> figures) => GLVertexManager.AddFigure(figures);

    /// <summary>
    /// Adds given figure to the vertex manager.
    /// </summary>
    /// <param name="figure">Gl figure to add.</param>
    /// <seealso cref="GLVertexManager.AddFigure(GLFigureBase)"/>
    public void AddFigure(GLFigureBase figure) => GLVertexManager.AddFigure(figure);

    /// <summary>
    /// Remove given figures from the vertex manager.
    /// </summary>
    /// <param name="figures">Gl figures to remove.</param>
    /// <seealso cref="GLVertexManager.RemoveFigure(GLFigureBase[])"/>
    /// <returns>Count of figures effectively removed.</returns>
    public int RemoveFigure(params GLFigureBase[] figures) => GLVertexManager.RemoveFigure(figures);

    /// <summary>
    /// Remove given figures from the vertex manager.
    /// </summary>
    /// <param name="figures">Gl figures to remove.</param>
    /// <seealso cref="GLVertexManager.RemoveFigure(IEnumerable{GLFigureBase})"/>
    /// <returns>Count of figures effectively removed.</returns>
    public int RemoveFigure(IEnumerable<GLFigureBase> figures) => GLVertexManager.RemoveFigure(figures);

    /// <summary>
    /// Remove given figure from the vertex manager.
    /// </summary>
    /// <param name="figure">Gl figure to remove.</param>
    /// <seealso cref="GLVertexManager.RemoveFigure(GLFigureBase)"/>
    /// <returns>True if figure effectively removed.</returns>
    public bool RemoveFigure(GLFigureBase figure) => GLVertexManager.RemoveFigure(figure);

    /// <summary>
    /// Enumerates gl vertexes [object].
    /// </summary>    
    public IEnumerable<GLVertex> Vertexes
    {
        get
        {
            foreach (var figure in Figures)
            {
                foreach (var primitive in figure.Primitives)
                {
                    foreach (var vertex in primitive.Vertexes)
                    {
                        yield return vertex;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Invalidate the gl model.<br/>        
    /// This cause <see cref="BuildModel"/> to be invoked at next render.
    /// </summary>    
    /// <seealso cref="IsInvalidated"/>.        
    public void InvalidateModel() => IsInvalidated = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void InvalidateView() => ViewInvalidated?.Invoke(this);

    bool buildModelIsInitialCall = true;

    internal bool EnsureModel(GLControl glControl)
    {
        if (IsInvalidated)
        {
            GLContext.AutoCleanup();

            Debug.WriteLine($"Build Model");

            FiguresOBC = new ObservableCollection<GLFigureBase>();

            _Figures = null;
            OnPropertyChanged(nameof(Figures));

            var initial = buildModelIsInitialCall;
            buildModelIsInitialCall = false;
            BuildModel?.Invoke(glControl, initial);

            IsInvalidated = false;
            InvalidateBBox();

            return true;
        }

        return false;
    }

    internal HashSet<GLControl> ShowCameraObjectControls = new HashSet<GLControl>();

    /// <summary>
    /// Build model action that will executed each time the model requires a full rebuild because invalidated.<br/>
    /// Arguments (<see cref="GLControl"/>, bool isInitial).
    /// </summary>
    public GLBuildModelDelegate? BuildModel = null;

    public override string? ToString()
    {
        var sb = new StringBuilder();

        sb.Append(GLVertexManager.ToString());

        return sb.ToString();
    }

    /// <summary>
    /// Load figure primitives from simple cmd ( newlines supported ).<br/>
    /// <seealso cref="GLPointFigure.SimpleCmd"/>
    /// <seealso cref="GLLineFigure.SimpleCmd"/>
    /// <seealso cref="GLTriangleFigure.SimpleCmd"/>
    /// </summary>
    public IEnumerable<GLPrimitiveBase> PrimitivesFromSimpleCmd(string simpleCmd)
    {
        foreach (var line in simpleCmd.Lines())
        {
            if (line.StartsWith(SIMPLE_CMD_POINT))
            {
                var pts = ParseVector3(line.StripBegin(SIMPLE_CMD_POINT).Trim(), pointCnt: 1);

                foreach (var p in pts) yield return new GLPoint(p);
            }

            else if (line.StartsWith(SIMPLE_CMD_LINE))
            {
                var pts = ParseVector3(line.StripBegin(SIMPLE_CMD_LINE).Trim(), pointCnt: 2).ToList();

                for (int i = 0; i < pts.Count; i += 2)
                {
                    var glline = GLLine.FromTo(pts[i], pts[i + 1]);
                    yield return glline;
                }
            }

            else if (line.StartsWith(SIMPLE_CMD_TRIANGLE))
            {
                var pts = ParseVector3(line.StripBegin(SIMPLE_CMD_TRIANGLE).Trim(), pointCnt: 3).ToList();

                for (int i = 0; i < pts.Count; i += 3)
                {
                    var tri = new GLTriangle(pts[i], pts[i + 1], pts[i + 2]);
                    yield return tri;
                }
            }
        }
    }

    /// <summary>
    /// Load primitives from given simgple cmd and group them by primitive type creating figures ( newlines supported ).<br/>
    /// <seealso cref="GLPointFigure.SimpleCmd"/>
    /// <seealso cref="GLLineFigure.SimpleCmd"/>
    /// <seealso cref="GLTriangleFigure.SimpleCmd"/>
    /// </summary>
    public IEnumerable<GLFigureBase> FiguresFromSimpleCmd(string simpleCmd)
    {
        var grps = PrimitivesFromSimpleCmd(simpleCmd)
            .GroupBy(primitive => primitive.PrimitiveType)
            .ToList();

        foreach (var grp in grps)
        {
            switch (grp.Key)
            {
                case GLPrimitiveType.Point:
                    yield return new GLPointFigure(grp.OfType<GLPoint>());
                    break;

                case GLPrimitiveType.Line:
                    yield return new GLLineFigure(grp.OfType<GLLine>());
                    break;

                case GLPrimitiveType.Triangle:
                    yield return new GLTriangleFigure(grp.OfType<GLTriangle>());
                    break;
            }
        }

    }

    /// <summary>
    /// Select/Deselect given primitives.
    /// </summary>
    /// <param name="primitives">Primitives for which toggle selection.</param>
    public void ToggleSelectPrimitives(IEnumerable<GLPrimitiveBase> primitives)
    {
        foreach (var primitive in primitives)
        {
            if (SelectedPrimitiveOBC.Contains(primitive))
                SelectedPrimitiveOBC.Remove(primitive);

            else
                SelectedPrimitiveOBC.Add(primitive);

            primitive.Selected = !primitive.Selected;

            foreach (var vtx in primitive.Vertexes)
            {
                vtx.ToggleFlags(GLVertexFlag.Selected);
            }
        }
    }

    /// <summary>
    /// Select/Deselect given figures.
    /// </summary>
    /// <param name="figures">Figures for which toggle selection.</param>
    public void ToggleSelectFigures(IEnumerable<GLFigureBase> figures)
    {
        foreach (var figure in figures)
        {
            if (SelectedFiguresOBC.Contains(figure))
                SelectedFiguresOBC.Remove(figure);

            else
                SelectedFiguresOBC.Add(figure);

            figure.Selected = !figure.Selected;
        }
    }

    /// <summary>
    /// Clear current selection ( of figures, primitives ) and cancel select command.
    /// </summary>
    /// <param name="resetToViewMode">If true (Default) cursor back to <see cref="CursorMode.View"/>.</param>
    public void ClearSelection(bool resetToViewMode = true)
    {
        {
            var selectedPrimitives = SelectedPrimitiveOBC.ToList();
            foreach (var primitive in selectedPrimitives)
            {
                foreach (var vtx in primitive.Vertexes)
                    vtx.ClearFlags(GLVertexFlag.Selected);
            }
            SelectedPrimitiveOBC.Clear();
        }

        {
            var selectedFigures = SelectedFiguresOBC.ToList();
            foreach (var figure in selectedFigures)
                figure.Selected = false;

            SelectedFiguresOBC.Clear();
        }

        CursorMode = CursorMode.View;
    }

    /// <summary>
    /// Retrieve <see cref="IGLPrimitive.SimpleCmd(bool)"/> representation of selected primitives and figure primitives.
    /// </summary>    
    public string GetSelectionSimpleCmd()
    {
        var sb = new StringBuilder();

        foreach (var primitive in SelectedPrimitiveOBC)
            sb.AppendLine(primitive.SimpleCmd());

        foreach (var figure in SelectedFiguresOBC)
            sb.AppendLine(figure.SimpleCmd());

        return sb.ToString();
    }

    /// <summary>
    /// Add to the model figures coming from given <see cref="IGLPrimitive.SimpleCmd(bool)"/>.<br/>
    /// Generated figures are primitives coming from the cmd grouped by primitive type.
    /// </summary>
    /// <param name="simpleCmd">Simple cmd representation.</param>
    /// <returns>Figures added to the model.</returns>
    public IEnumerable<IGLFigure> PasteSimpleCmd(string simpleCmd)
    {
        var figures = this.FiguresFromSimpleCmd(simpleCmd);

        AddFigure(figures);

        return figures;
    }

    /// <summary>
    /// Delete selected primitives/figures.
    /// </summary>
    public void DeleteSelected()
    {
        var primitivesToRemove = SelectedPrimitiveOBC.ToList();
        var figuresToRemove = SelectedFiguresOBC.ToList();

        RemoveFigure(figuresToRemove);

        foreach (var primitive in primitivesToRemove)
        {
            primitive.ParentFigure?.RemovePrimitive(primitive);
        }

        RecomputeLBBox();

        ClearSelection();
    }

}