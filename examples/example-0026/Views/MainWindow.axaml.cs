using Avalonia.Controls;
using Avalonia.Interactivity;

namespace example.Views;

// example-0026
// Shows 2 triangle intersection and SimpleCmd management.
//
// - press 's' to enable primitive selection
// - click on red triangle
// - press 'ctrl+c' to copy SimpleCmd
// - press 'canc' to delete red triangle
// - click on lower textbox and paste simple cmd then press enter to recreate the triangle

public partial class MainWindow : Window, INotifyPropertyChanged
{

    #region property changed

    public new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    enum CommandType
    {
        cmdNone,

        cmdIdPoint,
        cmdPickUCS
    };

    GLModel glModel;

    /// <summary>
    /// last cmd activated
    /// </summary>
    CommandType cmdType = CommandType.cmdNone;

    /// <summary>
    /// last 3 pts given for pick ucs cmd
    /// </summary>
    Vector3?[] pickUCS = new Vector3?[3]; // O, X, Y

    /// <summary>
    /// if true it snap on vertex
    /// </summary>
    bool enableVertexPick = false;

    /// <summary>
    /// last nearest vertex hitted on test
    /// </summary>
    GLVertex? lastVertexHitTest = null;

    /// <summary>
    /// actual ucs
    /// </summary>
    GLFigureBase? newCSFig = null;

    /// <summary>
    /// tag applied to cs figure
    /// </summary>
    object cs_tag = "cs";

    /// <summary>
    /// vtx mgr where to draw vertex snaps
    /// </summary>    
    GLVertexManager tmpVtxMgr = new GLVertexManager(expandModelBBox: false);

    /// <summary>
    /// vtx mgr where to draw ucs
    /// </summary>    
    GLVertexManager csVtxMgr = new GLVertexManager(expandModelBBox: true);

    #region UCS

    private Matrix4x4 _UCS = WCS;
    /// <summary>
    /// current ucs ( changing will redraw cs )
    /// </summary>
    public Matrix4x4 UCS
    {
        get => _UCS;
        set
        {
            var changed = value != _UCS;
            if (changed)
            {
                _UCS = value;
                OnPropertyChanged();

                csVtxMgr.Clear();

                csVtxMgr.AddFigure(MakeCSFigure(_UCS));

                foreach (var p in pickUCS)
                {
                    var glPoint = new GLPoint(p!.Value, Color.Green);

                    csVtxMgr.AddFigure(new GLPointFigure(glPoint));
                }

                GLControlSplit?.Invalidate();
            }
        }
    }

    #endregion

    #region Status

    private string _Status = "Ready.";
    /// <summary>
    /// UI status bar message.
    /// </summary>
    public string Status
    {
        get => _Status;
        set
        {
            var changed = value != _Status;
            if (changed)
            {
                _Status = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region GLControlSplit

    private AvaloniaGLControlSplit? _GLControlSplit = null;
    /// <summary>
    /// GLControlSplit
    /// </summary>
    public AvaloniaGLControlSplit? GLControlSplit
    {
        get => _GLControlSplit;
        set
        {
            var changed = value != _GLControlSplit;
            if (changed)
            {
                _GLControlSplit = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region WCSCoord_X

    private float _WCSCoord_X = 0;
    /// <summary>
    /// UI cs wcs x coord of hovered vertex.
    /// </summary>
    public float WCSCoord_X
    {
        get => _WCSCoord_X;
        set
        {
            var changed = value != _WCSCoord_X;
            if (changed)
            {
                _WCSCoord_X = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region WCSCoord_Y

    private float _WCSCoord_Y = 0;
    /// <summary>
    /// UI cs wcs y coord of hovered vertex.
    /// </summary>
    public float WCSCoord_Y
    {
        get => _WCSCoord_Y;
        set
        {
            var changed = value != _WCSCoord_Y;
            if (changed)
            {
                _WCSCoord_Y = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region WCSCoord_Z

    private float _WCSCoord_Z = 0;
    /// <summary>
    /// UI cs wcs z coord of hovered vertex.
    /// </summary>
    public float WCSCoord_Z
    {
        get => _WCSCoord_Z;
        set
        {
            var changed = value != _WCSCoord_Z;
            if (changed)
            {
                _WCSCoord_Z = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region CSCoord_X

    private float _CSCoord_X = 0;
    /// <summary>
    /// UI cs cs x coord of hovered vertex.
    /// </summary>
    public float CSCoord_X
    {
        get => _CSCoord_X;
        set
        {
            var changed = value != _CSCoord_X;
            if (changed)
            {
                _CSCoord_X = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region CSCoord_Y

    private float _CSCoord_Y = 0;
    /// <summary>
    /// UI cs cs y coord of hovered vertex.
    /// </summary>
    public float CSCoord_Y
    {
        get => _CSCoord_Y;
        set
        {
            var changed = value != _CSCoord_Y;
            if (changed)
            {
                _CSCoord_Y = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region CSCoord_Z

    private float _CSCoord_Z = 0;
    /// <summary>
    /// UI cs cs z coord of hovered vertex.
    /// </summary>
    public float CSCoord_Z
    {
        get => _CSCoord_Z;
        set
        {
            var changed = value != _CSCoord_Z;
            if (changed)
            {
                _CSCoord_Z = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region CommandLine

    private string _CommandLine = "";
    /// <summary>
    /// CommandLine
    /// </summary>
    public string CommandLine
    {
        get => _CommandLine;
        set
        {
            var changed = value != _CommandLine;
            if (changed)
            {
                _CommandLine = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    public MainWindow()
    {
        InitializeComponent();

        Title = AppDomain.CurrentDomain.FriendlyName;

        var glCtx = new GLContext();
        glModel = new GLModel(glCtx);
        glModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            glModel.AddCustomVertexManager(tmpVtxMgr);
            glModel.AddCustomVertexManager(csVtxMgr);

            newCSFig = MakeCSFigure(_UCS).Act(r => r.ExpandBBox = false);
            newCSFig.Tag = cs_tag;
            csVtxMgr.AddFigure(newCSFig);

            var t1 = glModel.PrimitivesFromSimpleCmd("t 45.823437,-4.1765623,3.3747683,45.823437,-6.038086,2.529329,47.83262,-6.038086,3.2898018")
                .OfType<GLTriangle>().First();

            var t2 = glModel.PrimitivesFromSimpleCmd("t 46.25,-5,3.015,46.25,-6.25,3.01875,47.5,-6.25,3.0125")
                .OfType<GLTriangle>().First();

            glModel.AddFigure(new GLTriangleFigure(t1).SetColor(Color.Red.SetAlphaF(.5f)));
            glModel.AddFigure(t1.LBBox.MakeFigure(Color.Red));

            glModel.AddFigure(new GLTriangleFigure(t2).SetColor(Color.Green.SetAlphaF(.5f)));
            glModel.AddFigure(t2.LBBox.MakeFigure(Color.Green));

            var iline = t1.Intersect(0.051f, t2, out var _);
            if (iline is not null)
                glModel.AddFigure(new[] { iline }.ToFigure());
        };

        this.AttachGLControlSplit(
            grGL,
            glModel,
            setGLControlSplit: x => GLControlSplit = x);

        this.KeyDown += MainWindow_KeyDown;
        this.PointerMoved += MainWindow_PointerMoved;
        this.PointerPressed += MainWindow_PointerPressed;
        this.PointerReleased += MainWindow_PointerRelease;

        cmdTbox.KeyDown += cmdTbox_KeyDown;
    }

    private void cmdTbox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var cmd = cmdTbox.Text;
            if (cmd is not null && cmd.Trim().Length > 0)
            {
                var figs = this.glModel.FiguresFromSimpleCmd(cmd).ToList();

                foreach (var fig in figs)
                    fig.Order = 1;

                this.glModel.AddFigure(figs);

                GLControlSplit?.Invalidate();
            }
        }
    }

    #region based on example-0021

    private void IdPoint_Click(object? sender, RoutedEventArgs e)
    {
        cmdType = CommandType.cmdIdPoint;
        ProcessCmd();
    }

    private void PickCS_Click(object? sender, RoutedEventArgs e)
    {
        cmdType = CommandType.cmdPickUCS;
        ProcessCmd();
    }

    private void ClearCmdClick(object? sender, RoutedEventArgs e)
    {
        CommandLine = "";
    }

    void resetPickUCS() => pickUCS = new Vector3?[] { Vector3.Zero, Vector3.UnitX, Vector3.UnitY };

    private void RestoreWCS_Click(object? sender, RoutedEventArgs e)
    {
        resetPickUCS();
        UCS = WCS;
    }

    void CancelCommand()
    {
        switch (cmdType)
        {
            case CommandType.cmdIdPoint:
            case CommandType.cmdPickUCS:
                pickUCS = new Vector3?[3];
                enableVertexPick = false;
                break;
        }
        cmdType = CommandType.cmdNone;

        ClearStatus();
    }

    void ProcessCmd()
    {
        switch (cmdType)
        {
            #region cmdIdPoint

            case CommandType.cmdIdPoint:
                {
                    Status = "Move over vertexes";
                    enableVertexPick = true;
                }
                break;

            #endregion

            //---

            #region cmdPickUCS

            case CommandType.cmdPickUCS:
                {
                    if (pickUCS[0] is null)
                    {
                        Status = "Pick a vertex origin:";
                        enableVertexPick = true;
                    }
                    else if (pickUCS[1] is null)
                    {
                        Status = "Pick a basex vertex:";
                        enableVertexPick = true;
                    }
                    else if (pickUCS[2] is null)
                    {
                        Status = "Pick a basex non parallel vertex:";
                        enableVertexPick = true;
                    }
                    else
                    {
                        var origin = pickUCS[0]!.Value;
                        var p1 = pickUCS[1]!.Value;
                        var p2 = pickUCS[2]!.Value;

                        UCS = MakeCS(origin, p1 - origin, p2 - origin, makeOrthonormalization: true);
                        enableVertexPick = false;

                        pickUCS = new Vector3?[3];
                        cmdType = CommandType.cmdNone;
                        ClearStatus();
                    }
                }
                break;
                #endregion

        }
    }    

    DateTime? dtLastLeftPointerPressed = null;

    private void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (this.GLControlSplit?.FocusedControl is not GLView glView) return;

        var cp = e.GetCurrentPoint(glView.AvaloniaGLControl);

        // save timestamp to detect if it is a click instead of a pan
        if (cp.Properties.IsLeftButtonPressed)
            dtLastLeftPointerPressed = DateTime.Now;
    }

    private void MainWindow_PointerRelease(object? sender, PointerReleasedEventArgs e)
    {
        // if not a click if could a pan, in that case return
        if (dtLastLeftPointerPressed is null || (DateTime.Now - dtLastLeftPointerPressed.Value).TotalMilliseconds > 250) return;

        switch (cmdType)
        {
            case CommandType.cmdPickUCS:
                {
                    if (lastVertexHitTest is not null)
                    {
                        var pickUcsIdxList = pickUCS.WithIndex().Where(r => r.item is null).ToList();
                        if (pickUcsIdxList is not null)
                        {
                            // store picked wcs vertex transforming from GLVertex figure objectmatrix if any
                            pickUCS[pickUcsIdxList[0].idx] =
                                Vector3.Transform(lastVertexHitTest.Position, lastVertexHitTest.ParentFigure!.ObjectMatrix);
                            ProcessCmd();
                        }
                    }
                }
                break;
        }
    }

    private void MainWindow_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (this.GLControlSplit?.FocusedControl is not GLView glView) return;

        tmpVtxMgr.Clear();
        lastVertexHitTest = null;

        var avaloniaGLControl = glView.AvaloniaGLControl;

        if (enableVertexPick)
        {
            // retrieve point coord relative to focused avalonia 
            var p = e.GetPosition(avaloniaGLControl).ToVector2();

            var glCtl = avaloniaGLControl.GLControl;

            var lraycast = glCtl.RayCastLocal(screen: p);

            var tol = glModel.LBBox.TolHint;

            // - gather all vertexes visible figure that isn't cs itself
            // - grouped by figure
            // - converting lraycast to oraycast by figure objectmatrix
            // - selecting those vertexes which distance from the oraycast lower a tolerance (5e-2f)
            //   saving ocoord of the vertex and projection distance resulting from the contains test
            // - sort by the projection distance
            var q = glModel.Figures
                .Where(fig => fig.Visible && fig.Tag != cs_tag)
                .SelectMany(fig => fig.Vertexes().Select(vtx => new { fig = fig, vtx = vtx }))
                .GroupBy(nfo => nfo.fig)
                .Select(nfo => new
                {
                    vtxs = nfo.Select(v => v.vtx),
                    fig = nfo.Key,
                    oraycast = nfo.Key.ObjectMatrixIsIdentity ? lraycast : lraycast.Transform(nfo.Key.ObjectMatrix.Inverse())
                })
                .SelectMany(nfo => nfo.vtxs
                .Select(vtx => new
                {
                    projDst = nfo.oraycast.Contains(tol, vtx.Position, out var prj),
                    ocoord = Vector3.Transform(vtx.Position, nfo.fig.ObjectMatrix.Inverse()),
                    vtx = vtx
                })
                .Where(nfo => nfo.projDst is not null)
                .OrderBy(nfo => nfo.projDst))
                .ToList();

            if (q.Count > 0)
            {
                // set lastVertexHisTest to the vertex which the projected distance to the oraycast is the smaller one
                lastVertexHitTest = q[0].vtx;
                var wcs_coord = q[0].ocoord;
                var cs_coord = wcs_coord.ToUCS(UCS);

                WCSCoord_X = wcs_coord.X;
                WCSCoord_Y = wcs_coord.Y;
                WCSCoord_Z = wcs_coord.Z;

                CSCoord_X = cs_coord.X;
                CSCoord_Y = cs_coord.Y;
                CSCoord_Z = cs_coord.Z;

                // draw all vertex candidates white and magenta for the nearest
                for (int i = 0; i < q.Count; ++i)
                {
                    var nfo = q[i];

                    var glPoint = new GLPoint(nfo.ocoord, i == 0 ? Color.Magenta : Color.White);

                    // if (i == 0) Debug.WriteLine($"==> {glPoint.Vertex.Position}");

                    tmpVtxMgr.AddFigure(new GLPointFigure(glPoint));
                }
            }
        }

        GLControlSplit.Invalidate();
    }

    void ClearStatus() => Status = "Ready.";

    private void MainWindow_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CancelCommand();
        }
    }

    #endregion

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (GLControlSplit?.FocusedControl is GLView glView)
            glView.AvaloniaGLControl.HandleKeyDown(e);        
    }

}