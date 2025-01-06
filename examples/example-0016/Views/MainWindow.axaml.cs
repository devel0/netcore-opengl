using Avalonia.Controls;

namespace example.Views;

// example-0016
// reflect vertex change to control invalidation
//
// - move slider xyz to change vertex coord 
//   or use keyboard (1(x), 2(y), 3(z)) with shift to decrease
// - change selected vertex using numeric updown

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

    GLModel glModel;

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

    #region FocusedControl

    private GLControl? _FocusedControl = null;
    /// <summary>
    /// FocusedControl
    /// </summary>
    public GLControl? FocusedControl
    {
        get => _FocusedControl;
        set
        {
            var changed = value != _FocusedControl;
            if (changed)
            {
                _FocusedControl = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    // property used to bind VertexEdit control gui to selected vertex

    #region VertexEdit

    private VertexEdit? _VertexEdit = null;
    /// <summary>
    /// VertexEdit
    /// </summary>
    public VertexEdit? VertexEdit
    {
        get => _VertexEdit;
        set
        {
            var changed = value != _VertexEdit;
            if (changed)
            {
                _VertexEdit = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    GLTriangle? tri = null;

    // create a point (10pixel [screen]) which will be moved afterward
    // to highlight selected vertex

    GLPointFigure selectedVertexFig = new GLPointFigure(Vector3.Zero)
    {
        PointSize = 10,
        Order = 1
    };

    public MainWindow()
    {
        InitializeComponent();

        var q = this.Resources;

        Title = AppDomain.CurrentDomain.FriendlyName;

        Width = DEFAULT_WINDOW_WIDTH;
        Height = DEFAULT_WINDOW_HEIGHT;

        // light[0] : -1700,0,1100

        var glCtx = new GLContext();
        glModel = new GLModel(glCtx);
        glModel.BuildModel = (glCtl, isInitial) =>
        {
            if (!isInitial) return;

            var glModel = glCtl.GLModel;

            glModel.Clear();

            var a = new Vector3(-50, -50, 0);
            var b = new Vector3(50, -50, 0);
            var c = new Vector3(0, 50, 0);

            var va = new GLVertex(a, Color.Red);
            var vb = new GLVertex(b, Color.Green);
            var vc = new GLVertex(c, Color.Blue);

            tri = new GLTriangle(va, vb, vc);

            var fig = new GLTriangleFigure(tri);
            glModel.AddFigure(fig);

            glModel.AddFigure(selectedVertexFig);

            glCtl.CameraView(CameraViewType.Top);

            glModel.ViewInvalidated += (model) => glCtl.Invalidate();

            BindVtxMgr(0);
        };

        this.AttachGLControlSplit(grGL, glModel,
            x => GLControlSplit = x,
            onFocusedControlChanged: (glSplit, avaloniaGlControl, isInitial) => 
            
            FocusedControl = avaloniaGlControl?.GLControl);

        VertexIndexSelector.ValueChanged += VertexIndexSelector_ValueChanged;

        this.KeyDown += (sender, e) =>
        {
            if (VertexEdit is null) return;

            var val = 1f;
            if (e.KeyModifiers == KeyModifiers.Shift) val *= -1f;

            switch (e.Key)
            {
                case Key.D1: VertexEdit.X += val; break;
                case Key.D2: VertexEdit.Y += val; break;
                case Key.D3: VertexEdit.Z += val; break;
            }

        };
    }

    /// <summary>
    /// connect a VertexEdit gui editor ( 3 slider coord xyz )
    /// to model vertex of given index
    /// </summary>    
    void BindVtxMgr(int idx)
    {
        if (tri is not null)
        {
            var v = idx switch
            {
                0 => tri.V1,
                1 => tri.V2,
                2 => tri.V3,
                _ => throw new ArgumentException($"invalid vertex index")
            };

            // pass getter, setter methods to vertex edit gui ctrl
            // to retrieve Vector3 from GLVertex and to set Vector3 Position of GLVertex
            VertexEdit = new VertexEdit(
                () => v.Position,
                (vv) =>
                {
                    // update application point figure position
                    selectedVertexFig.Move(vv, relative: false);

                    v.Position = vv;
                });
        }
    }

    private void VertexIndexSelector_ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (VertexIndexSelector.Value is null) return;

        var idx = (int)VertexIndexSelector.Value;

        BindVtxMgr(idx);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (GLControlSplit?.FocusedControl is GLView glView)
            glView.AvaloniaGLControl.HandleKeyDown(e);        
    }

}