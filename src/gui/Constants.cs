namespace SearchAThing.OpenGL.GUI;

public static class Constants
{

    #region colors

    /// <summary>
    /// (Default) gl split focused control border thickness ( screen pixels ).
    /// </summary>
    public const double GRID_SPLITTER_MANAGER_FocusedControlBorderThickness = 2;
    
    /// <summary>
    /// (Default) gl split focused control border brush.
    /// </summary>    
    public static readonly ISolidColorBrush GRID_SPLITTER_MANAGER_FocusedControlBorderBrush =
        new SolidColorBrush(ColorTranslator.FromHtml("#ffff9b").ToAvaloniaColor());

    /// <summary>
    /// (Default) gl split grid splitter color.
    /// </summary>    
    public static readonly Avalonia.Media.Color GRID_SPLITTER_MANAGER_SplitterColor =
        ColorTranslator.FromHtml("#5e5e5e").ToAvaloniaColor();

    /// <summary>
    /// (Default) gl split grid splitter brush.
    /// </summary>    
    public static readonly ISolidColorBrush GRID_SPLITTER_MANAGER_SplitterBrush =
        new SolidColorBrush(GRID_SPLITTER_MANAGER_SplitterColor);

    #endregion

    #region key gesture

    /// <summary>
    /// (Default) Toggle wireframe key gesture.
    /// </summary>    
    public static readonly KeyGesture DEFAULT_ToggleWireframeGesture = new KeyGesture(Key.W);

    /// <summary>
    /// (Default) Toggle shade width dege key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ToggleShadeWithEdgeGesture = new KeyGesture(Key.W, KeyModifiers.Control);

    /// <summary>
    /// (Default) Toggle show normals key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ToggleShowNormalsGesture = new KeyGesture(Key.N);

    /// <summary>
    /// (Default) Toggle perspective key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_TogglePerspectiveGesture = new KeyGesture(Key.P);

    /// <summary>
    /// (Default) Toggle use texture key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ToggleUseTextureGesture = new KeyGesture(Key.X);

    /// <summary>
    /// (Default) Toggle use shadow key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ToggleUseShadowGesture = new KeyGesture(Key.S);

    /// <summary>
    /// (Default) Camera pan left key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraPanLeftGesture = new KeyGesture(Key.Left, KeyModifiers.Shift);

    /// <summary>
    /// (Default) Camera pan right key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraPanRightGesture = new KeyGesture(Key.Right, KeyModifiers.Shift);

    /// <summary>
    /// (Default) Camera pan up key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraPanUpGesture = new KeyGesture(Key.Up, KeyModifiers.Shift);

    /// <summary>
    /// (Default) Camera pan down key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraPanDownGesture = new KeyGesture(Key.Down, KeyModifiers.Shift);

    /// <summary>
    /// (Default) Model rotate left key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ModelRotateLeftGesture = new KeyGesture(Key.Left);

    /// <summary>
    /// (Default) Model rotate right key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ModelRotateRightGesture = new KeyGesture(Key.Right);

    /// <summary>
    /// (Default) Model rotate up key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ModelRotateUpGesture = new KeyGesture(Key.Up);

    /// <summary>
    /// (Default) Model rotate down key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ModelRotateDownGesture = new KeyGesture(Key.Down);

    /// <summary>
    /// (Default) Camera rotate left key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraRotateLeftGesture = new KeyGesture(Key.Left, KeyModifiers.Alt);

    /// <summary>
    /// (Default) Camera rotate right key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraRotateRightGesture = new KeyGesture(Key.Right, KeyModifiers.Alt);

    /// <summary>
    /// (Default) Camera rotate up key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraRotateUpGesture = new KeyGesture(Key.Up, KeyModifiers.Alt);

    /// <summary>
    /// (Default) Camera rotate down key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraRotateDownGesture = new KeyGesture(Key.Down, KeyModifiers.Alt);

    /// <summary>
    /// (Default) Camera zoom in key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraZoomInGesture = new KeyGesture(Key.Up, KeyModifiers.Control);

    /// <summary>
    /// (Default) Camera zoom out key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraZoomOutGesture = new KeyGesture(Key.Down, KeyModifiers.Control);

    /// <summary>
    /// (Default) Camera tilt left key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraTiltLeftGesture = new KeyGesture(Key.Left, KeyModifiers.Control);

    /// <summary>
    /// (Default) Camera tilt right key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_CameraTiltRightGesture = new KeyGesture(Key.Right, KeyModifiers.Control);

    /// <summary>
    /// (Default) View bottom key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ViewBottomGesture = new KeyGesture(Key.O);

    /// <summary>
    /// (Default) View top gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ViewTopGesture = new KeyGesture(Key.T);

    /// <summary>
    /// (Default) View left key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ViewLeftGesture = new KeyGesture(Key.L);

    /// <summary>
    /// (Default) View right key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ViewRightGesture = new KeyGesture(Key.R);

    /// <summary>
    /// (Default) View front key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ViewFrontGesture = new KeyGesture(Key.F);

    /// <summary>
    /// (Default) View back key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ViewBackGesture = new KeyGesture(Key.B);

    /// <summary>
    /// (Default) Zoom fit key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ZoomFitGesture = new KeyGesture(Key.Z);

    /// <summary>
    /// (Default) Invalidate view key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_InvalidateGesture = new KeyGesture(Key.I);

    /// <summary>
    /// (Default) Open dev tool key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_DevTool = new KeyGesture(Key.F1);

    /// <summary>
    /// (Default) Save control view key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_SaveView = new KeyGesture(Key.F2);

    /// <summary>
    /// (Default) Restore control view key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_RestoreView = new KeyGesture(Key.F3);

    /// <summary>
    /// (Default) Save gl split layout view key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_SaveViewLayout = new KeyGesture(Key.F2, KeyModifiers.Shift);

    /// <summary>
    /// (Default) Restore gl split layout view key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_RestoreViewLayout = new KeyGesture(Key.F3, KeyModifiers.Shift);

    /// <summary>
    /// (Default) Split current view horizontally key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_SplitViewHorizontal = new KeyGesture(Key.H);

    /// <summary>
    /// (Default) Split current view vertically key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_SplitViewVertical = new KeyGesture(Key.V);

    /// <summary>
    /// (Default) Split view closed current view key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_SplitViewClose = new KeyGesture(Key.C);

    /// <summary>
    /// (Default) Toggle model bbox key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ToggleModelBBox = new KeyGesture(Key.X, KeyModifiers.Control);

    /// <summary>
    /// (Default) Toggle camera object key gesture.
    /// </summary>
    public static readonly KeyGesture DEFAULT_ToggleCameraObject = new KeyGesture(Key.C, KeyModifiers.Control);

    #endregion

    #region views

    /// <summary>
    /// (Default) Gl control camera view.
    /// </summary>
    public const CameraViewType DEFAULT_View = CameraViewType.Top;

    /// <summary>
    /// (Default) Gl split layout pathfilename: AppDomainBaseDir/layout.json
    /// </summary>    
    public static string LayoutDefaultPathfilename =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "layout.json");

    #endregion

    #region dev tool

    /// <summary>
    /// (Default) Gl dev tool window width (screen pixel).
    /// </summary>
    public const double DEVTOOL_WIDTH = 1200;

    /// <summary>
    /// (Default) Gl dev tool window height (screen pixel).
    /// </summary>
    public const double DEVTOOL_HEIGHT = 400;

    #endregion

    #region notification

    /// <summary>
    /// (Default) Notification title for I/O events.
    /// </summary>
    public const string NOTIFICATION_TITLE_INPUT_OUTPUT = "Input/Output";

    #endregion

}
