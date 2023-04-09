using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace example.Views;

// example-0019
// sphere vertex render and hittest scalability test
//
// - enable hittest
// - move mouse on the figure to see hitted triangles
// - increase divisions
//
// NOTE : hit test doesn't apply any optimization on hit test ( worst case ) : all figures evaluated

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

    #region Radius

    private float _Radius = 10;
    /// <summary>
    /// Radius
    /// </summary>
    public float Radius
    {
        get => _Radius;
        set
        {
            var changed = value != _Radius;
            if (changed)
            {
                _Radius = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region SurfaceDivisions

    private int _SurfaceDivisions = 3;
    /// <summary>
    /// SurfaceDivisions
    /// </summary>
    public int SurfaceDivisions
    {
        get => _SurfaceDivisions;
        set
        {
            var changed = value != _SurfaceDivisions;
            if (changed)
            {
                _SurfaceDivisions = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region RamUsed

    private string _RamUsed = "";
    /// <summary>
    /// RamUsed
    /// </summary>
    public string RamUsed
    {
        get => _RamUsed;
        set
        {
            var changed = value != _RamUsed;
            if (changed)
            {
                _RamUsed = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region TypeSphere

    private bool _TypeSphere = true;
    /// <summary>
    /// TypeSphere
    /// </summary>
    public bool TypeSphere
    {
        get => _TypeSphere;
        set
        {
            var changed = value != _TypeSphere;
            if (changed)
            {
                _TypeSphere = value;
                if (value) TypePlate = false;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region TypePlate

    private bool _TypePlate = false;
    /// <summary>
    /// TypePlate
    /// </summary>
    public bool TypePlate
    {
        get => _TypePlate;
        set
        {
            var changed = value != _TypePlate;
            if (changed)
            {
                _TypePlate = value;
                if (value) TypeSphere = false;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region HitTest

    private bool _HitTest = false;
    /// <summary>
    /// HitTest
    /// </summary>
    public bool HitTest
    {
        get => _HitTest;
        set
        {
            var changed = value != _HitTest;
            if (changed)
            {
                _HitTest = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    GLTriangleFigure? triFig = null;

    public MainWindow()
    {
        InitializeComponent();

        var q = this.Resources;

        Title = AppDomain.CurrentDomain.FriendlyName;

        var glCtx = new GLContext();
        glModel = new GLModel(glCtx);

        Task.Run(async () =>
        {
            while (true)
            {
                RamUsed = $"{GC.GetTotalAllocatedBytes().HumanReadable()}";
                await Task.Delay(1000);
            }
        });

        glModel.BuildModel = (glCtl, isInitial) =>
        {
            void syncedBuild()
            {
                glModel.Clear(clearPointLights: true);

                glModel.PointLights.Add(new GLPointLight(
                    new Vector3(0 * Radius, 0 * Radius, 2f * Radius), showPoint: true));

                glModel.PointLights.Add(new GLPointLight(
                    new Vector3(-2f * Radius, 0 * Radius, 0 * Radius), Color.Red, showPoint: true));

                // -----------------------

                Debug.WriteLine($"Generating [{(TypeSphere ? "sphere" : "plate")}] with Radius:{Radius} Divisions:{SurfaceDivisions}");

                if (TypeSphere)
                {
                    var q = new UVSphere(Vector3.Zero, Radius).Triangles(SurfaceDivisions);

                    triFig = new UVSphere(Vector3.Zero, Radius).Figure(SurfaceDivisions);
                }

                else
                {
                    var tris = new List<GLTriangle>();

                    var step = Radius / SurfaceDivisions;
                    for (int sx = 0; sx < SurfaceDivisions; ++sx)
                    {
                        for (int sy = 0; sy < SurfaceDivisions; ++sy)
                        {
                            var p1 = new Vector3(sx * step, sy * step, 0);
                            var p2 = p1 + new Vector3(step, 0, 0);
                            var p3 = p2 + new Vector3(0, step, 0);
                            tris.Add(new GLTriangle(p1, p2, p3));

                            var p4 = p1 + new Vector3(0, step, 0);
                            tris.Add(new GLTriangle(p1, p3, p4));
                        }
                    }

                    triFig = new GLTriangleFigure(tris);
                }

                glModel.AddFigure(triFig);

                glModel.SetupLightAttenuation();
            }

            if (isInitial)
                syncedBuild();

            else
            {
                // running separate thread
                Task.Run(async () =>
                {
                    // work on model
                    syncedBuild();

                    // await the invalidate ( render ) to work on model
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        GLControlSplit?.Invalidate();
                    });
                });
            }
        };

        var hitTris = new HashSet<GLTriangle>();

        this.PointerMoved += (a, b) =>
        {
            if (!HitTest) return;

            var glView = GLControlSplit?.FocusedControl;
            if (glView is null) return;

            var glCtl = glView.AvaloniaGLControl.GLControl;

            var cp = b.GetCurrentPoint(glView);

            var lraycast = glCtl.RayCastLocal(screen: cp.Position.ToVector2());

            var newHitTris = new HashSet<GLTriangle>();
            var existingHitTris = 0;

            var figures = glModel.Figures.ToList();

            foreach (var fig in figures)
            {
                Line oraycast;

                if (fig.ObjectMatrixIsIdentity) // no transform required if fig object matrix already an identity
                    oraycast = lraycast;

                else
                    oraycast = lraycast.Transform(fig.ObjectMatrix.Inverse());

                foreach (var tri in fig.Primitives.OfType<GLTriangle>())
                {
                    var q = oraycast.Intersect(tri.Plane);
                    if (q is not null && tri.Contains(q.Value))
                    {
                        if (hitTris.Contains(tri)) ++existingHitTris;
                        newHitTris.Add(tri);
                    }

                }
            }

            if (newHitTris.Count != hitTris.Count || existingHitTris != hitTris.Count)
            {
                foreach (var tri in hitTris)
                {
                    tri.SetColor(DEFAULT_MaterialColor);
                    tri.Order = DEFAULT_FigureOrder;
                }

                foreach (var tri in newHitTris)
                {
                    tri.SetColor(Color.Yellow);
                    tri.Order = 1;
                }

                this.GLControlSplit?.Invalidate();
            }

            hitTris = newHitTris;

        };

        this.AttachGLControlSplit(grGL, glModel,
            setGLControlSplit: glSplit => GLControlSplit = glSplit,

            onFocusedControlChanged: (glSplit, avaloniaGlControl, isInitial) =>
            {
                FocusedControl = avaloniaGlControl?.GLControl;

                if (isInitial)
                {
                    isInitial = false;
                    GLControlSplit?.LoadViewLayout();
                }
            },

            onControlCreated: avaloniaGLControl =>
            {
                var glCtl = avaloniaGLControl.GLControl;

                glCtl.PropertyChanged += AvaloniaGLControl_PropertyChanged;
            },

            onControlRemoved: avaloniaGLControl =>
            {
                var glCtl = avaloniaGLControl.GLControl;

                glCtl.PropertyChanged -= AvaloniaGLControl_PropertyChanged;
            });

        //! [DebounceExample]
        var debouncedInvalidate = new DebounceAction<object?>(TimeSpan.FromMilliseconds(500), (args) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                // invalidating the model imply recall of the BuildModel
                // at the first time the control will be invalidated too
                glModel.Invalidate();

                GLControlSplit?.Invalidate();
            });
        });

        this.PropertyChanged += (sender, e) =>
        {
            if (
                e.PropertyName == nameof(Radius) || e.PropertyName == nameof(SurfaceDivisions) ||
                e.PropertyName == nameof(TypeSphere) || e.PropertyName == nameof(TypePlate))
            {
                debouncedInvalidate.Hit(null);
            }
        };
        //! [DebounceExample]

    }

    private void AvaloniaGLControl_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is GLControl glCtl)
        {
            if (e.PropertyName == nameof(GLControl.IsControlInvalidated))
            {
                if (glCtl.IsControlInvalidated)
                    glCtl.Invalidate();
            }
        }
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
            var dxf = new netDxf.DxfDocument();

            foreach (var fig in glModel.Figures)
            {
                if (fig.PrimitiveType == GLPrimitiveType.Triangle)
                {
                    foreach (var tri in fig.Primitives.OfType<GLTriangle>())
                    {
                        dxf.Entities.Add(new netDxf.Entities.Face3D(
                            new netDxf.Vector3(tri.V1.Position.X, tri.V1.Position.Y, tri.V1.Position.Z),
                            new netDxf.Vector3(tri.V2.Position.X, tri.V2.Position.Y, tri.V2.Position.Z),
                            new netDxf.Vector3(tri.V3.Position.X, tri.V3.Position.Y, tri.V3.Position.Z)));
                    }
                }
            }

            dxf.Save(file.Path.AbsolutePath, isBinary: true);
        }
    }
}
