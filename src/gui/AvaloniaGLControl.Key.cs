using Avalonia.Threading;

namespace SearchAThing.OpenGL.GUI;

public partial class AvaloniaGLControl
{

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ToggleWireframe"/>.
    /// </summary>
    public KeyGesture? ToggleWireframeGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ToggleShadeWithEdge"/>.
    /// </summary>
    public KeyGesture? ToggleShadeWithEdge = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ToggleShowNormals"/>.
    /// </summary>
    public KeyGesture? ToggleShowNormals = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.TogglePerspective"/>.
    /// </summary>
    public KeyGesture? TogglePerspectiveGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ToggleUseTexture"/>.
    /// </summary>
    public KeyGesture? ToggleUseTextureGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ToggleUseShadow"/>.
    /// </summary>
    public KeyGesture? ToggleUseShadowGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.SwitchSelectionMode"/>.
    /// </summary>
    public KeyGesture? SwitchSelectionModeGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.PanLeft"/>.
    /// </summary>
    public KeyGesture? PanLeftGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.PanRight"/>.
    /// </summary>
    public KeyGesture? PanRightGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.PanUp"/>.
    /// </summary>
    public KeyGesture? PanUpGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.PanDown"/>.
    /// </summary>
    public KeyGesture? PanDownGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ModelRotateLeft"/>.
    /// </summary>
    public KeyGesture? ModelRotateLeftGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ModelRotateRight"/>.
    /// </summary>
    public KeyGesture? ModelRotateRightGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ModelRotateUp"/>.
    /// </summary>
    public KeyGesture? ModelRotateUpGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ModelRotateDown"/>.
    /// </summary>
    public KeyGesture? ModelRotateDownGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraRotateLeft"/>.
    /// </summary>
    public KeyGesture? CameraRotateLeftGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraRotateRight"/>.
    /// </summary>
    public KeyGesture? CameraRotateRightGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraRotateUp"/>.
    /// </summary>
    public KeyGesture? CameraRotateUpGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraRotateDown"/>.
    /// </summary>
    public KeyGesture? CameraRotateDownGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraZoomIn"/>.
    /// </summary>
    public KeyGesture? CameraZoomInGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraZoomOut"/>.
    /// </summary>
    public KeyGesture? CameraZoomOutGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.TiltRight"/>.
    /// </summary>
    public KeyGesture? CameraTiltRightGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.TiltLeft"/>.
    /// </summary>
    public KeyGesture? CameraTiltLeftGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraView"/> with <see cref="CameraViewType.Top"/>.
    /// </summary>
    public KeyGesture? ViewTopGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraView"/> with <see cref="CameraViewType.Bottom"/>.
    /// </summary>
    public KeyGesture? ViewBottomGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraView"/> with <see cref="CameraViewType.Left"/>.
    /// </summary>
    public KeyGesture? ViewLeftGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraView"/> with <see cref="CameraViewType.Right"/>.
    /// </summary>
    public KeyGesture? ViewRightGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraView"/> with <see cref="CameraViewType.Front"/>.
    /// </summary>
    public KeyGesture? ViewFrontGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.CameraView"/> with <see cref="CameraViewType.Back"/>.
    /// </summary>
    public KeyGesture? ViewBackGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ZoomFit"/>.
    /// </summary>
    public KeyGesture? ZoomFitGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.Invalidate"/>.
    /// </summary>
    public KeyGesture? InvalidateGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="AvaloniaGLControl.ShowDevTool"/>.
    /// </summary>
    public KeyGesture? ShowDevToolGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.SaveView"/>.
    /// </summary>
    public KeyGesture? SaveViewGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.LoadView"/>.
    /// </summary>
    public KeyGesture? RestoreViewGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="AvaloniaGLControl.SaveViewLayout"/>.
    /// </summary>
    public KeyGesture? SaveViewLayoutGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="AvaloniaGLControl.LoadViewLayout"/>.
    /// </summary>
    public KeyGesture? RestoreViewLayoutGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="AvaloniaGLControl.SplitHorizontal"/>.
    /// </summary>
    public KeyGesture? SplitViewHorizontalGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="AvaloniaGLControl.SplitVertical"/>.
    /// </summary>
    public KeyGesture? SplitViewVerticalGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="AvaloniaGLControl.SplitClose"/>.
    /// </summary>
    public KeyGesture? SplitViewCloseGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ShowModelBBox"/>.
    /// </summary>
    public KeyGesture? ToggleModelBBoxGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ShowCameraObject"/>.
    /// </summary>
    public KeyGesture? ToggleCameraObjectGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.ToggleIdentifyCoord"/>.
    /// </summary>
    public KeyGesture? ToggleIdentifyCoordGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLControl.SetRotationCenter"/>.
    /// </summary>
    public KeyGesture? SetRotationCenterGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLModel.ClearSelection"/>.
    /// </summary>
    public KeyGesture? ClearSelectionGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLModel.GetSelectionSimpleCmd"/>.
    /// </summary>
    public KeyGesture? CopySimpleCmdOfSelectionGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLModel.PasteSimpleCmd"/>.
    /// </summary>
    public KeyGesture? PasteSimpleCmdGesture = null;

    /// <summary>
    /// Key gesture associated to <see cref="GLModel.DeleteSelected"/>.
    /// </summary>
    public KeyGesture? DeleteSelectedGesture = null;

    //    

    /// <summary>
    /// Deassociate all key gestures.
    /// </summary>
    public void ClearKeyGestures()
    {
        ToggleWireframeGesture =
        ToggleShadeWithEdge =
        ToggleShowNormals =
        TogglePerspectiveGesture =
        ToggleUseTextureGesture =
        ToggleUseShadowGesture =
        SwitchSelectionModeGesture =

        PanLeftGesture =
        PanRightGesture =
        PanUpGesture =
        PanDownGesture =

        ModelRotateLeftGesture =
        ModelRotateRightGesture =
        ModelRotateUpGesture =
        ModelRotateDownGesture =

        CameraRotateLeftGesture =
        CameraRotateRightGesture =
        CameraRotateUpGesture =
        CameraRotateDownGesture =

        CameraZoomInGesture =
        CameraZoomOutGesture =

        CameraTiltLeftGesture =
        CameraTiltRightGesture =

        ViewBottomGesture =
        ViewTopGesture =
        ViewLeftGesture =
        ViewRightGesture =
        ViewFrontGesture =
        ViewBackGesture =

        ZoomFitGesture =
        InvalidateGesture =

        ShowDevToolGesture =
        SaveViewGesture =
        RestoreViewGesture =
        SaveViewLayoutGesture =
        RestoreViewLayoutGesture =

        SplitViewHorizontalGesture =
        SplitViewVerticalGesture =
        SplitViewCloseGesture =

        ToggleModelBBoxGesture =
        ToggleCameraObjectGesture =
        ToggleIdentifyCoordGesture =
        SetRotationCenterGesture =

        ClearSelectionGesture =
        CopySimpleCmdOfSelectionGesture =
        PasteSimpleCmdGesture =
        DeleteSelectedGesture =

        null;
    }

    /// <summary>
    /// Set all default key gestures.
    /// </summary>
    public void SetDefaultKeyGestures()
    {
        ToggleWireframeGesture = DEFAULT_ToggleWireframeGesture;
        ToggleShadeWithEdge = DEFAULT_ToggleShadeWithEdgeGesture;
        ToggleShowNormals = DEFAULT_ToggleShowNormalsGesture;
        TogglePerspectiveGesture = DEFAULT_TogglePerspectiveGesture;
        ToggleUseTextureGesture = DEFAULT_ToggleUseTextureGesture;
        ToggleUseShadowGesture = DEFAULT_ToggleUseShadowGesture;
        SwitchSelectionModeGesture = DEFAULT_SwitchSelectionModeGesture;

        PanLeftGesture = DEFAULT_CameraPanLeftGesture;
        PanRightGesture = DEFAULT_CameraPanRightGesture;
        PanUpGesture = DEFAULT_CameraPanUpGesture;
        PanDownGesture = DEFAULT_CameraPanDownGesture;

        ModelRotateLeftGesture = DEFAULT_ModelRotateLeftGesture;
        ModelRotateRightGesture = DEFAULT_ModelRotateRightGesture;
        ModelRotateUpGesture = DEFAULT_ModelRotateUpGesture;
        ModelRotateDownGesture = DEFAULT_ModelRotateDownGesture;

        CameraRotateLeftGesture = DEFAULT_CameraRotateLeftGesture;
        CameraRotateRightGesture = DEFAULT_CameraRotateRightGesture;
        CameraRotateUpGesture = DEFAULT_CameraRotateUpGesture;
        CameraRotateDownGesture = DEFAULT_CameraRotateDownGesture;

        CameraZoomInGesture = DEFAULT_CameraZoomInGesture;
        CameraZoomOutGesture = DEFAULT_CameraZoomOutGesture;

        CameraTiltLeftGesture = DEFAULT_CameraTiltLeftGesture;
        CameraTiltRightGesture = DEFAULT_CameraTiltRightGesture;

        ViewBottomGesture = DEFAULT_ViewBottomGesture;
        ViewTopGesture = DEFAULT_ViewTopGesture;
        ViewLeftGesture = DEFAULT_ViewLeftGesture;
        ViewRightGesture = DEFAULT_ViewRightGesture;
        ViewFrontGesture = DEFAULT_ViewFrontGesture;
        ViewBackGesture = DEFAULT_ViewBackGesture;

        ZoomFitGesture = DEFAULT_ZoomFitGesture;
        InvalidateGesture = DEFAULT_InvalidateGesture;
        ToggleIdentifyCoordGesture = DEFAULT_ToggleIdentifyCoordGesture;

        ShowDevToolGesture = DEFAULT_DevTool;
        SaveViewGesture = DEFAULT_SaveView;
        RestoreViewGesture = DEFAULT_RestoreView;
        SaveViewLayoutGesture = DEFAULT_SaveViewLayout;
        RestoreViewLayoutGesture = DEFAULT_RestoreViewLayout;

        SplitViewHorizontalGesture = DEFAULT_SplitViewHorizontal;
        SplitViewVerticalGesture = DEFAULT_SplitViewVertical;
        SplitViewCloseGesture = DEFAULT_SplitViewClose;

        ToggleModelBBoxGesture = DEFAULT_ToggleModelBBox;
        ToggleCameraObjectGesture = DEFAULT_ToggleCameraObject;
        SetRotationCenterGesture = DEFAULT_SetRotationCenterGesture;

        ClearSelectionGesture = DEFAULT_ClearSelectionGesture;
        CopySimpleCmdOfSelectionGesture = DEFAULT_CopySimpleCmdOfSelectionGesture;
        PasteSimpleCmdGesture = DEFAULT_PasteSimpleCmdGesture;
        DeleteSelectedGesture = DEFAULT_DeleteSelectedGesture;
    }

    public void HandleKeyDown(KeyEventArgs e)
    {
        bool MatchGesture(KeyGesture? gesture) => gesture?.Matches(e) == true;

        if (MatchGesture(ToggleWireframeGesture)) { GLControl.ToggleWireframe(); e.Handled = true; return; }
        if (MatchGesture(ToggleShadeWithEdge)) { GLControl.ToggleShadeWithEdge(); e.Handled = true; return; }
        if (MatchGesture(ToggleShowNormals)) { GLControl.ToggleShowNormals(); e.Handled = true; return; }
        if (MatchGesture(TogglePerspectiveGesture)) { GLControl.TogglePerspective(); e.Handled = true; return; }
        if (MatchGesture(ToggleUseTextureGesture)) { GLControl.ToggleUseTexture(); e.Handled = true; return; }
        if (MatchGesture(ToggleUseShadowGesture)) { GLControl.ToggleUseShadow(); e.Handled = true; return; }
        if (MatchGesture(SwitchSelectionModeGesture)) { GLControl.SwitchSelectionMode(); e.Handled = true; return; }

        //

        if (MatchGesture(PanLeftGesture)) { GLControl.PanLeft(); e.Handled = true; return; }
        if (MatchGesture(PanRightGesture)) { GLControl.PanRight(); e.Handled = true; return; }
        if (MatchGesture(PanUpGesture)) { GLControl.PanUp(); e.Handled = true; return; }
        if (MatchGesture(PanDownGesture)) { GLControl.PanDown(); e.Handled = true; return; }

        //

        if (MatchGesture(ModelRotateLeftGesture)) { GLControl.ModelRotateLeft(); e.Handled = true; return; }
        if (MatchGesture(ModelRotateRightGesture)) { GLControl.ModelRotateRight(); e.Handled = true; return; }
        if (MatchGesture(ModelRotateUpGesture)) { GLControl.ModelRotateUp(); e.Handled = true; return; }
        if (MatchGesture(ModelRotateDownGesture)) { GLControl.ModelRotateDown(); e.Handled = true; return; }

        //

        if (MatchGesture(CameraRotateLeftGesture)) { GLControl.CameraRotateLeft(); e.Handled = true; return; }
        if (MatchGesture(CameraRotateRightGesture)) { GLControl.CameraRotateRight(); e.Handled = true; return; }
        if (MatchGesture(CameraRotateUpGesture)) { GLControl.CameraRotateUp(); e.Handled = true; return; }
        if (MatchGesture(CameraRotateDownGesture)) { GLControl.CameraRotateDown(); e.Handled = true; return; }

        //

        if (MatchGesture(CameraZoomInGesture)) { GLControl.CameraZoomIn(); e.Handled = true; return; }
        if (MatchGesture(CameraZoomOutGesture)) { GLControl.CameraZoomOut(); e.Handled = true; return; }

        //

        if (MatchGesture(CameraTiltLeftGesture)) { GLControl.TiltLeft(); e.Handled = true; return; }
        if (MatchGesture(CameraTiltRightGesture)) { GLControl.TiltRight(); e.Handled = true; return; }

        //

        if (MatchGesture(ViewBottomGesture)) { GLControl.CameraView(CameraViewType.Bottom); e.Handled = true; return; }
        if (MatchGesture(ViewTopGesture)) { GLControl.CameraView(CameraViewType.Top); e.Handled = true; return; }
        if (MatchGesture(ViewLeftGesture)) { GLControl.CameraView(CameraViewType.Left); e.Handled = true; return; }
        if (MatchGesture(ViewRightGesture)) { GLControl.CameraView(CameraViewType.Right); e.Handled = true; return; }
        if (MatchGesture(ViewFrontGesture)) { GLControl.CameraView(CameraViewType.Front); e.Handled = true; return; }
        if (MatchGesture(ViewBackGesture)) { GLControl.CameraView(CameraViewType.Back); e.Handled = true; return; }

        //

        if (MatchGesture(ZoomFitGesture)) { GLControl.ZoomFit(); e.Handled = true; return; }
        if (MatchGesture(InvalidateGesture)) { GLControl.Invalidate(InvalidateEnum.RebuildModelAndRedraw); e.Handled = true; return; }
        if (MatchGesture(ToggleIdentifyCoordGesture)) { GLControl.ToggleIdentifyCoord(); e.Handled = true; return; }

        //

        if (MatchGesture(ShowDevToolGesture)) { ShowDevTool(); e.Handled = true; return; }
        if (MatchGesture(SaveViewLayoutGesture)) { SaveViewLayout(); e.Handled = true; return; };
        if (MatchGesture(RestoreViewLayoutGesture)) { LoadViewLayout(); e.Handled = true; return; };
        if (MatchGesture(SaveViewGesture)) { GLControl.SaveView(); e.Handled = true; return; };
        if (MatchGesture(RestoreViewGesture)) { GLControl.LoadView(); e.Handled = true; return; };
        if (MatchGesture(SetRotationCenterGesture)) { GLControl.SetRotationCenter(); e.Handled = true; return; }

        //

        if (MatchGesture(SplitViewHorizontalGesture))
        {
            var curCtl = GridSplitterManager.FocusedControl;
            SplitHorizontal();
            curCtl?.AvaloniaGLControl.GLControl.Invalidate();
            e.Handled = true;
            return;
        }

        if (MatchGesture(SplitViewVerticalGesture))
        {
            var curCtl = GridSplitterManager.FocusedControl;
            SplitVertical();
            curCtl?.AvaloniaGLControl.GLControl.Invalidate();
            e.Handled = true;
            return;
        }

        if (MatchGesture(SplitViewCloseGesture)) { SplitClose(); e.Handled = true; return; }

        //

        if (MatchGesture(ToggleModelBBoxGesture))
        {
            GLControl.ToggleShowModelBBox();
            return;
        }

        if (MatchGesture(ToggleCameraObjectGesture))
        {
            GLControl.ToggleCameraObject();
            return;
        }

        //

        if (MatchGesture(ClearSelectionGesture))
        {
            GLControl.GLModel.ClearSelection();
            GridSplitterManager.Invalidate();
            return;
        }

        if (MatchGesture(CopySimpleCmdOfSelectionGesture))
        {
            var txt = GLControl.GLModel.GetSelectionSimpleCmd();
            Task.Run(async () =>
            {
                var clip = Application.Current?.Clipboard;
                if (clip is not null) await clip!.SetTextAsync(txt);
            });
            // CopyToClipboardRequest?.Invoke(sb.ToString());
            GLControl.GLModel.SendNotification("Copy", "Selection (SimpleCmd) copied");
            return;
        }

        if (MatchGesture(PasteSimpleCmdGesture))
        {
            Task.Run(async () =>
            {
                try
                {
                    var clip = Application.Current?.Clipboard;
                    if (clip is not null)
                    {
                        var txt = await clip!.GetTextAsync();

                        Dispatcher.UIThread.Post(() =>
                        {
                            var figs = GLControl.GLModel.PasteSimpleCmd(txt).ToList();
                            var primitivesCnt = figs.Select(w => w.Primitives.Count).Sum();

                            GLControl.InvalidateAll();
                            GLControl.GLModel.SendNotification("Paste", $"{figs.Count} figures ({primitivesCnt} primitives) pasted.");
                        });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(() =>
                        GLControl.GLModel.SendNotification("Paste", "Paste of given SimpleCmd failed."));
                }
            });
            return;
        }

        if (MatchGesture(DeleteSelectedGesture))
        {
            GLControl.GLModel.DeleteSelected();
            GridSplitterManager.Invalidate();
            return;
        }

    }

}