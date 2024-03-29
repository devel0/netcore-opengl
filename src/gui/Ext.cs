using Avalonia.Controls.Notifications;

namespace SearchAThing.OpenGL.GUI;

public static partial class Ext
{

    /// <summary>
    /// Convert avalonia Size object to drawing Size.
    /// </summary>
    public static Size ToRenderSize(this in Avalonia.Size size) =>
        new Size((int)size.Width, (int)size.Height);

    /// <summary>
    /// Convert given drawing color to avalonia color.
    /// </summary>
    public static AColor ToAvaloniaColor(this in Color color) =>
        AColor.FromArgb(color.A, color.R, color.G, color.B);

    /// <summary>
    /// Convert given avalonia point to vector2.
    /// </summary>
    public static Vector2 ToVector2(this in Avalonia.Point pt) =>
        new Vector2((float)pt.X, (float)pt.Y);

    /// <summary>
    /// Convert given gl notification type to corresponding avalonia notification type.
    /// </summary>    
    public static NotificationType ToAvaloniaNotificationType(this GLNotificationType type) => type switch
    {
        GLNotificationType.Information => NotificationType.Information,
        GLNotificationType.Success => NotificationType.Success,
        GLNotificationType.Warning => NotificationType.Warning,
        GLNotificationType.Error => NotificationType.Error,
        _ => throw new ArgumentException($"invalid {type} nofication type")
    };

}