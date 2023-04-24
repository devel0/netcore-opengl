using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Avalonia gl control.<br/>
/// It's a graphical ui <see cref="IRenderDevice"/> used to transfer <see cref="GLControl"/> pixels
/// to the window bitmap through the override of the avalonia <see cref="Render"/> method
/// that will be called by the avalonia framework every time the visual invalidates.<br/>
/// When control size changed <see cref="GLControl.Invalidate"/> will be invoked and 
/// <see cref="GLSizeChanged"/> event emitted.
/// </summary>
public partial class AvaloniaGLControl : Control, INotifyPropertyChanged, IRenderDevice
{

    #region forwarders

    GLModel glModel => GLControl.GLModel;

    public GLContext GLContext => glModel.GLContext;

    internal GL GL => GLContext.GL;

    #endregion

    #region property changed
    public new event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// invoke this method to forward propertchanged event notification.
    /// note: not needed to specify propertyName set by compiler service to called property.
    /// </summary>        
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    internal event EventHandler GLControlConnected;

    #region glControl

    private GLControl? _glControl = null;
    /// <summary>
    /// Gl control allow operations on the gl model.
    /// </summary>
    public GLControl GLControl
    {
        get
        {
            if (_glControl is null) throw new Exception($"glcontrol not initialized");
            return _glControl;
        }
        set
        {
            if (_glControl is not null) throw new Exception($"glcontrol already attached");

            _glControl = value;

            Size = Bounds.Size.ToRenderSize();
            _glControl.RenderInvalidated += (sender, e) =>
            {
                if (sender is GLControl glControl && !glControl.IsRendering)
                    InvalidateVisual();
            };

            // used by GLView to listen for GLControl prop changes ( Title, Overlay )
            GLControlConnected?.Invoke(this, EventArgs.Empty);

            _glControl.PropertyChanged += GLControl_PropertyChanged;
            _glControl.GLModel.PropertyChanged += GLModel_PropertyChanged;

            this.LayoutUpdated += AvaloniaGLControl_LayoutUpdated;
            SetDefaultKeyGestures();
            SetCursor();

            OnPropertyChanged();
        }
    }

    #endregion

    static Assembly? _netcore_opengl_gui_assembly = null;

    internal static Assembly netcore_opengl_gui_assembly
    {
        get
        {
            if (_netcore_opengl_gui_assembly is null)
            {
                var q = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == NETCORE_OPENGL_GUI_ASSEMBLY_NAME);

                if (q is null)
                {
                    throw new Exception($"unable to find [netcore-opengl-gui] assembly");
                }

                _netcore_opengl_gui_assembly = q;
            }

            return _netcore_opengl_gui_assembly;
        }
    }

    void SetCursor()
    {
        var selectionMode = glModel.SelectionMode;
        var identifyMode = GLControl.IdentifyCoord;

        if (selectionMode != Core.SelectionMode.None)
        {
            switch (selectionMode)
            {
                case Core.SelectionMode.Primitive:
                    {
                        var iconStream = netcore_opengl_gui_assembly
                            .GetManifestResourceStream(GuiAssetResourceName(RESOURCE_FILENAME_SelectPrimitiveCursor_32));

                        var bitmap = new Bitmap(iconStream);

                        Cursor = new Cursor(bitmap, new PixelPoint(15, 0));
                    }
                    break;

                case Core.SelectionMode.Figure:
                    {
                        Cursor = new Cursor(StandardCursorType.Hand);
                    }
                    break;
            }
        }

        else if (identifyMode)
            Cursor = new Cursor(StandardCursorType.Cross);

        else
            Cursor = Cursor.Default;
    }

    private void GLModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GLModel.SelectionMode))
        {
            SetCursor();
        }
    }

    private void GLControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GLControl.IdentifyCoord))
        {
            var identifyCoord = GLControl.IdentifyCoord;

            if (identifyCoord)
                StartIdentifyCoord();
            else
                StopIdentifyCoord();

            SetCursor();
        }
    }

    #region Size

    private Size _Size;

    public Size Size
    {
        get => _Size;
        private set
        {
            var changed = value != _Size;
            if (changed)
            {
                _Size = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region TransferGLPixelsCount

    private int _TransferGLPixelsCount = 0;

    public int TransferGLPixelsCount
    {
        get => _TransferGLPixelsCount;
        internal set
        {
            var changed = value != _TransferGLPixelsCount;
            if (changed)
            {
                _TransferGLPixelsCount = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    /// <summary>
    /// Create an avalonia render device gl control.
    /// </summary>
    public AvaloniaGLControl()
    {
        Size = new Size();
    }

    Rect? previousBounds = null;

    private void AvaloniaGLControl_LayoutUpdated(object? sender, EventArgs e)
    {
        if (previousBounds is null || !Bounds.Equals(previousBounds))
        {
            Size = Bounds.Size.ToRenderSize();
            GLControl.Invalidate(); // size changed
            previousBounds = Bounds;
            GLSizeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Event emitted when size change detected.
    /// </summary>
    public event EventHandler? GLSizeChanged;

    WriteableBitmap? bitmap = null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Post(Action action) => Dispatcher.UIThread.Post(action);

    bool transferringGLPixels = false;

    public void TransferGLPixels(GL GL)
    {
        if (transferringGLPixels || Size.IsEmpty) return;

        transferringGLPixels = true;

        if (bitmap is not null) bitmap.Dispose();

        GLControl.Render(this);

        bitmap = new WriteableBitmap(
            new PixelSize(Size.Width, Size.Height),
            new global::Avalonia.Vector(96, 96),
            Avalonia.Platform.PixelFormat.Rgba8888,
            Avalonia.Platform.AlphaFormat.Opaque);

        using (var l = bitmap.Lock())
        {
            GL.PixelStore(
                PixelStoreParameter.PackRowLength,
                l.RowBytes / 4);

            unsafe
            {
                GL.ReadPixels(0, 0, (uint)Size.Width, (uint)Size.Height,
                    PixelFormat.Rgba, PixelType.UnsignedByte, l.Address.ToPointer());
            }
        }

        ++TransferGLPixelsCount;

        // Debug.WriteLine($"TransferGLPixelsCount = {TransferGLPixelsCount}");

        transferringGLPixels = false;
    }

    public override void Render(DrawingContext context)
    {
        if (_glControl is null) return;

        if (GLControl.IsRenderInvalidated || bitmap is not null)
            TransferGLPixels(GL);

        if (bitmap is not null)
        {
            //bitmap.Save("/home/devel0/Desktop/test.png");

            context.DrawImage(bitmap,
                new Rect(bitmap.Size),
                new Rect(bitmap.Size),
                BitmapInterpolationMode.LowQuality);
        }
    }

    GLDevTool? devTool = null;

    /// <summary>
    /// Activates the <see cref="GLDevTool"/> developer tool.
    /// </summary>
    public void ShowDevTool()
    {
        if (devTool is not null)
        {
            devTool.Activate();
        }

        else
        {
            devTool = new GLDevTool(GridSplitterManager)
            {
                Width = DEVTOOL_WIDTH,
                Height = DEVTOOL_HEIGHT,
            };
            devTool.Closed += (a, b) => devTool = null;
            devTool.Show();
        }
    }

    AvaloniaGLControlSplit? _GridSplitterManager;

    /// <summary>
    /// Retrieve attached gl split manager.
    /// </summary>    
    public AvaloniaGLControlSplit GridSplitterManager
    {
        get
        {
            if (_GridSplitterManager is null)
            {
                _GridSplitterManager = this.SearchParent<AvaloniaGLControlSplit>();
                if (_GridSplitterManager is null) throw new Exception($"can't find GridSplitterManager in control's parents");
            }
            return _GridSplitterManager;
        }
    }

    /// <summary>
    /// Split current view horizontally.
    /// </summary>
    public void SplitHorizontal() => Split(GridSplitDirection.Horizontally);

    /// <summary>
    /// Split current view vertically.
    /// </summary>
    public void SplitVertical() => Split(GridSplitDirection.Vertically);

    /// <summary>
    /// Split current view in given direction.
    /// </summary>
    /// <param name="direction">Split direction.</param>
    public void Split(GridSplitDirection direction)
    {
        var newCtl = GridSplitterManager.Split(direction);
        newCtl?.AvaloniaGLControl.SetViewNfo(GetViewNfo());
    }

    /// <summary>
    /// Close current view ( if not is the last one ).
    /// </summary>
    public void SplitClose() => GridSplitterManager.Remove();

    /// <summary>
    /// Retrieve an object that can be serialized to save current <see cref="GLControl"/> view config.
    /// </summary>    
    /// <param name="includeLights">If true (default) lights will saved within view nfo.</param>    
    public ViewNfo GetViewNfo(bool includeLights = true) => GLControl.GetViewNfo(includeLights);

    /// <summary>
    /// Restore <see cref="GLControl"/> view settings from given nfo object.    
    /// </summary>    
    /// <param name="includeLights">If true (default) lights set to the model.</param>    
    public void SetViewNfo(ViewNfo nfo, bool includeLights = true) => GLControl.SetViewNfo(nfo, includeLights);

    /// <summary>
    /// Save all <see cref="GLControl"/> view config to the given pathfilename.<br/>    
    /// </summary>
    /// <param name="pathfilename">(Optional) pathfilename which store view config.<br/>
    /// If null <see cref="LayoutDefaultPathfilename"/> will be used.</param>
    public void SaveViewLayout(string? pathfilename = null)
    {
        var uidToViewConfig = new Dictionary<int, ViewNfo>();

        var focusedCtl = GridSplitterManager.FocusedControl;
        var focusedUID = 0;

        // GridSplitterManager.PrintStructure(Console.Out);        

        var layout = GridSplitterManager.SaveStructure(emitControl: (ctl, uid) =>
        {
            uidToViewConfig.Add(uid, ctl.AvaloniaGLControl.GetViewNfo(includeLights: false));

            if (ctl == focusedCtl) focusedUID = uid;
        });

        ViewLayoutNfo viewLayoutNfo;

        if (layout is not null)
            viewLayoutNfo = new ViewLayoutNfo
            {
                Layout = layout,
                UIDView = uidToViewConfig,
                Lights = glModel.PointLights.ToList()
            };

        else
        {
            var uidView = new Dictionary<int, ViewNfo>();
            uidView.Add(0, GLControl.GetViewNfo());

            viewLayoutNfo = new ViewLayoutNfo
            {
                UIDView = uidView,
                Lights = glModel.PointLights.ToList()
            };
        }

        viewLayoutNfo.FocusedUIDView = focusedUID;

        viewLayoutNfo.OverrideAmbient = glModel.OverrideAmbientEnabled ? glModel.OverrideAmbient : null;
        viewLayoutNfo.OverrideDiffuse = glModel.OverrideDiffuseEnabled ? glModel.OverrideDiffuse : null;
        viewLayoutNfo.OverrideSpecular = glModel.OverrideSpecularEnabled ? glModel.OverrideSpecular : null;

        if (pathfilename is null) pathfilename = LayoutDefaultPathfilename;
        var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
        File.WriteAllText(pathfilename, JsonConvert.SerializeObject(viewLayoutNfo, Formatting.Indented, settings));

        glModel.SendNotification(NOTIFICATION_TITLE_INPUT_OUTPUT, $"Saved view to\n{pathfilename}");
    }

    /// <summary>
    /// Load all <see cref="GLControl"/> view config from the given pathfilename.
    /// </summary>
    /// <param name="pathfilename">(Optional) pathfilename which store view config.<br/>
    /// If null <see cref="LayoutDefaultPathfilename"/> will be used.</param>
    public void LoadViewLayout(string? pathfilename = null)
    {
        if (pathfilename is null) pathfilename = LayoutDefaultPathfilename;

        if (!File.Exists(pathfilename))
        {
            glModel.SendNotification(
                NOTIFICATION_TITLE_INPUT_OUTPUT,
                $"View layout file\n{pathfilename}\nnot found",
                GLNotificationType.Warning);

            return;
        }

        var nfo = JsonConvert.DeserializeObject<ViewLayoutNfo>(File.ReadAllText(pathfilename));
        if (nfo is null) return;

        if (nfo.Layout is null)
            GLControl.SetViewNfo(nfo.UIDView.First().Value);

        else
        {
            GLView? viewToFocus = null;

            GridSplitterManager.LoadStructure(nfo.Layout, reconfigureControl: (glview, uid) =>
            {
                glview.AvaloniaGLControl.SetViewNfo(nfo.UIDView[uid], includeLights: false);
                if (nfo.FocusedUIDView == uid)
                    viewToFocus = glview;
            });

            GridSplitterManager.FocusedControl = viewToFocus;
        }

        if (nfo.Lights is not null)
        {
            glModel.PointLights.Clear();
            foreach (var light in nfo.Lights)
            {
                glModel.PointLights.Add(light);
            }
        }

        glModel.OverrideLightStrengths(nfo.OverrideAmbient, nfo.OverrideDiffuse, nfo.OverrideSpecular);

        GLControl.Invalidate();
    }

    /// <summary>
    /// Retrieve gl window associated to this avalonia gl control.
    /// </summary>    
    public GLWindow? GLWindow => this.SearchParent<GLWindow>();

}

