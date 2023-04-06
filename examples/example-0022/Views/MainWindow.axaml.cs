using Avalonia.Controls;
using GShark.Geometry;

namespace example.Views;

// example-0022
// TODO: doc

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

    #region Status

    private string _Status = "Ready.";
    /// <summary>
    /// Status
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

            glModel.AddFigure(MakeWCSFigure());

            var tube1 = new Cone(
                baseCS: (WCS * Matrix4x4.CreateRotationX((float)(PI / 2)) * Matrix4x4.CreateRotationZ(-(float)(PI / 4)))
                    .Move(-1f, -1f, 0),
                baseRadius: 1, topRadius: 1, height: 6,
                bottomCap: false, topCap: false).Figure();

            var tube2 = tube1.Mirror(YZCS)!;

            glModel.AddFigure(tube1);
            glModel.AddFigure(tube2);

            glModel.PointLights.Add(new GLPointLight(0, -1, 2));

            // grab tube vertexes near join as profile to sweep

            var profilePts = tube1.Vertexes().Where(r => r.Position.Length() < 2).Select(w => w.Position).ToList();
            var profileCenter = profilePts.Mean();

            glModel.AddFigure(new GLPointFigure(profilePts).SetColor(Color.Green));

            var railPts = new List<Vector3>();
            {
                var N = 10;
                var alpha = 0f;
                var alphaStep = (float)(PI / 2 / N);
                var rotCenter = new Vector3(0, -2f, 0);
                for (int i = 0; i < N + 1; ++i)
                {
                    railPts.Add(Vector3.Transform(profileCenter, Matrix4x4.CreateRotationZ(-alpha, rotCenter)));
                    alpha += alphaStep;
                }
            }

            var sweepNurb = NurbsSurface.FromSweep(
                new NurbsCurve(railPts.ToPoint3().ToList(), 1),
                new NurbsCurve(profilePts.ToPoint3().ToList(), 1));

            var railFig = new GLPointFigure(railPts).SetColor(Color.Yellow);
            glModel.AddFigure(railFig);

            var joinFig = sweepNurb.NurbToGL(Color.Red, N: 10);
            glModel.AddFigure(new GLTriangleFigure(joinFig));

            glCtl.LoadView();
        };

        this.AttachGLControlSplit(
            grGL,
            glModel,
            setGLControlSplit: x => GLControlSplit = x);
    }

    void ClearStatus() => Status = "Ready.";

}