using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;

namespace SearchAThing.OpenGL.GUI;

public delegate void GLControlCreatedDelegate(AvaloniaGLControl avaloniaGLControl);

public delegate void GLSplitAttachedDelegate(AvaloniaGLControlSplit glSplit);

public delegate void GLControlFocusedDelegate(AvaloniaGLControlSplit glSplit, AvaloniaGLControl avaloniaGLControl, bool isInitial);

/// <summary>
/// Used in stand-alone application types ( console programs ) to create an application gl window.<br/>
/// This control automatically keeps track of created gl control and listen for their property change
/// in order to invalidate their visual.<br/>
/// 
/// This window contains the <see cref="AvaloniaGLControlSplit"/> that is responsible to create 
/// <see cref="GLView"/> and set to them the <see cref="AvaloniaGLControl"/> which is the target
/// render device for the <see cref="GLControl"/>.
/// \snippet{'trimleft'} examples/example-0000/Program.cs example-0000
/// </summary>
public partial class GLWindow : Window, INotifyPropertyChanged
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

    /// <summary>
    /// Reference to gl model within its gl context.<br/>
    /// It will automatically created within the <see cref="GLWindow"/> constructor.
    /// </summary>    
    public GLModel GLModel { get; private set; }

    #region GLControlSplit

    private AvaloniaGLControlSplit? _GLControlSplit = null;
    /// <summary>
    /// Reference to the application gl split control.<br/>
    /// It will automatically created and attached within the <see cref="GLWindow"/> constructor.
    /// </summary>
    public AvaloniaGLControlSplit? GLControlSplit
    {
        get => _GLControlSplit;
        private set
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
    /// Focused gl control.<br/>
    /// Changes if the gl split <see cref="AvaloniaGLControlSplit"/> has more than the single initial views inside it.
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

    public GLWindow()
    {
        ;
    }

    /// <summary>
    /// Create a gl window and contextually:
    /// - a <see cref="GLModel"/>
    /// - a <see cref="GLContext"/>     
    /// - a split gl <see cref="AvaloniaGLControlSplit"/>
    /// </summary>
    /// <param name="onGLControlCreated">Callback with <see cref="AvaloniaGLControl"/> argument invoked when a <see cref="GLControl"/> will be created.</param>
    /// <param name="onGLSplitAttached">Callback with <see cref="AvaloniaGLControlSplit"/> argument invoked when a <see cref="AvaloniaGLControlSplit"/> is attached to this gl window.</param>
    /// <param name="onFocusedControlChanged">Callback after avalonia gl control focused (args: <see cref="SearchAThing.OpenGL.GUI.AvaloniaGLControlSplit">AvaloniaGLControlSplit</see>, <see cref="AvaloniaGLControl"/>, bool isInitial) </param>
    public GLWindow(
        GLControlCreatedDelegate? onGLControlCreated = null,
        GLSplitAttachedDelegate? onGLSplitAttached = null,
        GLControlFocusedDelegate? onFocusedControlChanged = null)
    {
        InitializeComponent();

        GLModel = new GLModel(new GLContext());

        GLModel.NotificationRequest += (title, msg, type) =>
            this.NotificationManager?.Show(new Notification(title, msg, type.ToAvaloniaNotificationType()));        

        this.AttachGLControlSplit(RootGrid, GLModel,
            setGLControlSplit: x =>
            {
                if (GLControlSplit is null)
                {
                    GLControlSplit = x;
                    onGLSplitAttached?.Invoke(x);
                }
            },

            onFocusedControlChanged: (glsplit, avaloniaGLControl, isInitial) =>
            {
                FocusedControl = avaloniaGLControl?.GLControl;
                if (onFocusedControlChanged is not null)
                    onFocusedControlChanged.Invoke(GLControlSplit!, avaloniaGLControl!, isInitial);
            },

            onControlCreated: avaloniaGLControl =>
            {
                onGLControlCreated?.Invoke(avaloniaGLControl);

                if (avaloniaGLControl is not null)
                {
                    avaloniaGLControl.GLControl.PropertyChanged += GLControl_PropertyChanged;
                }
            });
    }

    private void GLControl_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GLControl.IsRenderInvalidated) &&
            sender is GLControl glCtl &&
            glCtl.IsRenderInvalidated &&
            GLControlSplit is not null)
        {
            GLControlSplit.Invalidate();
        }
    }

    /// <summary>
    /// Reference to avalonia notification manager, useful to show popup messages.
    /// </summary>    
    public WindowNotificationManager? NotificationManager { get; set; }

    /// <summary>
    /// Create a gl window.
    /// </summary>
    /// <param name="width">(Optional) window width (pixels).</param>
    /// <param name="height">(Optional) window height (pixels).</param>
    /// <param name="onGLControlCreated">(Optional) gl control created handler. Args (<see cref="AvaloniaGLControl"/>).</param>
    /// <param name="onGLSplitAttached">(Optional) gl split attached. Args (<see cref="AvaloniaGLControlSplit"/>)</param>
    /// <param name="onFocusedControlChanged">Callback after avalonia gl control focused (args: <see cref="SearchAThing.OpenGL.GUI.AvaloniaGLControlSplit">AvaloniaGLControlSplit</see>, <see cref="AvaloniaGLControl"/>, bool isInitial) </param>
    /// <returns></returns>
    public static GLWindow Create(
        double? width = null, double? height = null,
        GLControlCreatedDelegate? onGLControlCreated = null,
        GLSplitAttachedDelegate? onGLSplitAttached = null,
        GLControlFocusedDelegate? onFocusedControlChanged = null)
    {
        string? title = null;
        GLWindow? w = null;

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            w = new GLWindow(onGLControlCreated, onGLSplitAttached, onFocusedControlChanged);

            if (title is not null)
                w.Title = title;

            else
            {
                w.Title = AppDomain.CurrentDomain.FriendlyName;
            }
            if (width is not null) w.Width = width.Value;
            if (height is not null) w.Height = height.Value;

            w.Opened += (a, b) =>
            {
                w.NotificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(w.RootGrid));
            };
        }).Wait();

        return w!;
    }

    /// <summary>
    /// Start avalonia engine from console mode to show up gl window.
    /// </summary>
    public void ShowSync()
    {
        var areFinished = new AutoResetEvent(false);

        // set the app to exit when window closed
        Closed += (a, b) => { areFinished.Set(); };

        // show the window (async)
        Dispatcher.UIThread.Post(() => Show());

        Console.CancelKeyPress += (a, b) => { areFinished.Set(); b.Cancel = true; };

        areFinished.WaitOne();
    }

    /// <summary>
    /// Invalidate gl control split.
    /// </summary>
    public void Invalidate() => GLControlSplit?.Invalidate();

}

public static partial class Ext
{

    /// <summary>
    /// Attach bindings such as notification manager.
    /// </summary>    
    internal static void AttachGLBindings(this Window window, Grid grGL, GLModel glModel)
    {
        WindowNotificationManager? notificationManager = null;

        window.Opened += (sender, e) =>
        {
            notificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(grGL));
        };

        glModel.NotificationRequest += (title, msg, type) =>
            notificationManager?.Show(new Notification(title, msg, type.ToAvaloniaNotificationType()));
        
        window.Closed += (sender, e) =>
        {

        };
    }

}