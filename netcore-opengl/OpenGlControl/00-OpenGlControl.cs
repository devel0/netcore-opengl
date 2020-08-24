using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using static System.Math;
using System.Numerics;
using Avalonia.Input;
using System;
using System.Linq;

namespace SearchAThing
{

    public class GlControlMatrixSource
    {
        public Matrix4x4 ModelMatrix { get; private set; }
        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; private set; }

        public Vector3 CameraPos { get; private set; }
        public Vector3 CameraTarget { get; private set; }
        public Vector3 CameraUp { get; private set; }

        public bool Perspective { get; private set; }
        public float OrthoZoom { get; private set; }

        public float FovDeg { get; private set; }
        public float Near { get; private set; }
        public float Far { get; private set; }

        public GlControlMatrixSource(OpenGlControl glControl)
        {
            ModelMatrix = glControl.ModelMatrix;
            ViewMatrix = glControl.ViewMatrix;
            ProjectionMatrix = glControl.ProjectionMatrix;

            CameraPos = glControl.CameraPos;
            CameraTarget = glControl.CameraTarget;
            CameraUp = glControl.CameraUp;

            Perspective = glControl.Perspective;
            OrthoZoom = glControl.OrthoZoom;

            FovDeg = glControl.FovDeg;
            Near = glControl.Near;
            Far = glControl.Far;
        }
    }

    public partial class OpenGlControl : Control
    {

        bool initialized = false;

        #region Model
        private OpenGlModelBase _Model = null;

        public static readonly DirectProperty<OpenGlControl, OpenGlModelBase> ModelProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, OpenGlModelBase>("Model", o => o.Model, (o, v) => o.Model = v);

        public OpenGlModelBase Model
        {
            get => _Model;
            set
            {
                var attachModel = false;
                if (_Model != value)
                {
                    if (_Model != null)
                    {
                        Model.Deregister(this);
                        this.PointerPressed -= ControlPointerPressed;
                        this.PointerWheelChanged -= ControlPointerWheelChanged;
                        this.PointerReleased -= ControlPointerReleased;
                        this.PointerMoved -= ControlPointerMoved;
                        this.GotFocus -= ControlGotFocus;
                        this.KeyDown += ControlKeyDown;
                        this.KeyUp += ControlKeyUp;
                        OnDetachModel();
                    }
                    attachModel = true;
                }
                SetAndRaise(ModelProperty, ref _Model, value);
                if (attachModel)
                {
                    Model.Register(this);
                    this.PointerPressed += ControlPointerPressed;
                    this.PointerMoved += ControlPointerMoved;
                    this.PointerReleased += ControlPointerReleased;
                    this.PointerWheelChanged += ControlPointerWheelChanged;
                    this.GotFocus += ControlGotFocus;
                    this.KeyDown += ControlKeyDown;
                    this.KeyUp += ControlKeyUp;
                    OnModelAttached();
                }
            }
        }
        #endregion

        // ==[ MODEL MATRIX ]======================================================================

        #region ModelMatrix
        private Matrix4x4 _ModelMatrix = Matrix4x4.Identity;

        public static readonly DirectProperty<OpenGlControl, Matrix4x4> ModelMatrixProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, Matrix4x4>("ModelMatrix", o => o.ModelMatrix, (o, v) => o.ModelMatrix = v);

        public Matrix4x4 ModelMatrix
        {
            get => _ModelMatrix;
            set => SetAndRaise(ModelMatrixProperty, ref _ModelMatrix, value);
        }
        #endregion       

        // ==[ VIEW ]==============================================================================

        #region ViewMatrix
        private Matrix4x4 _ViewMatrix = Matrix4x4.Identity;

        public static readonly DirectProperty<OpenGlControl, Matrix4x4> ViewMatrixProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, Matrix4x4>("ViewMatrix", o => o.ViewMatrix);//, (o, v) => o.ViewMatrix = v);

        public Matrix4x4 ViewMatrix
        {
            get => _ViewMatrix;
            //set => SetAndRaise(ViewMatrixProperty, ref _ViewMatrix, value);
        }
        #endregion 

        void RebuildViewMatrix()
        {
            SetAndRaise(ViewMatrixProperty, ref _ViewMatrix, Matrix4x4.CreateLookAt(CameraPos, CameraTarget, CameraUp));
        }

        public Vector3 CameraPos
        {
            get { return new Vector3(CameraPosX, CameraPosY, CameraPosZ); }
            set
            {
                var oldCameraPosX = _CameraPosX;
                var oldCameraPosY = _CameraPosY;
                var oldCameraPosZ = _CameraPosZ;

                var changed = value.X != oldCameraPosX || value.Y != oldCameraPosY || value.Z != oldCameraPosZ;
                _CameraPosX = value.X;
                _CameraPosY = value.Y;
                _CameraPosZ = value.Z;

                RaisePropertyChanged(CameraPosXProperty, oldCameraPosX, value.X);
                RaisePropertyChanged(CameraPosYProperty, oldCameraPosY, value.Y);
                RaisePropertyChanged(CameraPosZProperty, oldCameraPosZ, value.Z);

                if (changed) RebuildViewMatrix();
            }
        }

        #region CameraPosX
        private float _CameraPosX = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraPosXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraPosX", o => o.CameraPosX, (o, v) => o.CameraPosX = v);

        public float CameraPosX
        {
            get => _CameraPosX;
            set
            {
                var changed = value != _CameraPosX;
                SetAndRaise(CameraPosXProperty, ref _CameraPosX, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion    

        #region CameraPosY
        private float _CameraPosY = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraPosYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraPosY", o => o.CameraPosY, (o, v) => o.CameraPosY = v);

        public float CameraPosY
        {
            get => _CameraPosY;
            set
            {
                var changed = value != _CameraPosY;
                SetAndRaise(CameraPosYProperty, ref _CameraPosY, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion         

        #region CameraPosZ
        private float _CameraPosZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraPosZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraPosZ", o => o.CameraPosZ, (o, v) => o.CameraPosZ = v);

        public float CameraPosZ
        {
            get => _CameraPosZ;
            set
            {
                var changed = value != _CameraPosZ;
                SetAndRaise(CameraPosZProperty, ref _CameraPosZ, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion

        public Vector3 CameraTarget
        {
            get { return new Vector3(CameraTargetX, CameraTargetY, CameraTargetZ); }
            set
            {
                var oldCameraTargetX = _CameraTargetX;
                var oldCameraTargetY = _CameraTargetY;
                var oldCameraTargetZ = _CameraTargetZ;

                var changed = value.X != oldCameraTargetX || value.Y != oldCameraTargetY || value.Z != oldCameraTargetZ;
                _CameraTargetX = value.X;
                _CameraTargetY = value.Y;
                _CameraTargetZ = value.Z;

                RaisePropertyChanged(CameraTargetXProperty, oldCameraTargetX, value.X);
                RaisePropertyChanged(CameraTargetYProperty, oldCameraTargetY, value.Y);
                RaisePropertyChanged(CameraTargetZProperty, oldCameraTargetZ, value.Z);

                if (changed) RebuildViewMatrix();
            }
        }

        #region CameraTargetX
        private float _CameraTargetX = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraTargetXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraTargetX", o => o.CameraTargetX, (o, v) => o.CameraTargetX = v);

        public float CameraTargetX
        {
            get => _CameraTargetX;
            set
            {
                var changed = value != _CameraTargetX;
                SetAndRaise(CameraTargetXProperty, ref _CameraTargetX, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion    

        #region CameraTargetY
        private float _CameraTargetY = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraTargetYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraTargetY", o => o.CameraTargetY, (o, v) => o.CameraTargetY = v);

        public float CameraTargetY
        {
            get => _CameraTargetY;
            set
            {
                var changed = value != _CameraTargetY;
                SetAndRaise(CameraTargetYProperty, ref _CameraTargetY, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion    

        #region CameraTargetZ
        private float _CameraTargetZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraTargetZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraTargetZ", o => o.CameraTargetZ, (o, v) => o.CameraTargetZ = v);

        public float CameraTargetZ
        {
            get => _CameraTargetZ;
            set
            {
                var changed = value != _CameraTargetZ;
                SetAndRaise(CameraTargetZProperty, ref _CameraTargetZ, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion

        public Vector3 CameraUp
        {
            get { return new Vector3(CameraUpX, CameraUpY, CameraUpZ); }
            set
            {
                var oldCameraUpX = _CameraUpX;
                var oldCameraUpY = _CameraUpY;
                var oldCameraUpZ = _CameraUpZ;

                var changed = value.X != oldCameraUpX || value.Y != oldCameraUpY || value.Z != oldCameraUpZ;
                _CameraUpX = value.X;
                _CameraUpY = value.Y;
                _CameraUpZ = value.Z;

                RaisePropertyChanged(CameraUpXProperty, oldCameraUpX, value.X);
                RaisePropertyChanged(CameraUpYProperty, oldCameraUpY, value.Y);
                RaisePropertyChanged(CameraUpZProperty, oldCameraUpZ, value.Z);

                if (changed) RebuildViewMatrix();
            }
        }

        #region CameraUpX
        private float _CameraUpX = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraUpXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraUpX", o => o.CameraUpX, (o, v) => o.CameraUpX = v);

        public float CameraUpX
        {
            get => _CameraUpX;
            set
            {
                var changed = value != _CameraUpX;
                SetAndRaise(CameraUpXProperty, ref _CameraUpX, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion    

        #region CameraUpY
        private float _CameraUpY = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraUpYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraUpY", o => o.CameraUpY, (o, v) => o.CameraUpY = v);

        public float CameraUpY
        {
            get => _CameraUpY;
            set
            {
                var changed = value != _CameraUpY;
                SetAndRaise(CameraUpYProperty, ref _CameraUpY, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion    

        #region CameraUpZ
        private float _CameraUpZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraUpZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraUpZ", o => o.CameraUpZ, (o, v) => o.CameraUpZ = v);

        public float CameraUpZ
        {
            get => _CameraUpZ;
            set
            {
                var changed = value != _CameraUpZ;
                SetAndRaise(CameraUpZProperty, ref _CameraUpZ, value);
                if (changed) RebuildViewMatrix();
            }
        }
        #endregion

        /// <summary>
        /// set view matrix all at once avoiding rebuild over property changed
        /// </summary>        
        public void SetViewMatrix(Vector3 cameraPos, Vector3 cameraTarget, Vector3 cameraUp)
        {
            SetAndRaise(CameraPosXProperty, ref _CameraPosX, cameraPos.X);
            SetAndRaise(CameraPosYProperty, ref _CameraPosY, cameraPos.Y);
            SetAndRaise(CameraPosZProperty, ref _CameraPosZ, cameraPos.Z);

            SetAndRaise(CameraTargetXProperty, ref _CameraTargetX, cameraTarget.X);
            SetAndRaise(CameraTargetYProperty, ref _CameraTargetY, cameraTarget.Y);
            SetAndRaise(CameraTargetZProperty, ref _CameraTargetZ, cameraTarget.Z);

            SetAndRaise(CameraUpXProperty, ref _CameraUpX, cameraUp.X);
            SetAndRaise(CameraUpYProperty, ref _CameraUpY, cameraUp.Y);
            SetAndRaise(CameraUpZProperty, ref _CameraUpZ, cameraUp.Z);

            RebuildViewMatrix();
        }

        // ==[ PROJECTION ]========================================================================

        #region ProjectionMatrix
        private Matrix4x4 _ProjectionMatrix = Matrix4x4.Identity;

        public static readonly DirectProperty<OpenGlControl, Matrix4x4> ProjectionMatrixProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, Matrix4x4>("ProjectionMatrix", o => o.ProjectionMatrix, (o, v) => o.ProjectionMatrix = v);

        public Matrix4x4 ProjectionMatrix
        {
            get => _ProjectionMatrix;
            set => SetAndRaise(ProjectionMatrixProperty, ref _ProjectionMatrix, value);
        }
        #endregion

        public void RebuildProjectionMatrix()
        {
            var ar = Bounds.Size.AspectRatio;

            if (Perspective && FovDeg > 0)
                ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FovDeg.ToRad(), (float)ar, Near, Far);
            else
            {
                var ccs = CameraCS;
                CameraPos = (Vector3D)CameraTarget + ccs.BaseZ;

                var mm = ModelMatrix;

                var obbox = new BBox3D(Model.BBox.Points.Select(w => (Vector3D)Vector3.Transform(w, ViewMatrix)));
                var obboxSize = obbox.Size;
                var obboxWidth = obboxSize.X;
                var obboxHeight = obboxSize.Y;
                var oAr = obboxWidth / obboxHeight;

                var ogWidth = (obboxWidth * ar);
                var ogHeight = (obboxHeight * oAr);

                ProjectionMatrix = Matrix4x4.CreateOrthographic(
                    (float)ogWidth * OrthoZoom,
                    (float)ogHeight * OrthoZoom,
                    Near, Far);
            }
        }

        #region FovDeg
        private float _FovDeg = 0;

        public static readonly DirectProperty<OpenGlControl, float> FovDegProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("FovDeg", o => o.FovDeg, (o, v) => o.FovDeg = v);

        public float FovDeg
        {
            get => _FovDeg;
            set
            {
                var changed = value != _FovDeg;
                SetAndRaise(FovDegProperty, ref _FovDeg, value);
                if (changed) RebuildProjectionMatrix();
            }
        }
        #endregion    

        #region Near
        private float _Near = 0;

        public static readonly DirectProperty<OpenGlControl, float> NearProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Near", o => o.Near, (o, v) => o.Near = v);

        public float Near
        {
            get => _Near;
            set
            {
                var changed = value != _Near;
                SetAndRaise(NearProperty, ref _Near, value);
                if (changed) RebuildProjectionMatrix();
            }
        }
        #endregion    

        #region Far
        private float _Far = 0;

        public static readonly DirectProperty<OpenGlControl, float> FarProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Far", o => o.Far, (o, v) => o.Far = v);

        public float Far
        {
            get => _Far;
            set
            {
                var changed = value != _Far;
                SetAndRaise(FarProperty, ref _Far, value);
                if (changed) RebuildProjectionMatrix();
            }
        }
        #endregion

        #region Perspective
        private bool _Perspective = true;

        public static readonly DirectProperty<OpenGlControl, bool> PerspectiveProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, bool>("Perspective", o => o.Perspective, (o, v) => o.Perspective = v);

        public bool Perspective
        {
            get => _Perspective;
            set
            {
                var changed = value != _Perspective;
                SetAndRaise(PerspectiveProperty, ref _Perspective, value);
                if (changed) RebuildProjectionMatrix();
            }
        }
        #endregion

        #region OrthoZoom
        private float _OrthoZoom = 1f;

        public static readonly DirectProperty<OpenGlControl, float> OrthoZoomProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("OrthoZoom", o => o.OrthoZoom, (o, v) => o.OrthoZoom = v);

        public float OrthoZoom
        {
            get => _OrthoZoom;
            set
            {
                var changed = value != _OrthoZoom;
                SetAndRaise(OrthoZoomProperty, ref _OrthoZoom, value);
                if (changed) RebuildProjectionMatrix();
            }
        }
        #endregion

        /// <summary>
        /// set projection matrix all at once avoiding rebuild over property changed
        /// </summary>        
        public void SetProjectionMatrix(float fovDeg, float near, float far, bool perspective, float orthoZoom = 1f)
        {
            SetAndRaise(FovDegProperty, ref _FovDeg, fovDeg);
            SetAndRaise(NearProperty, ref _Near, near);
            SetAndRaise(FarProperty, ref _Far, far);
            SetAndRaise(PerspectiveProperty, ref _Perspective, perspective);
            SetAndRaise(OrthoZoomProperty, ref _OrthoZoom, orthoZoom);

            RebuildProjectionMatrix();
        }

        // ==[ ]===================================================================================

        #region ShowOrbit
        private bool _ShowOrbit = true;

        public static readonly DirectProperty<OpenGlControl, bool> ShowOrbitProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, bool>("ShowOrbit", o => o.ShowOrbit, (o, v) => o.ShowOrbit = v);

        public bool ShowOrbit
        {
            get => _ShowOrbit;
            set => SetAndRaise(ShowOrbitProperty, ref _ShowOrbit, value);
        }
        #endregion

        #region OrbitShowCross
        private bool _OrbitShowCross = false;

        public static readonly DirectProperty<OpenGlControl, bool> OrbitShowCrossProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, bool>("OrbitShowCross", o => o.OrbitShowCross, (o, v) => o.OrbitShowCross = v);

        public bool OrbitShowCross
        {
            get => _OrbitShowCross;
            set => SetAndRaise(OrbitShowCrossProperty, ref _OrbitShowCross, value);
        }
        #endregion    

        internal bool ShiftKeyPressed = false;

        public DateTime middleButtonPressTimestamp = DateTime.MinValue;

        public OpenGlControl()
        {
            this.RenderTransform = new ScaleTransform(1, -1);
        }

        void ControlPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            ctl.Focus();

            Model.FocusedControl = ctl;

            var cp = e.GetCurrentPoint(ctl);
            EvalOrbitPointerPressed(cp);

            if (cp.Properties.IsMiddleButtonPressed)
            {
                if ((DateTime.Now - middleButtonPressTimestamp).TotalMilliseconds <= Model.Options.DoubleClickMs)
                {
                    ZoomFit();
                }
                else if (panPressPosition == null)
                {
                    panPressPosition = cp;
                    panPressMatrixSource = new GlControlMatrixSource(this);
                }
                middleButtonPressTimestamp = DateTime.Now;
            }
        }

        void ControlPointerMoved(object sender, PointerEventArgs e)
        {
            var ctl = sender as OpenGlControl;
            var p = e.GetCurrentPoint(ctl);
            EvalPanPointerMoved(p);
            EvalOrbitPointerMoved(p);
        }

        void ControlPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            var ctl = sender as OpenGlControl;
            var p = e.GetCurrentPoint(ctl);
            EvalPanPointerReleased(p);
            EvalOrbitPointerReleased(p);
        }

        void ControlPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            var ctl = sender as OpenGlControl;
            EvalZoomChanged(e.Delta.Y);
        }

        void ControlGotFocus(object sender, GotFocusEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            Model.FocusedControl = ctl;
        }

        void ControlKeyDown(object sender, KeyEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            ctl.ShiftKeyPressed = e.Key == Key.LeftShift || e.Key == Key.RightShift;
        }

        void ControlKeyUp(object sender, KeyEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            ShiftKeyPressed = false;
        }

        protected virtual void OnModelAttached() { }

        protected virtual void OnDetachModel() { }

        public override void Render(DrawingContext context)
        {
            if (Model == null) return;

            var scaling = VisualRoot.RenderScaling;
            var ps = new PixelSize(
                Max(1, (int)(Bounds.Width * scaling)),
                Max(1, (int)(Bounds.Height * scaling)));

            Model.RenderToGlControl(this, context, ps);
        }

    }

}