using Avalonia;
using System.Numerics;
using Avalonia.Threading;
using System.Text;
using Avalonia.Input;
using static System.Math;
using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace SearchAThing.SciExamples
{

    public partial class SampleGlControl : OpenGlControl
    {

        // vars ==========================================================================================

        public PointerPoint? pointerPressPosition { get; private set; }
        public PointerPoint? pointerMovedPosition { get; private set; }

        public Vector3 WorldPosMin => new Vector3(-10000f, -10000, -10000);
        public Vector3 WorldPosMax => new Vector3(10000f, 10000, 10000);

        public Vector3 CameraPosMin => new Vector3(-1000f, -1000, -1000);
        public Vector3 CameraPosMax => new Vector3(1000f, 1000, 1000);

        public Vector3 FovNearFarMin => new Vector3(0.1f, 1e-4f, 1e-4f);
        public Vector3 FovNearFarMax => new Vector3(179.9f, 1e6f, 1e6f);

        public Vector3 Vector3Zero => new Vector3(0f, 0, 0);
        public Vector3 Vector3One => new Vector3(1, 1, 1);
        public Vector3 Vector3OneNeg => new Vector3(-1, -1, -1);
        public Vector3 Vector3_360 => new Vector3(360, 360, 360);
        public Vector3 Vector3_180 => new Vector3(180, 180, 180);
        public Vector3 Vector3_180Neg => new Vector3(-180, -180, -180);

        public const float FOV_DEFAULT = 45f;
        public const float NEAR_DEFAULT = 10f;
        public const float FAR_DEFAULT = 100000f;

        static readonly Vector3 OBJ_COLOR_DEFAUILT = new Vector3(0.3f, 0.7f, 1);
        static readonly float AMBIENT_DEFAULT = 0.6f;
        static readonly float ALPHA_DEFAULT = 0.7f;

        // props =========================================================================================       

        #region ShowModel        
        private bool _ShowModel = true;

        public static readonly DirectProperty<SampleGlControl, bool> ShowModelProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, bool>("ShowModel", o => o.ShowModel, (o, v) => o.ShowModel = v);

        public bool ShowModel
        {
            get => _ShowModel;
            set => SetAndRaise(ShowModelProperty, ref _ShowModel, value);
        }
        #endregion

        #region ShowModelBBox      
        private bool _ShowModelBBox = false;

        public static readonly DirectProperty<SampleGlControl, bool> ShowModelBBoxProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, bool>("ShowModelBBox", o => o.ShowModelBBox, (o, v) => o.ShowModelBBox = v);

        public bool ShowModelBBox
        {
            get => _ShowModelBBox;
            set => SetAndRaise(ShowModelBBoxProperty, ref _ShowModelBBox, value);
        }
        #endregion

        internal Task AnimTask = null;

        internal void StartAnim()
        {
            if (AnimTask != null) return;

            AnimTask = Task.Run(async () =>
            {
                while (true)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        CurrentTime = DateTime.Now;
                    });
                    var model = (Model as SampleGlModel);
                    await Task.Delay((int)model.BuildModelRefreshTimeLapse.TotalMilliseconds);
                }

            });
        }

        #region CurrentTime        
        private DateTime _CurrentTime;

        public static readonly DirectProperty<SampleGlControl, DateTime> CurrentTimeProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, DateTime>("CurrentTime", o => o.CurrentTime, (o, v) => o.CurrentTime = v);

        public DateTime CurrentTime
        {
            get => _CurrentTime;
            set => SetAndRaise(CurrentTimeProperty, ref _CurrentTime, value);
        }
        #endregion

        #region Wireframe        
        private bool _Wireframe;

        public static readonly DirectProperty<SampleGlControl, bool> WireframeProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, bool>("Wireframe", o => o.Wireframe, (o, v) => o.Wireframe = v);

        public bool Wireframe
        {
            get => _Wireframe;
            set => SetAndRaise(WireframeProperty, ref _Wireframe, value);
        }
        #endregion       

        public Vector3 ObjColor
        {
            get { return new Vector3(ObjColorRed, ObjColorGreen, ObjColorBlue); }
            set
            {
                ObjColorRed = value.X;
                ObjColorGreen = value.Y;
                ObjColorBlue = value.Z;
            }
        }

        #region ObjColorRed
        private float _ObjColorRed = 0f;

        public static readonly DirectProperty<SampleGlControl, float> ObjColorRedProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("ObjColorRed", o => o.ObjColorRed, (o, v) => o.ObjColorRed = v);

        public float ObjColorRed
        {
            get => _ObjColorRed;
            set => SetAndRaise(ObjColorRedProperty, ref _ObjColorRed, value);
        }
        #endregion    

        #region ObjColorGreen
        private float _ObjColorGreen = 0f;

        public static readonly DirectProperty<SampleGlControl, float> ObjColorGreenProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("ObjColorGreen", o => o.ObjColorGreen, (o, v) => o.ObjColorGreen = v);

        public float ObjColorGreen
        {
            get => _ObjColorGreen;
            set => SetAndRaise(ObjColorGreenProperty, ref _ObjColorGreen, value);
        }
        #endregion    

        #region ObjColorBlue
        private float _ObjColorBlue = 0f;

        public static readonly DirectProperty<SampleGlControl, float> ObjColorBlueProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("ObjColorBlue", o => o.ObjColorBlue, (o, v) => o.ObjColorBlue = v);

        public float ObjColorBlue
        {
            get => _ObjColorBlue;
            set => SetAndRaise(ObjColorBlueProperty, ref _ObjColorBlue, value);
        }
        #endregion

        public Vector3 LightPos
        {
            get { return new Vector3(LightPosX, LightPosY, LightPosZ); }
            set
            {
                LightPosX = value.X;
                LightPosY = value.Y;
                LightPosZ = value.Z;
            }
        }

        #region LightPosX
        private float _LightPosX = 0f;

        public static readonly DirectProperty<SampleGlControl, float> LightPosXProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("LightPosX", o => o.LightPosX, (o, v) => o.LightPosX = v);

        public float LightPosX
        {
            get => _LightPosX;
            set => SetAndRaise(LightPosXProperty, ref _LightPosX, value);
        }
        #endregion    

        #region LightPosY
        private float _LightPosY = 0;

        public static readonly DirectProperty<SampleGlControl, float> LightPosYProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("LightPosY", o => o.LightPosY, (o, v) => o.LightPosY = v);

        public float LightPosY
        {
            get => _LightPosY;
            set => SetAndRaise(LightPosYProperty, ref _LightPosY, value);
        }
        #endregion    

        #region LightPosZ
        private float _LightPosZ = 0;

        public static readonly DirectProperty<SampleGlControl, float> LightPosZProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("LightPosZ", o => o.LightPosZ, (o, v) => o.LightPosZ = v);

        public float LightPosZ
        {
            get => _LightPosZ;
            set => SetAndRaise(LightPosZProperty, ref _LightPosZ, value);
        }
        #endregion             

        #region Ambient        
        private float _Ambient = AMBIENT_DEFAULT;

        public static readonly DirectProperty<SampleGlControl, float> AmbientProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("Ambient", o => o.Ambient, (o, v) => o.Ambient = v);

        public float Ambient
        {
            get => _Ambient;
            set => SetAndRaise(AmbientProperty, ref _Ambient, value);
        }
        #endregion

        #region Alpha        
        private float _Alpha = ALPHA_DEFAULT;

        public static readonly DirectProperty<SampleGlControl, float> AlphaProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("Alpha", o => o.Alpha, (o, v) => o.Alpha = v);

        public float Alpha
        {
            get => _Alpha;
            set => SetAndRaise(AlphaProperty, ref _Alpha, value);
        }
        #endregion

        #region Info        
        private string _Info = "";

        public static readonly DirectProperty<SampleGlControl, string> InfoProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, string>("Info", o => o.Info, (o, v) => o.Info = v);

        public string Info
        {
            get => _Info;
            set => SetAndRaise(InfoProperty, ref _Info, value);
        }
        #endregion

        #region CurrentWorldCoord
        private Vector3D _CurrentWorldCoord = Vector3D.Zero;

        public static readonly DirectProperty<SampleGlControl, Vector3D> CurrentWorldCoordProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, Vector3D>("CurrentWorldCoord", o => o.CurrentWorldCoord, (o, v) => o.CurrentWorldCoord = v);

        public Vector3D CurrentWorldCoord
        {
            get => _CurrentWorldCoord;
            set
            {
                SetAndRaise(CurrentWorldCoordProperty, ref _CurrentWorldCoord, value);
                CurrentWorldCoordStr = value.ToString();
            }
        }
        #endregion    

        #region CurrentWorldCoordStr
        private string _CurrentWorldCoordStr = "";

        public static readonly DirectProperty<SampleGlControl, string> CurrentWorldCoordStrProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, string>("CurrentWorldCoordStr", o => o.CurrentWorldCoordStr, (o, v) => o.CurrentWorldCoordStr = v);

        public string CurrentWorldCoordStr
        {
            get => _CurrentWorldCoordStr;
            set => SetAndRaise(CurrentWorldCoordStrProperty, ref _CurrentWorldCoordStr, value);
        }
        #endregion    

        //Subject<string> CurrentWorldCoordSubj = new Subject<string>();

        Action<SampleGlControl> onModelAttached = null;

        public SampleGlControl(Action<SampleGlControl> onModelAttached = null)
        {
            this.onModelAttached = onModelAttached;

            AffectsRender<SampleGlControl>(
                // changing this property time will affect render and inform to refresh display
                CurrentTimeProperty,
                WireframeProperty, ShowModelProperty, ShowModelBBoxProperty, ShowOrbitProperty, OrbitShowCrossProperty,
                ModelMatrixProperty,
                PerspectiveProperty,
                CameraPosXProperty, CameraPosYProperty, CameraPosZProperty,
                CameraTargetXProperty, CameraTargetYProperty, CameraTargetZProperty,
                CameraUpXProperty, CameraUpYProperty, CameraUpZProperty,
                FovDegProperty, NearProperty, FarProperty,
                OrthoZoomProperty,
                ObjColorRedProperty, ObjColorGreenProperty, ObjColorBlueProperty,
                LightPosXProperty, LightPosYProperty, LightPosZProperty,
                AmbientProperty, AlphaProperty);
        }

        // events ========================================================================================

        void pointerPressed(object sender, PointerPressedEventArgs e)
        {
            pointerPressPosition = e.GetCurrentPoint(this);
            UpdateInfo();

            //Model.InvalidateAllControls();
        }

        void pointerMoved(object sender, PointerEventArgs e)
        {
            pointerMovedPosition = e.GetCurrentPoint(this);
            UpdateInfo();

            Model.InvalidateAllControls();
        }

        void pointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            var Model = this.Model as SampleGlModel;


        }

        // init/deinit ====================================================================================

        bool modelAttachedInitDone = false;
        protected override void OnModelAttached()
        {
            this.PointerPressed += pointerPressed;
            this.PointerMoved += pointerMoved;
            this.PointerWheelChanged += pointerWheelChanged;

            this.EffectiveViewportChanged += (a, b) =>
            {
                if (Bounds.Width != 0 && Bounds.Height != 0)
                {
                    if (!modelAttachedInitDone)
                    {
                        Reset(true);

                        InvalidateVisual();
                        modelAttachedInitDone = true;
                    }
                }
            };
            this.AttachedToVisualTree += (a, b) =>
            {

            };
        }

        protected override void OnDetachModel()
        {
            this.PointerWheelChanged -= pointerWheelChanged;
            this.PointerMoved -= pointerMoved;
            this.PointerPressed -= pointerPressed;
        }

        public void Reset(bool execOnModelAttach = false)
        {
            var Model = this.Model as SampleGlModel;

            Dispatcher.UIThread.Post(() =>
            {
                // model                
                ResetRotation();

                // view
                var ct = new Vector3();
                var cp = ct + (Vector3)(Vector3D.ZAxis * Model.vtxMgrBBox.Size.Length);
                var cu = new Vector3(0, 1, 0);
                SetViewMatrix(cp, cp, cu);
                SetProjectionMatrix(FOV_DEFAULT, NEAR_DEFAULT, FAR_DEFAULT, perspective: true);

                // aspect                                
                ObjColor = OBJ_COLOR_DEFAUILT;
                LightPos = Model.vtxMgrBBox.Min - Model.vtxMgrBBox.Size.X * Vector3D.XAxis + Model.vtxMgrBBox.Size.Z * 5 * Vector3D.ZAxis;
                Ambient = AMBIENT_DEFAULT;

                ViewFront();

                if (execOnModelAttach && this.onModelAttached != null) this.onModelAttached(this);
            });
        }

        public void UpdateInfo()
        {
            var Model = this.Model as SampleGlModel;

            var curNDC = new Line3D(Vector3D.Zero, Vector3D.Zero);
            var rayLine = new Line3D(Vector3D.Zero, Vector3D.Zero);

            Dispatcher.UIThread.Post(() =>
            {
                var sb = new StringBuilder();

                sb.AppendLine("SCREEN");
                sb.AppendLine($"    w x h: {Bounds.Width} x {Bounds.Height} (aspect:{Round(Bounds.Width / Bounds.Height, 3)})");
                sb.AppendLine($"   curPos: {pointerMovedPosition?.Position.Eval(k => string.Format("{0:0.0},{1:0.0}", k.X, k.Y))}");
                sb.AppendLine($"   clkPos: {pointerPressPosition?.Position.Eval(k => string.Format("{0:0.0},{1:0.0}", k.X, k.Y))}");
                sb.AppendLine($"MODEL");
                sb.AppendLine($"      min: {Model.vtxMgrBBox.Min}");
                sb.AppendLine($"      max: {Model.vtxMgrBBox.Max}");
                sb.AppendLine($" pts/idxs: {string.Format("{0,9:0}", Model.VtxMgr.Points)}/{string.Format("{0,9:0}", Model.VtxMgr.Idxs.Count)}");

                Info = sb.ToString();
            });
        }

    }

}