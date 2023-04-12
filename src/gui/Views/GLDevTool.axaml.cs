using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ClosedXML.Excel;

namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Developer tool to inspect vertex manager at runtime.
/// </summary>
public partial class GLDevTool : Window, INotifyPropertyChanged
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

    public GLControl GLControl => glSplit.FocusedControl!.AvaloniaGLControl.GLControl;

    public GLModel GLModel => GLControl.GLModel;

    GLVertexManager vtxMgr => GLModel.GLVertexManager;

    GLVertexManager devVtxMgr = new GLVertexManager();

    #region HighlightedVertexes

    private ObservableCollection<PointTransformNfo> _HighlightedVertexes = new ObservableCollection<PointTransformNfo>();
    /// <summary>
    /// HighlightedVertexes
    /// </summary>
    public ObservableCollection<PointTransformNfo> HighlightedVertexes
    {
        get => _HighlightedVertexes;
        set
        {
            var changed = value != _HighlightedVertexes;
            if (changed)
            {
                _HighlightedVertexes = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    public GLDevTool()
    {
        InitializeComponent();
    }

    /// <summary>
    /// workaround avalonia
    /// </summary>
    private void cbpress(object? sender, PointerPressedEventArgs e)
    {
        if (sender is CheckBox cb) cb.IsChecked = !cb.IsChecked;
    }

    // GridSplitterManager<GLView> glSplit;
    AvaloniaGLControlSplit glSplit;

    #region AvaloniaGLControl

    private AvaloniaGLControl? _AvaloniaGLControl = null;
    /// <summary>
    /// AvaloniaGLControl
    /// </summary>
    public AvaloniaGLControl? AvaloniaGLControl
    {
        get => _AvaloniaGLControl;
        set
        {
            var changed = value != _AvaloniaGLControl;
            if (changed)
            {
                _AvaloniaGLControl = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LightPosWrap

    private Vector3Wrap? _LightPosWrap = null;
    /// <summary>
    /// LightPosWrap
    /// </summary>
    public Vector3Wrap? LightPosWrap
    {
        get => _LightPosWrap;
        set
        {
            var changed = value != _LightPosWrap;
            if (changed)
            {
                _LightPosWrap = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LightPosMinX

    private float _LightPosMinX = 0;
    /// <summary>
    /// LightPosMinX
    /// </summary>
    public float LightPosMinX
    {
        get => _LightPosMinX;
        set
        {
            var changed = value != _LightPosMinX;
            if (changed)
            {
                _LightPosMinX = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LightPosMaxX

    private float _LightPosMaxX = 1;
    /// <summary>
    /// LightPosMaxX
    /// </summary>
    public float LightPosMaxX
    {
        get => _LightPosMaxX;
        set
        {
            var changed = value != _LightPosMaxX;
            if (changed)
            {
                _LightPosMaxX = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LightPosMinY

    private float _LightPosMinY = 0;
    /// <summary>
    /// LightPosMinY
    /// </summary>
    public float LightPosMinY
    {
        get => _LightPosMinY;
        set
        {
            var changed = value != _LightPosMinY;
            if (changed)
            {
                _LightPosMinY = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LightPosMaxY

    private float _LightPosMaxY = 0;
    /// <summary>
    /// LightPosMaxY
    /// </summary>
    public float LightPosMaxY
    {
        get => _LightPosMaxY;
        set
        {
            var changed = value != _LightPosMaxY;
            if (changed)
            {
                _LightPosMaxY = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LightPosMinZ

    private float _LightPosMinZ = 0;
    /// <summary>
    /// LightPosMinZ
    /// </summary>
    public float LightPosMinZ
    {
        get => _LightPosMinZ;
        set
        {
            var changed = value != _LightPosMinZ;
            if (changed)
            {
                _LightPosMinZ = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region LightPosMaxZ

    private float _LightPosMaxZ = 0;
    /// <summary>
    /// LightPosMaxZ
    /// </summary>
    public float LightPosMaxZ
    {
        get => _LightPosMaxZ;
        set
        {
            var changed = value != _LightPosMaxZ;
            if (changed)
            {
                _LightPosMaxZ = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    public GLDevTool(AvaloniaGLControlSplit glSplit)
    {
        InitializeComponent();

        this.glSplit = glSplit;

        AvaloniaGLControl = glSplit.FocusedControl?.AvaloniaGLControl;
        this.DataContext = glSplit.FocusedControl?.AvaloniaGLControl.GLControl;

        glSplit.PropertyChanged += (a, b) =>
        {
            if (b.PropertyName == nameof(GridSplitterManager<GLView>.FocusedControl))
            {
                AvaloniaGLControl = glSplit.FocusedControl?.AvaloniaGLControl;
                this.DataContext = glSplit.FocusedControl?.AvaloniaGLControl.GLControl;
            }
        };

        GLModel.PointLights.CollectionChanged += GLModel_LightsCollectionChanged;

        // GLModel.PointLights.Clearing += GLModel_LightsClearing; // TODO: interaction
        foreach (var light in GLModel.PointLights) light.PropertyChanged += GLModel_LightPropertyChanged;
        GLControl.PropertyChanged += GLControl_PropertyChanged;

        GLModel.AddCustomVertexManager(devVtxMgr);
        dgFigures.SelectionChanged += dgFigure_SelectionChanged;
        dgVertexes.SelectionChanged += dgVertexes_SelectionChanged;

        GLModel.ViewInvalidated += (model) => GLControl.Invalidate();

        CbHighlight.IsCheckedChanged += (a, b) => RefreshHighlightedFigures();

        dgLights.SelectionChanged += dgLights_SelectionChanged;

        this.Closed += GLDevTool_Closed;
    }

    private async void ExportDxfClick(object? sender, RoutedEventArgs e)
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>()
            {
                new FilePickerFileType("dxf") { Patterns = new List<string>() { "*.dxf" } }
            }
        });

        if (file is not null)
        {
            try
            {
                var dxf = new netDxf.DxfDocument();
                var glModel = GLControl.GLModel;

                foreach (var fig in glModel.Figures.Where(r => r.Visible))
                {
                    switch (fig.PrimitiveType)
                    {
                        case GLPrimitiveType.Point:
                            {
                                foreach (var pt in fig.Primitives.OfType<GLPoint>())
                                    dxf.Entities.Add(pt.ToDxfPoint());
                            }
                            break;

                        case GLPrimitiveType.Line:
                            {
                                foreach (var line in fig.Primitives.OfType<GLLine>())
                                    dxf.Entities.Add(line.ToDxfLine());
                            }
                            break;

                        case GLPrimitiveType.Triangle:
                            {
                                foreach (var tri in fig.Primitives.OfType<GLTriangle>())
                                    dxf.Entities.Add(tri.ToDxfFace3D());
                            }
                            break;
                    }
                }

                dxf.Save(file.Path.AbsolutePath, isBinary: true);

                GLControl.SendNotification("Export dxf", $"Saved to [{file.Path.AbsolutePath}]", GLNotificationType.Success);
            }
            catch (Exception ex)
            {
                GLControl.SendNotification("Export dxf", $"Error saving to [{file.Path.AbsolutePath}]\n{ex.Message}", GLNotificationType.Error);
            }
        }
    }

    private void dgLights_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var lbbox = GLModel.LBBox;

        LightPosMinX = lbbox.Min.X - lbbox.Size.X * 1e1f;
        LightPosMaxX = lbbox.Max.X + lbbox.Size.X * 1e1f;

        LightPosMinY = lbbox.Min.Y - lbbox.Size.Y * 1e1f;
        LightPosMaxY = lbbox.Max.Y + lbbox.Size.Y * 1e1f;

        LightPosMinZ = lbbox.Min.Z - lbbox.Size.Z * 1e1f;
        LightPosMaxZ = lbbox.Max.Z + lbbox.Size.Z * 1e1f;

        if (dgLights.SelectedItem is GLPointLight pl)
        {
            LightPosWrap = new Vector3Wrap(pl, nameof(GLPointLight.Position));
        }
    }

    private void PointLightsAddClick(object? sender, RoutedEventArgs e)
    {
        GLModel.PointLights.Add(new GLPointLight());
    }

    private void PointLightsDelClick(object? sender, RoutedEventArgs e)
    {
        var selectedLight = dgLights.SelectedItems.OfType<GLPointLight>().FirstOrDefault();
        if (selectedLight is null) return;

        GLModel.PointLights.Remove(selectedLight);
    }

    private void PointLightsResetClick(object? sender, RoutedEventArgs e)
    {
        GLModel.ResetLight();
    }

    private void SaveViewClick(object? sender, RoutedEventArgs e)
    {
        var glCtl = glSplit.FocusedControl?.AvaloniaGLControl.GLControl;
        glCtl?.SaveView();
    }

    private void RestoreViewClick(object? sender, RoutedEventArgs e)
    {
        var glCtl = glSplit.FocusedControl?.AvaloniaGLControl.GLControl;
        glCtl?.LoadView();
    }

    private void dgVertexes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        devVtxMgr.Clear();

        var selVtxGrps = dgVertexes.SelectedItems.OfType<PointTransformNfo>()
            .GroupBy(x => x.Vertex.ParentFigure)
            .ToList();

        if (selVtxGrps.Count > 0)
        {
            foreach (var selVtxGrpItem in selVtxGrps)
            {
                var fig = selVtxGrpItem.Key;
                var selVtx = selVtxGrpItem.First();

                var pointFig = new GLPointFigure(
                    new GLPoint(new GLVertex(selVtx.Vertex.Position, Color.Yellow)))
                {
                    ObjectMatrix = fig?.ObjectMatrix is null ? Matrix4x4.Identity : fig.ObjectMatrix,
                    PointSize = 10
                };

                if (fig is not null)
                    fig.PropertyChanged += (sender, e) =>
                    {
                        if (e.PropertyName == nameof(IGLFigure.ObjectMatrix) && sender is IGLFigure fig)
                        {
                            pointFig.ObjectMatrix = fig.ObjectMatrix;
                        }
                    };

                devVtxMgr.AddFigure(pointFig);
            }
        }
        GLControl.Invalidate();
    }

    void RefreshHighlightedFigures()
    {                
        var selectedFigures = dgFigures.SelectedItems.OfType<GLFigureBase>().ToHashSet();

        foreach (var fig in GLModel.Figures)
        {
            fig.Highlight = CbHighlight.IsChecked == true && selectedFigures.Contains(fig);
        }

        var vertexes = selectedFigures
            .SelectMany(w => w.Vertexes())
            .GroupBy(w => w.Position.ToString())
            .Select(w => w.First());

        HighlightedVertexes = new ObservableCollection<PointTransformNfo>();
        foreach (var vertex in vertexes)
            HighlightedVertexes.Add(new PointTransformNfo(GLControl, vertex));

        glSplit.Invalidate();
    }

    private async void ExportXlsxClick(object? sender, RoutedEventArgs e)
    {
        var file = await this.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            FileTypeChoices = new List<FilePickerFileType>()
            {
                new FilePickerFileType("xlsx") { Patterns = new List<string>() { "*.xlsx" } }
            }
        });

        if (file is not null)
        {
            SetupClosedXMLFonts();

            var wb = new XLWorkbook();

            var pathfilename = file.Path.AbsolutePath;

            var ws = wb.AddWorksheet("Vertexes");

            var r = 1;
            var c = 1;

            IXLCell cell = ws.Cell(r, c);

            void setCell(int r, int c, string val, bool bold = false)
            {
                cell = ws.Cell(r, c);
                cell.Value = val;
                if (bold) cell.Style.Font.SetBold();
            }

            var colNormal = c; setCell(r, c, "Normal", bold: true); c += 3;
            var colObject = c; setCell(r, c, "Object", bold: true); c += 3;
            var colLocal = c; setCell(r, c, "Local", bold: true); c += 3;
            var colWorld = c; setCell(r, c, "World", bold: true); c += 3;
            var colEye = c; setCell(r, c, "Eye", bold: true); c += 3;
            var colClip = c; setCell(r, c, "Clip", bold: true); c += 4;
            var colNDC = c; setCell(r, c, "NDC", bold: true); c += 3;
            var colScreen = c; setCell(r, c, "Screen", bold: true); c += 2;

            ++r;

            void DumpVec2(int r, int c, in Vector2 v)
            {
                setCell(r, c, Invariant($"{v.X}"));
                setCell(r, c + 1, Invariant($"{v.Y}"));
            }

            void DumpVec3(int r, int c, in Vector3 v)
            {
                setCell(r, c, Invariant($"{v.X}"));
                setCell(r, c + 1, Invariant($"{v.Y}"));
                setCell(r, c + 2, Invariant($"{v.Z}"));
            }

            void DumpVec4(int r, int c, in Vector4 v)
            {
                setCell(r, c, Invariant($"{v.X}"));
                setCell(r, c + 1, Invariant($"{v.Y}"));
                setCell(r, c + 2, Invariant($"{v.Z}"));
                setCell(r, c + 3, Invariant($"{v.W}"));
            }

            foreach (var vtx in HighlightedVertexes)
            {
                DumpVec3(r, colNormal, vtx.Normal);
                DumpVec3(r, colObject, vtx.Object);
                DumpVec3(r, colLocal, vtx.Local);
                DumpVec3(r, colWorld, vtx.World);
                DumpVec3(r, colEye, vtx.Eye);
                DumpVec4(r, colClip, vtx.Clip);
                DumpVec3(r, colNDC, vtx.NDC);
                DumpVec2(r, colScreen, vtx.Screen);

                ++r;
            }

            ws.FinalizeWorksheet();

            wb.SaveAs(pathfilename);

            var psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            psi.FileName = pathfilename;
            Process.Start(psi);
        }
    }

    private void dgFigure_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RefreshHighlightedFigures();
    }

    private void GLDevTool_Closed(object? sender, EventArgs e)
    {
    }

    private void GLControl_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is GLControl glControl)
        {
            if (glControl.IsControlInvalidated)
            {
                glControl.Invalidate();
            }
        }
    }


    private void GLModel_LightPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        GLControl.Invalidate();
    }

    private void GLModel_LightsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    if (e.OldItems is not null && e.OldItems.Count != 0) throw new InternalError($"expects empty olditems");
                    e.NewItems?.Cast<GLPointLight>().Foreach(light => light.PropertyChanged += GLModel_LightPropertyChanged);
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                {
                    if (e.NewItems is not null && e.NewItems.Count != 0) throw new InternalError($"expects empty newitems");
                    e.OldItems?.Cast<GLPointLight>().Foreach(light => light.PropertyChanged -= GLModel_LightPropertyChanged);
                }
                break;

            default:
                GLControl.Invalidate();
                //FIXME - 
                //throw new NotImplementedException($"obc action {e.Action}");
                break;

        }
    }

    private void GLModel_LightsClearing(object? sender, IList<GLPointLight> items) =>
        items.Foreach(light => light.PropertyChanged -= GLModel_LightPropertyChanged);

}