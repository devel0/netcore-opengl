namespace SearchAThing.OpenGL.Core;

public partial class GLControl
{

    public CameraViewType? LastCameraView { get; private set; } = null;

    /// <summary>
    /// Set the view using given cube view type.
    /// </summary>
    /// <param name="type">Cube view type <see cref="CameraViewType"/>.</param>
    /// <param name="zoomFit">If true execute a zoom fit (Default: true).</param>
    /// <param name="invalidate">If true refresh the scene.</param>
    public void CameraView(CameraViewType type, bool zoomFit = true, bool invalidate = DEFAULT_INVALIDATE)
    {                   
        if (GLModel.LBBox.IsEmpty)
        {
            this.LastCameraView = type;
            return;
        }

        var bbox = GLModel.LBBox;

        ResetView();

        ModelMatrix = Matrix4x4.CreateTranslation(-bbox.Middle);
        
        var pi_2_rad = (float)(PI / 2d);
        var pi_rad = (float)PI;
        var angviewrad = DEFAULT_StdViewAngleDeg.ToRad();

        CameraPos = DEFAULT_CAMERA_POS;
        CameraTarget = DEFAULT_CAMERA_TARGET;
        CameraUp = DEFAULT_CAMERA_UP;

        var mBaseLeft = () => ModelMatrix * Matrix4x4.CreateRotationX(-pi_2_rad) * Matrix4x4.CreateRotationY(pi_2_rad);
        var mBaseRight = () => ModelMatrix * Matrix4x4.CreateRotationX(-pi_2_rad) * Matrix4x4.CreateRotationY(-pi_2_rad);
        var mBaseFront = () => ModelMatrix * Matrix4x4.CreateRotationX(-pi_2_rad);
        var mBaseBack = () => ModelMatrix * Matrix4x4.CreateRotationX(-pi_2_rad) * Matrix4x4.CreateRotationY(pi_rad);
        var mBaseTop = () => ModelMatrix;
        var mBaseBottom = () => ModelMatrix * Matrix4x4.CreateRotationY(pi_rad);

        var mAddLeft = () => Matrix4x4.CreateRotationY(angviewrad);
        var mAddRight = () => Matrix4x4.CreateRotationY(-angviewrad);
        var mAddBottom = () => Matrix4x4.CreateRotationX(-angviewrad);
        var mAddTop = () => Matrix4x4.CreateRotationX(angviewrad);

        switch (type)
        {

            #region ----------------------------------------- cube std face views

            case CameraViewType.Left:
                ModelMatrix = mBaseLeft();
                break;

            case CameraViewType.Right:
                ModelMatrix = mBaseRight();
                break;

            case CameraViewType.Front:
                ModelMatrix = mBaseFront();
                break;

            case CameraViewType.Back:
                ModelMatrix = mBaseBack();
                break;

            case CameraViewType.Top:
                ModelMatrix = mBaseTop();
                break;

            case CameraViewType.Bottom:
                ModelMatrix = mBaseBottom();
                break;

            #endregion

            #region ----------------------------------------- cube left side

            case CameraViewType.LeftLeft:
                ModelMatrix = mBaseLeft() * mAddLeft();
                break;

            case CameraViewType.LeftRight:
                ModelMatrix = mBaseLeft() * mAddRight();
                break;

            case CameraViewType.LeftBottom:
                ModelMatrix = mBaseLeft() * mAddBottom();
                break;

            case CameraViewType.LeftBottomLeft:
                ModelMatrix = mBaseLeft() * mAddBottom() * mAddLeft();
                break;

            case CameraViewType.LeftBottomRight:
                ModelMatrix = mBaseLeft() * mAddBottom() * mAddRight();
                break;

            case CameraViewType.LeftTop:
                ModelMatrix = mBaseLeft() * mAddTop();
                break;

            case CameraViewType.LeftTopLeft:
                ModelMatrix = mBaseLeft() * mAddTop() * mAddLeft();
                break;

            case CameraViewType.LeftTopRight:
                ModelMatrix = mBaseLeft() * mAddTop() * mAddRight();
                break;

            #endregion

            #region ----------------------------------------- cube right side

            case CameraViewType.RightLeft:
                ModelMatrix = mBaseRight() * mAddLeft();
                break;

            case CameraViewType.RightRight:
                ModelMatrix = mBaseRight() * mAddRight();
                break;

            case CameraViewType.RightBottom:
                ModelMatrix = mBaseRight() * mAddBottom();
                break;

            case CameraViewType.RightBottomLeft:
                ModelMatrix = mBaseRight() * mAddBottom() * mAddLeft();
                break;

            case CameraViewType.RightBottomRight:
                ModelMatrix = mBaseRight() * mAddBottom() * mAddRight();
                break;

            case CameraViewType.RightTop:
                ModelMatrix = mBaseRight() * mAddTop();
                break;

            case CameraViewType.RightTopLeft:
                ModelMatrix = mBaseRight() * mAddTop() * mAddLeft();
                break;

            case CameraViewType.RightTopRight:
                ModelMatrix = mBaseRight() * mAddTop() * mAddRight();
                break;

            #endregion

            #region ----------------------------------------- cube front side

            case CameraViewType.FrontLeft:
                ModelMatrix = mBaseFront() * mAddLeft();
                break;

            case CameraViewType.FrontRight:
                ModelMatrix = mBaseFront() * mAddRight();
                break;

            case CameraViewType.FrontBottom:
                ModelMatrix = mBaseFront() * mAddBottom();
                break;

            case CameraViewType.FrontBottomLeft:
                ModelMatrix = mBaseFront() * mAddBottom() * mAddLeft();
                break;

            case CameraViewType.FrontBottomRight:
                ModelMatrix = mBaseFront() * mAddBottom() * mAddRight();
                break;

            case CameraViewType.FrontTop:
                ModelMatrix = mBaseFront() * mAddTop();
                break;

            case CameraViewType.FrontTopLeft:
                ModelMatrix = mBaseFront() * mAddTop() * mAddLeft();
                break;

            case CameraViewType.FrontTopRight:
                ModelMatrix = mBaseFront() * mAddTop() * mAddRight();
                break;

            #endregion

            #region ----------------------------------------- cube back side                

            case CameraViewType.BackLeft:
                ModelMatrix = mBaseBack() * mAddLeft();
                break;

            case CameraViewType.BackRight:
                ModelMatrix = mBaseBack() * mAddRight();
                break;

            case CameraViewType.BackBottom:
                ModelMatrix = mBaseBack() * mAddBottom();
                break;

            case CameraViewType.BackBottomLeft:
                ModelMatrix = mBaseBack() * mAddBottom() * mAddLeft();
                break;

            case CameraViewType.BackBottomRight:
                ModelMatrix = mBaseBack() * mAddBottom() * mAddRight();
                break;

            case CameraViewType.BackTop:
                ModelMatrix = mBaseBack() * mAddTop();
                break;

            case CameraViewType.BackTopLeft:
                ModelMatrix = mBaseBack() * mAddTop() * mAddLeft();
                break;

            case CameraViewType.BackTopRight:
                ModelMatrix = mBaseBack() * mAddTop() * mAddRight();
                break;

            #endregion

            #region ----------------------------------------- cube top side                

            case CameraViewType.TopLeft:
                ModelMatrix = mBaseTop() * mAddLeft();
                break;

            case CameraViewType.TopRight:
                ModelMatrix = mBaseTop() * mAddRight();
                break;

            case CameraViewType.TopBottom:
                ModelMatrix = mBaseTop() * mAddBottom();
                break;

            case CameraViewType.TopBottomLeft:
                ModelMatrix = mBaseTop() * mAddBottom() * mAddLeft();
                break;

            case CameraViewType.TopBottomRight:
                ModelMatrix = mBaseTop() * mAddBottom() * mAddRight();
                break;

            case CameraViewType.TopTop:
                ModelMatrix = mBaseTop() * mAddTop();
                break;

            case CameraViewType.TopTopLeft:
                ModelMatrix = mBaseTop() * mAddTop() * mAddLeft();
                break;

            case CameraViewType.TopTopRight:
                ModelMatrix = mBaseTop() * mAddTop() * mAddRight();
                break;

            #endregion

            #region ----------------------------------------- cube bottom side                

            case CameraViewType.BottomLeft:
                ModelMatrix = mBaseBottom() * mAddLeft();
                break;

            case CameraViewType.BottomRight:
                ModelMatrix = mBaseBottom() * mAddRight();
                break;

            case CameraViewType.BottomBottom:
                ModelMatrix = mBaseBottom() * mAddBottom();
                break;

            case CameraViewType.BottomBottomLeft:
                ModelMatrix = mBaseBottom() * mAddBottom() * mAddLeft();
                break;

            case CameraViewType.BottomBottomRight:
                ModelMatrix = mBaseBottom() * mAddBottom() * mAddRight();
                break;

            case CameraViewType.BottomTop:
                ModelMatrix = mBaseBottom() * mAddTop();
                break;

            case CameraViewType.BottomTopLeft:
                ModelMatrix = mBaseBottom() * mAddTop() * mAddLeft();
                break;

            case CameraViewType.BottomTopRight:
                ModelMatrix = mBaseBottom() * mAddTop() * mAddRight();
                break;

                #endregion

        }

        if (zoomFit) ZoomFit(invalidate: false);

        if (invalidate) Invalidate();

        LastCameraView = type;
    }

}
