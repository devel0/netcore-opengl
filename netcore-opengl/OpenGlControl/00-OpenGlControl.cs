using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using static System.Math;
using System.Numerics;
using Avalonia.Input;
using SearchAThing;
using System;
using System.Reactive;
using System.Runtime.CompilerServices;

namespace SearchAThing
{

    public partial class OpenGlControl : Control
    {

        // vars ========================================================================================
        bool initialized = false;

        // props =======================================================================================

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

        #region TranslationX
        private float _TranslationX = 0;

        public static readonly DirectProperty<OpenGlControl, float> TranslationXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("TranslationX", o => o.TranslationX, (o, v) => o.TranslationX = v);

        public float TranslationX
        {
            get => _TranslationX;
            set => SetAndRaise(TranslationXProperty, ref _TranslationX, value);
        }
        #endregion    

        #region TranslationY
        private float _TranslationY = 0;

        public static readonly DirectProperty<OpenGlControl, float> TranslationYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("TranslationY", o => o.TranslationY, (o, v) => o.TranslationY = v);

        public float TranslationY
        {
            get => _TranslationY;
            set => SetAndRaise(TranslationYProperty, ref _TranslationY, value);
        }
        #endregion    

        #region TranslationZ
        private float _TranslationZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> TranslationZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("TranslationZ", o => o.TranslationZ, (o, v) => o.TranslationZ = v);

        public float TranslationZ
        {
            get => _TranslationZ;
            set => SetAndRaise(TranslationZProperty, ref _TranslationZ, value);
        }
        #endregion    

        Vector3 Translation
        {
            get { return new Vector3(TranslationX, TranslationY, TranslationZ); }
            set
            {
                TranslationX = value.X;
                TranslationY = value.Y;
                TranslationZ = value.Z;
            }
        }

        internal Matrix4x4 RotationMatrix = Matrix4x4.Identity;
        // backing field to distinguish from RotationXYZ dp change
        internal float RotationMatrixX = 0f;
        internal float RotationMatrixY = 0f;
        internal float RotationMatrixZ = 0f;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RebuildRotationMatrix() => RotationMatrix =
            Matrix4x4.CreateFromYawPitchRoll(RotationMatrixY.ToRad(), RotationMatrixX.ToRad(), RotationMatrixZ.ToRad());

        #region RotationX
        private float _RotationX = 0;

        public static readonly DirectProperty<OpenGlControl, float> RotationXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("RotationX", o => o.RotationX, (o, v) => o.RotationX = v);

        /// <summary>
        /// rotation around Xaxis (deg)
        /// </summary>
        public float RotationX
        {
            get => _RotationX;
            set
            {
                if (value != RotationMatrixX)
                {
                    RotationMatrixX = value;
                    RebuildRotationMatrix();
                }
                SetAndRaise(RotationXProperty, ref _RotationX, value);
            }
        }
        #endregion    

        #region RotationY
        private float _RotationY = 0;

        public static readonly DirectProperty<OpenGlControl, float> RotationYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("RotationY", o => o.RotationY, (o, v) => o.RotationY = v);

        /// <summary>
        /// rotation around Yaxis (deg)
        /// </summary>
        public float RotationY
        {
            get => _RotationY;
            set
            {
                if (value != RotationMatrixY)
                {
                    RotationMatrixY = value;
                    RebuildRotationMatrix();
                }
                SetAndRaise(RotationYProperty, ref _RotationY, value);
            }
        }
        #endregion    

        #region RotationZ
        private float _RotationZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> RotationZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("RotationZ", o => o.RotationZ, (o, v) => o.RotationZ = v);

        /// <summary>
        /// rotation around Zaxis (deg)
        /// </summary>
        public float RotationZ
        {
            get => _RotationZ;
            set
            {
                if (value != RotationMatrixZ)
                {
                    RotationMatrixZ = value;
                    RebuildRotationMatrix();
                }
                SetAndRaise(RotationZProperty, ref _RotationZ, value);
            }
        }
        #endregion    

        internal Matrix4x4 OrbitPressRotationMatrix = Matrix4x4.Identity;

        public Vector3 CameraPos
        {
            get { return new Vector3(CameraPosX, CameraPosY, CameraPosZ); }
            set
            {
                CameraPosX = value.X;
                CameraPosY = value.Y;
                CameraPosZ = value.Z;
            }
        }

        internal Vector3 OrbitPressCameraPos = new Vector3();

        #region CameraPosX
        private float _CameraPosX = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraPosXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraPosX", o => o.CameraPosX, (o, v) => o.CameraPosX = v);

        public float CameraPosX
        {
            get => _CameraPosX;
            set => SetAndRaise(CameraPosXProperty, ref _CameraPosX, value);
        }
        #endregion    

        #region CameraPosY
        private float _CameraPosY = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraPosYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraPosY", o => o.CameraPosY, (o, v) => o.CameraPosY = v);

        public float CameraPosY
        {
            get => _CameraPosY;
            set => SetAndRaise(CameraPosYProperty, ref _CameraPosY, value);
        }
        #endregion         

        #region CameraPosZ
        private float _CameraPosZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraPosZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraPosZ", o => o.CameraPosZ, (o, v) => o.CameraPosZ = v);

        public float CameraPosZ
        {
            get => _CameraPosZ;
            set => SetAndRaise(CameraPosZProperty, ref _CameraPosZ, value);
        }
        #endregion

        public Vector3 CameraTarget
        {
            get { return new Vector3(CameraTargetX, CameraTargetY, CameraTargetZ); }
            set
            {
                CameraTargetX = value.X;
                CameraTargetY = value.Y;
                CameraTargetZ = value.Z;
            }
        }

        internal Vector3 OrbitPressTargetCameraPos = new Vector3();

        #region CameraTargetX
        private float _CameraTargetX = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraTargetXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraTargetX", o => o.CameraTargetX, (o, v) => o.CameraTargetX = v);

        public float CameraTargetX
        {
            get => _CameraTargetX;
            set => SetAndRaise(CameraTargetXProperty, ref _CameraTargetX, value);
        }
        #endregion    

        #region CameraTargetY
        private float _CameraTargetY = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraTargetYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraTargetY", o => o.CameraTargetY, (o, v) => o.CameraTargetY = v);

        public float CameraTargetY
        {
            get => _CameraTargetY;
            set => SetAndRaise(CameraTargetYProperty, ref _CameraTargetY, value);
        }
        #endregion    

        #region CameraTargetZ
        private float _CameraTargetZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraTargetZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraTargetZ", o => o.CameraTargetZ, (o, v) => o.CameraTargetZ = v);

        public float CameraTargetZ
        {
            get => _CameraTargetZ;
            set => SetAndRaise(CameraTargetZProperty, ref _CameraTargetZ, value);
        }
        #endregion

        public Vector3 CameraUp
        {
            get { return new Vector3(CameraUpX, CameraUpY, CameraUpZ); }
            set
            {
                CameraUpX = value.X;
                CameraUpY = value.Y;
                CameraUpZ = value.Z;
            }
        }

        internal Vector3 OrbitPressCameraUp = new Vector3();

        #region CameraUpX
        private float _CameraUpX = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraUpXProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraUpX", o => o.CameraUpX, (o, v) => o.CameraUpX = v);

        public float CameraUpX
        {
            get => _CameraUpX;
            set => SetAndRaise(CameraUpXProperty, ref _CameraUpX, value);
        }
        #endregion    

        #region CameraUpY
        private float _CameraUpY = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraUpYProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraUpY", o => o.CameraUpY, (o, v) => o.CameraUpY = v);

        public float CameraUpY
        {
            get => _CameraUpY;
            set => SetAndRaise(CameraUpYProperty, ref _CameraUpY, value);
        }
        #endregion    

        #region CameraUpZ
        private float _CameraUpZ = 0;

        public static readonly DirectProperty<OpenGlControl, float> CameraUpZProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("CameraUpZ", o => o.CameraUpZ, (o, v) => o.CameraUpZ = v);

        public float CameraUpZ
        {
            get => _CameraUpZ;
            set => SetAndRaise(CameraUpZProperty, ref _CameraUpZ, value);
        }
        #endregion

        #region FovDeg
        private float _FovDeg = 0;

        public static readonly DirectProperty<OpenGlControl, float> FovDegProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("FovDeg", o => o.FovDeg, (o, v) => o.FovDeg = v);

        public float FovDeg
        {
            get => _FovDeg;
            set => SetAndRaise(FovDegProperty, ref _FovDeg, value);
        }
        #endregion    

        #region Near
        private float _Near = 0;

        public static readonly DirectProperty<OpenGlControl, float> NearProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Near", o => o.Near, (o, v) => o.Near = v);

        public float Near
        {
            get => _Near;
            set => SetAndRaise(NearProperty, ref _Near, value);
        }
        #endregion    

        #region Far
        private float _Far = 0;

        public static readonly DirectProperty<OpenGlControl, float> FarProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Far", o => o.Far, (o, v) => o.Far = v);

        public float Far
        {
            get => _Far;
            set => SetAndRaise(FarProperty, ref _Far, value);
        }
        #endregion

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

        #region Perspective
        private bool _Perspective = true;

        public static readonly DirectProperty<OpenGlControl, bool> PerspectiveProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, bool>("Perspective", o => o.Perspective, (o, v) => o.Perspective = v);

        public bool Perspective
        {
            get => _Perspective;
            set => SetAndRaise(PerspectiveProperty, ref _Perspective, value);
        }
        #endregion

        #region OrthoZoom
        private float _OrthoZoom = 1f;

        public static readonly DirectProperty<OpenGlControl, float> OrthoZoomProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("OrthoZoom", o => o.OrthoZoom, (o, v) => o.OrthoZoom = v);

        public float OrthoZoom
        {
            get => _OrthoZoom;
            set => SetAndRaise(OrthoZoomProperty, ref _OrthoZoom, value);
        }
        #endregion

        public OrbitGeometry orbitGeometry { get; private set; } = new OrbitGeometry();

        OrbitFunction orbitFunction = OrbitFunction.None;

        internal bool ShiftKeyPressed = false;

        public PointerPoint? panPressPosition { get; private set; }
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
                }
                middleButtonPressTimestamp = DateTime.Now;
            }
        }

        void ControlPointerMoved(object sender, PointerEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            if (panPressPosition != null)
            {
                // TODO:
            }
            EvalOrbitPointerMoved(e.GetCurrentPoint(ctl));
        }

        void ControlPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            EvalOrbitPointerReleased(e.GetCurrentPoint(ctl));

            panPressPosition = null;
        }

        void ControlPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            if (e.Delta.Y > 0)
                ctl.ZoomOut();
            else
                ctl.ZoomIn();
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