using Avalonia.Controls;

using static example.Toolkit;

namespace example.Views;

// example-0008
// draw nurb sphere or tube with lighting
//
// - slide light positions
// - toggle shade with edge using ctrl+w
// - toggle show normals using n

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

            // tube
            {
                var fig = Example0007(SURF_DIVS: 20);
                var figColor = new Vector3(0.27f, 0.38f, 0.58f);
                var figColor2 = new Vector3(0.5f, 0.5f, 0.5f);

                var VTR = 500 * Vector3.UnitZ;

                fig.ObjectMatrix = Matrix4x4.CreateTranslation(VTR);

                glModel.AddFigure(fig);
            }

            // base
            {
                var cuboidCenter = new Vector3(1000, 0, -50);
                var cuboidSize = new Vector3(2 * 4000, 2 * 3000, 100);

                var box = new Box(MakeCS(cuboidCenter - Vector3.UnitZ * cuboidSize.Z / 2, Vector3.UnitZ), cuboidSize, Color.White);
                glModel.AddFigure(box.Sides);
            }

            // light obstacle
            {
                var cuboidCenter = new Vector3(-1500, 0, 500);
                var cuboidSize = new Vector3(10, 2500, 1000);

                var box = new Box(MakeCS(cuboidCenter - Vector3.UnitZ * cuboidSize.Z / 2, Vector3.UnitZ), cuboidSize, Color.Pink);
                glModel.AddFigure(box.Sides);
            }

            var light1 = new GLPointLight(-1700, 0, 1100);
            light1.Constant = 10;
            light1.ShowPoint = true;

            var light2 = new GLPointLight(-1700, 500, 900);
            light2.Constant = 10;
            light2.ShowPoint = true;

            glModel.PointLights.Add(light1);
            glModel.PointLights.Add(light2);

            // setup light attenuation based on model bbox size            
            glModel.SetupLightAttenuation(adjustLinear: 5e-1f, adjustQuadratic: 1e-4f);
        };

        this.AttachGLControlSplit(grGL, glModel,

            onFocusedControlChanged: (glSplit, avaloniaGLControl, isInitial) =>
            {
                AvaloniaGLControl = avaloniaGLControl;
            },

            onControlCreated: avaloniaGLControl =>
            {
                var glCtl = avaloniaGLControl.GLControl;

                // track GLControl IsControlInvalidated property to force scene keep in sync at each change

                glCtl.PropertyChanged += (a, b) =>
                {
                    if (b.PropertyName == nameof(GLControl.IsControlInvalidated))
                    {
                        if (glCtl.IsControlInvalidated)
                            glCtl.Invalidate();
                    }
                };
            });

    }
}