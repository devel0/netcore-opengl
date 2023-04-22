using Avalonia.Controls;

namespace SearchAThing.OpenGL.GUI;

public delegate void AvaloniaControlInitialEvent(AvaloniaGLControlSplit split, AvaloniaGLControl ctl, bool isInitial);

public delegate void AvaloniaControlEvent(AvaloniaGLControl ctl);

/// <summary>
/// Gl split control attached to <see cref="GLWindow"/> for stand-alone application or <see cref="Window"/>
/// on mvvm applications.<br/>
/// It creates <see cref="GLView"/> at start then allow to create more when split activated
/// through associated key gestures.<br/>
/// </summary>
public partial class AvaloniaGLControlSplit : GridSplitterManager<GLView>
{

    bool gridSplitterAttached = false;

    public AvaloniaGLControlSplit(GLModel glModel)
    {
        FocusedControlBorderThickness = GRID_SPLITTER_MANAGER_FocusedControlBorderThickness;
        FocusedControlBorderBrush = GRID_SPLITTER_MANAGER_FocusedControlBorderBrush;
        SplitterBrush = GRID_SPLITTER_MANAGER_SplitterBrush;

        this.LayoutUpdated += (a, b) =>
        {
            if (!gridSplitterAttached)
            {
                CreateControl = () =>
                {
                    GLView? glView = null;

                    GLView? existingGLControl = null;

                    if (FocusedControl is GLView focusedControl)
                        existingGLControl = focusedControl;

                    GLControl glControl;

                    glView = new GLView();

                    if (existingGLControl is not null)
                        glControl = new GLControl(existingGLControl.AvaloniaGLControl.GLControl, glView.AvaloniaGLControl);

                    else
                        glControl = new GLControl(glModel, glView.AvaloniaGLControl);

                    glControl.NotEmptyRenderPreview += (glControl) =>
                    {
                        if (glControl.LastCameraView is null)
                            glControl.CameraView(DEFAULT_View);

                        else if (glControl.LastCameraView != CameraViewType.Manual)
                            glControl.CameraView(glControl.LastCameraView.Value);
                    };

                    glControl.InvalidateAllRequest += (sender, e) =>
                    {
                        Invalidate();
                    };

                    glView.AvaloniaGLControl.GLControl = glControl;

                    return glView;
                };

                gridSplitterAttached = true;
            }

        };
    }

    /// <summary>
    /// Load all <see cref="GLControl"/> view config from the given pathfilename.
    /// </summary>
    /// <param name="pathfilename">(Optional) pathfilename which store view config.<br/>
    /// If null <see cref="LayoutDefaultPathfilename"/> will be used.</param>
    public void LoadViewLayout(string? pathfilename = null) =>
        FocusedControl?.AvaloniaGLControl.LoadViewLayout(pathfilename);

    /// <summary>
    /// Save all <see cref="GLControl"/> view config to the given pathfilename.<br/>    
    /// </summary>
    /// <param name="pathfilename">(Optional) pathfilename which store view config.<br/>
    /// If null <see cref="LayoutDefaultPathfilename"/> will be used.</param>
    public void SaveViewLayout(string? pathfilename = null) =>
        FocusedControl?.AvaloniaGLControl.SaveViewLayout(pathfilename);

    /// <summary>
    /// Invalidates all gl controls children of this gl split.
    /// </summary>
    /// <param name="invalidate">Type of invalidation (Default: RedrawOnly).</param>
    public void Invalidate(InvalidateEnum invalidate = InvalidateEnum.RedrawOnly) =>
        Children.ForEach(ctl => ctl.AvaloniaGLControl.GLControl.Invalidate(invalidate));

}

public static partial class Toolkit
{

    /// <summary>
    /// Attach gl split to the given avalonia window.<br/>
    /// It's automatically invoked in the stand-alone console application through
    /// <see cref="GLWindow"/> and have to be attached during MainWindow constructor 
    /// for mvvm application types.
    /// </summary>
    /// <param name="owner">Gl split window owner.</param>
    /// <param name="grid">Grid control container for gl split control.</param>
    /// <param name="glModel">Gl model.</param>
    /// <param name="setGLControlSplit">Callback after gl split attaches (args: <see cref="SearchAThing.OpenGL.GUI.AvaloniaGLControlSplit">AvaloniaGLControlSplit</see>).</param>
    /// <param name="onFocusedControlChanged">Callback after avalonia gl control focused (args: <see cref="SearchAThing.OpenGL.GUI.AvaloniaGLControlSplit">AvaloniaGLControlSplit</see>, <see cref="AvaloniaGLControl"/>, bool isInitial) </param>
    /// <param name="onControlCreated">Callback after avalonia gl control created (args: <see cref="AvaloniaGLControl"/>).</param>
    /// <param name="onControlRemoved">Callback after avalonia gl control removed (args: <see cref="AvaloniaGLControl"/>).</param>
    public static void AttachGLControlSplit(this Window owner,
        Grid grid, GLModel glModel,
        Action<AvaloniaGLControlSplit>? setGLControlSplit = null,
        AvaloniaControlInitialEvent? onFocusedControlChanged = null,
        AvaloniaControlEvent? onControlCreated = null,
        AvaloniaControlEvent? onControlRemoved = null)
    {
        bool firstActivate = true;

        if (owner is not GLWindow)
            owner.AttachGLBindings(grid, glModel);

        owner.Opened += (a, b) =>
        {
            if (firstActivate)
            {
                firstActivate = false;

                var splitCtl = new AvaloniaGLControlSplit(glModel);

                grid.Children.Add(splitCtl);

                // set GLControlSplit only after FocusedControl not null to avoid broken binding 
                setGLControlSplit?.Invoke(splitCtl);

                if (onControlCreated is not null)
                    splitCtl.ControlCreated += (ctl) =>
                    {
                        onControlCreated?.Invoke(ctl.AvaloniaGLControl);
                    };

                if (onControlRemoved is not null)
                    splitCtl.ControlRemoved += (ctl) =>
                    {
                        onControlRemoved?.Invoke(ctl.AvaloniaGLControl);
                    };

                bool isInitial = true;

                splitCtl.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(GridSplitterManager<AvaloniaGLControl>.FocusedControl) &&
                        sender is AvaloniaGLControlSplit glControlSplit)
                    {
                        if (glControlSplit.FocusedControl is not null)
                        {
                            var _isInitial = isInitial;
                            isInitial = false;

                            onFocusedControlChanged?.Invoke(
                                splitCtl,
                                glControlSplit.FocusedControl.AvaloniaGLControl,
                                _isInitial);
                        }
                    }
                };
            }

        };
    }

}
