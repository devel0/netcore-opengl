using Avalonia.Controls;
using Avalonia.Interactivity;

namespace example.Views;

// example-0021
// Use of raycast to pick vertexes and define a new ucs.
//
// - click on "Pick another CS" and select 3 non colinear points hovering the ellipse
// - enable sphere mesh click on "view mesh", the use "z" to zoomfit
// - click on "Pick another CS" and select 3 non colinear points hovering sphere vertexes
// - click on "Iddentify point" and hover on sphere vertex to see points under cs with negative z on cs coord
//   while wcs couldn't

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
    /// sample mesh
    /// </summary>
    GLTriangleFigure? sphereFig = null;

    /// <summary>
    /// sample planar fig
    /// </summary>
    GLFigureBase? ellipseFig = null;

    /// <summary>
    /// tag applied to cs figure
    /// </summary>
    object cs_tag = "cs";

    /// <summary>
    /// arrows diameter factor
    /// </summary>
    const float arrowDiameterFactor = .01f;

    /// <summary>
    /// vtx mgr where to draw vertex snaps
    /// </summary>    
    GLVertexManager tmpVtxMgr = new GLVertexManager(expandModelBBox: false);

    /// <summary>
    /// vtx mgr where to draw ucs
    /// </summary>    
    GLVertexManager csVtxMgr = new GLVertexManager(expandModelBBox: true);

    // following INotifyPropertyChanged properties are created using "propnc" snippet
    // from vscode ( see https://github.com/devel0/knowledge/blob/95708e64099220c326472bd66da56e73ac5e46a5/doc/vscode-snippets-csharp.json )

    #region ShowSphereFig

    private bool _ShowSphereFig = false;
    /// <summary>
    /// Toggle mesh visibility
    /// </summary>
    public bool ShowSphereFig
    {
        get => _ShowSphereFig;
        set
        {
            var changed = value != _ShowSphereFig;
            if (changed)
            {
                _ShowSphereFig = value;
                OnPropertyChanged();

                if (sphereFig is not null)
                {
                    sphereFig.Visible = value;
                    if (ellipseFig is not null)
                        ellipseFig.Visible = !sphereFig.Visible;
                }

                GLControlSplit?.Invalidate();
            }
        }
    }

    #endregion

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

                var cs_tris = MakeCSFigure(_UCS, solidArrowsDiameterFactor: arrowDiameterFactor)
                    .Primitives.OfType<GLTriangle>().ToList();

                var colorRedGreen = ((Color.Red.ToVector4() + Color.Green.ToVector4()) / 2).ToColor();

                cs_tris.AddRange(new Plate(
                    _UCS.Origin(),
                    _UCS.Origin() + _UCS.BaseX(),
                    _UCS.Origin() + _UCS.BaseX() + _UCS.BaseY(),
                    _UCS.Origin() + _UCS.BaseY())
                    .GetTriangles(Color.White, Color.Red, colorRedGreen, Color.Green));

                newCSFig = new GLTriangleFigure(cs_tris);
                newCSFig.ExcludeFromShadeWithEdge = true;
                newCSFig.Tag = cs_tag;

                csVtxMgr.AddFigure(newCSFig);

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

    public MainWindow()
    {
        InitializeComponent();

        Title = AppDomain.CurrentDomain.FriendlyName;

        Width = DEFAULT_WINDOW_WIDTH;
        Height = DEFAULT_WINDOW_HEIGHT;

        var glCtx = new GLContext();
        glModel = new GLModel(glCtx);
        glModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            glModel.AddCustomVertexManager(tmpVtxMgr);
            glModel.AddCustomVertexManager(csVtxMgr);

            var ellipse = new Ellipse(WCS, radiusX: .1f, radiusY: .1f);
            ellipseFig = ellipse.Figure(50).SetColor(Color.Cyan);
            glModel.AddFigure(ellipseFig);

            newCSFig = MakeCSFigure(_UCS, solidArrowsDiameterFactor: arrowDiameterFactor);
            newCSFig.Tag = cs_tag;
            csVtxMgr.AddFigure(newCSFig);

            var sphere = new UVSphere(Vector3.Zero, radius: .5f);
            sphereFig = sphere.Figure(divisions: 8);
            sphereFig.Visible = this.ShowSphereFig;

            glModel.AddFigure(sphereFig);

            glCtl.LoadView();
        };

        this.AttachGLControlSplit(
            grGL,
            glModel,
            setGLControlSplit: x => GLControlSplit = x);

        this.KeyDown += MainWindow_KeyDown;
        this.PointerMoved += MainWindow_PointerMoved;
        this.PointerPressed += MainWindow_PointerPressed;
    }

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

    private void MainWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (this.GLControlSplit?.FocusedControl is not GLView glView) return;

        var cp = e.GetCurrentPoint(glView.AvaloniaGLControl);

        if (cp.Properties.IsLeftButtonPressed && e.ClickCount == 1)
        {
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
                    projDst = nfo.oraycast.Contains(5e-2f, vtx.Position, out var prj),
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

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (GLControlSplit?.FocusedControl is GLView glView)
            glView.AvaloniaGLControl.HandleKeyDown(e);        
    }

}