namespace SearchAThing.OpenGL.Render;

public enum CameraViewType
{
    /// <summary>
    /// allow to specify x, y, z rotation angles
    /// </summary>
    Manual,

    /// <summary>
    /// view l
    /// </summary>
    Left,

    /// <summary>
    /// view r
    /// </summary>
    Right,

    /// <summary>
    /// view f
    /// </summary>
    Front,

    /// <summary>
    /// view b
    /// </summary>
    Back,

    /// <summary>
    /// view t
    /// </summary>
    Top,

    /// <summary>
    /// view o
    /// </summary>
    Bottom,

    //------------------------------------------ cube left side

    /// <summary>
    /// view ll
    /// </summary>
    LeftLeft,

    /// <summary>
    /// view lr
    /// </summary>
    LeftRight,

    /// <summary>
    /// view lo
    /// </summary>
    LeftBottom,

    /// <summary>
    /// view lol
    /// </summary>
    LeftBottomLeft,

    /// <summary>
    /// view lor
    /// </summary>
    LeftBottomRight,

    /// <summary>
    /// view lt
    /// </summary>
    LeftTop,

    /// <summary>
    /// view ltl
    /// </summary>
    LeftTopLeft,

    /// <summary>
    /// view ltr
    /// </summary>
    LeftTopRight,

    //------------------------------------------ cube right side

    /// <summary>
    /// view rl
    /// </summary>
    RightLeft,

    /// <summary>
    /// view rr
    /// </summary>
    RightRight,

    /// <summary>
    /// view ro
    /// </summary>
    RightBottom,

    /// <summary>
    /// view rol
    /// </summary>
    RightBottomLeft,

    /// <summary>
    /// view ror
    /// </summary>
    RightBottomRight,

    /// <summary>
    /// view rt
    /// </summary>
    RightTop,

    /// <summary>
    /// view ltl
    /// </summary>
    RightTopLeft,

    /// <summary>
    /// view ltr
    /// </summary>
    RightTopRight,

    //------------------------------------------ cube front side

    /// <summary>
    /// view fl
    /// </summary>
    FrontLeft,

    /// <summary>
    /// view fr
    /// </summary>
    FrontRight,

    /// <summary>
    /// view fo
    /// </summary>
    FrontBottom,

    /// <summary>
    /// view fol
    /// </summary>
    FrontBottomLeft,

    /// <summary>
    /// view for
    /// </summary>
    FrontBottomRight,

    /// <summary>
    /// view ft
    /// </summary>
    FrontTop,

    /// <summary>
    /// view ftl
    /// </summary>
    FrontTopLeft,

    /// <summary>
    /// view ftr
    /// </summary>
    FrontTopRight,

    //------------------------------------------ cube back face

    /// <summary>
    /// view bl
    /// </summary>
    BackLeft,

    /// <summary>
    /// view br
    /// </summary>
    BackRight,

    /// <summary>
    /// view bo
    /// </summary>
    BackBottom,

    /// <summary>
    /// view bol
    /// </summary>
    BackBottomLeft,

    /// <summary>
    /// view bor
    /// </summary>
    BackBottomRight,

    /// <summary>
    /// view bt
    /// </summary>
    BackTop,

    /// <summary>
    /// view btl
    /// </summary>
    BackTopLeft,

    /// <summary>
    /// view btr
    /// </summary>
    BackTopRight,

    //------------------------------------------ cube top face

    /// <summary>
    /// view tl
    /// </summary>
    TopLeft,

    /// <summary>
    /// view tr
    /// </summary>
    TopRight,

    /// <summary>
    /// view to
    /// </summary>
    TopBottom,

    /// <summary>
    /// view tol
    /// </summary>
    TopBottomLeft,

    /// <summary>
    /// view tor
    /// </summary>
    TopBottomRight,

    /// <summary>
    /// view tt
    /// </summary>
    TopTop,

    /// <summary>
    /// view ttl
    /// </summary>
    TopTopLeft,

    /// <summary>
    /// view ttr
    /// </summary>
    TopTopRight,

    //------------------------------------------ cube bottom face

    /// <summary>
    /// view ol
    /// </summary>
    BottomLeft,

    /// <summary>
    /// view or
    /// </summary>
    BottomRight,

    /// <summary>
    /// view oo
    /// </summary>
    BottomBottom,

    /// <summary>
    /// view ool
    /// </summary>
    BottomBottomLeft,

    /// <summary>
    /// view oor
    /// </summary>
    BottomBottomRight,

    /// <summary>
    /// view ot
    /// </summary>
    BottomTop,

    /// <summary>
    /// view otl
    /// </summary>
    BottomTopLeft,

    /// <summary>
    /// view otr
    /// </summary>
    BottomTopRight,

}
  
public static partial class Ext
{

    public static string ShortString(this CameraViewType cvt) => cvt switch
    {
        CameraViewType.Manual => "m",

        CameraViewType.Left => "l",
        CameraViewType.Right => "r",
        CameraViewType.Front => "f",
        CameraViewType.Back => "b",
        CameraViewType.Top => "t",
        CameraViewType.Bottom => "o",

        CameraViewType.LeftLeft => "ll",
        CameraViewType.LeftRight => "lr",
        CameraViewType.LeftBottom => "lo",
        CameraViewType.LeftBottomLeft => "lol",
        CameraViewType.LeftBottomRight => "lor",
        CameraViewType.LeftTop => "lt",
        CameraViewType.LeftTopLeft => "ltl",
        CameraViewType.LeftTopRight => "ltr",

        CameraViewType.RightLeft => "rl",
        CameraViewType.RightRight => "rr",
        CameraViewType.RightBottom => "ro",
        CameraViewType.RightBottomLeft => "rol",
        CameraViewType.RightBottomRight => "ror",
        CameraViewType.RightTop => "rt",
        CameraViewType.RightTopLeft => "rtl",
        CameraViewType.RightTopRight => "rtr",

        CameraViewType.FrontLeft => "fl",
        CameraViewType.FrontRight => "fr",
        CameraViewType.FrontBottom => "fo",
        CameraViewType.FrontBottomLeft => "fol",
        CameraViewType.FrontBottomRight => "for",
        CameraViewType.FrontTop => "ft",
        CameraViewType.FrontTopLeft => "ftl",
        CameraViewType.FrontTopRight => "ftr",

        CameraViewType.BackLeft => "bl",
        CameraViewType.BackRight => "br",
        CameraViewType.BackBottom => "bo",
        CameraViewType.BackBottomLeft => "bol",
        CameraViewType.BackBottomRight => "bor",
        CameraViewType.BackTop => "bt",
        CameraViewType.BackTopLeft => "btl",
        CameraViewType.BackTopRight => "btr",

        CameraViewType.TopLeft => "tl",
        CameraViewType.TopRight => "tr",
        CameraViewType.TopBottom => "to",
        CameraViewType.TopBottomLeft => "tol",
        CameraViewType.TopBottomRight => "tor",
        CameraViewType.TopTop => "tt",
        CameraViewType.TopTopLeft => "ttl",
        CameraViewType.TopTopRight => "ttr",

        CameraViewType.BottomLeft => "ol",
        CameraViewType.BottomRight => "or",
        CameraViewType.BottomBottom => "oo",
        CameraViewType.BottomBottomLeft => "ool",
        CameraViewType.BottomBottomRight => "oor",
        CameraViewType.BottomTop => "ot",
        CameraViewType.BottomTopLeft => "otl",
        CameraViewType.BottomTopRight => "otr",

        _ => throw new ArgumentException($"invalid view type {cvt}")
    };

}