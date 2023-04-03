using Avalonia.Controls;

namespace example.Views;

// example-0002
// random lines ( avalonia AXAML program )
//
// - click added desktop button to invalidate with rebuild of the scene

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

    public MainWindow()
    {
        InitializeComponent();

        Title = AppDomain.CurrentDomain.FriendlyName;

        var glCtx = new GLContext();
        glModel = new GLModel(glCtx);
        glModel.BuildModel = (glCtl, isInitial) =>
        {
            var glModel = glCtl.GLModel;

            glModel.Clear();

            var L = 100;
            var rnd = new Random();
            var N = 100;

            {
                var xLine = GLLine.PointV(Vector3.Zero, new Vector3(L, 0, 0), Color.White, Color.Red);
                var yLine = GLLine.PointV(Vector3.Zero, new Vector3(0, L, 0), Color.White, Color.Green);
                var zLine = GLLine.PointV(Vector3.Zero, new Vector3(0, 0, L), Color.White, Color.Blue);

                glModel.AddFigure(new GLLineFigure(xLine, yLine, zLine));
            }

            var lines = new List<GLLine>();

            for (int i = 0; i < N; ++i)
            {
                var rnds = Enumerable.Range(0, 6).Select(r => rnd.NextSingle()).ToList();

                var color = Color.FromArgb((byte)(rnds[0] * 255), (byte)(rnds[1] * 255), (byte)(rnds[2] * 255));

                var line = GLLine.FromTo(
                    new Vector3(rnds[0] * L, rnds[1] * L, rnds[2] * L),
                    new Vector3(rnds[3] * L, rnds[4] * L, rnds[5] * L),
                    color);

                lines.Add(line);
            }

            glModel.AddFigure(new GLLineFigure(lines));

            if (isInitial) glCtl.CameraView(CameraViewType.Top);
        };

        // AvaloniaGLControlSplit is the root control that holds one or more GLControl
        // in the mvvm gl model there is the need to attach this control as a child of a given Grid
        this.AttachGLControlSplit(
            // grid where to place split ctl as a child
            grGL,
            // the GL Model
            glModel,
            // bind gl split to a local property for further usage
            setGLControlSplit: x => GLControlSplit = x);

        this.btnRandomLines.Click += (a, b) =>
        {
            // through gl split access to the focused gl control and call the invalidate with model rebuild            
            GLControlSplit?.Invalidate(InvalidateEnum.RebuildModelAndRedraw);
        };
    }

}