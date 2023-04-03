using Avalonia.Controls;
using Avalonia.Data;

namespace SearchAThing.OpenGL.GUI;

/// <summary>
/// Avalonia control that hold a `grRoot` grid inside of which take place a title and an <see cref="AvaloniaGLControl"/>.<br/>
/// Its purpose is to separate the avalonia gl control from outer gui control, in this case the gl view for
/// desktop widget customization ( actually the title is overlayed to ).
/// </summary>
public partial class GLView : UserControl, INotifyPropertyChanged
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

    #region AvaloniaGLControl

    private AvaloniaGLControl _AvaloniaGLControl;
    /// <summary>
    /// AvaloniaGLControl
    /// </summary>
    public AvaloniaGLControl AvaloniaGLControl
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

    public GLView()
    {
        InitializeComponent();

        this.AvaloniaGLControl = new SearchAThing.OpenGL.GUI.AvaloniaGLControl();

        var a = TitleTblk.Bind(
            TextBlock.TextProperty,
            new Binding(nameof(AvaloniaGLControl.Title)) { Source = AvaloniaGLControl });

        grRoot.Children.Add(AvaloniaGLControl);
    }

    protected override void OnKeyDown(KeyEventArgs e) => AvaloniaGLControl.HandleKeyDown(e);

}
