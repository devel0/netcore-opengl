using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using static System.Math;
using System.Numerics;
using Avalonia.Input;
using SearchAThing;
using System;
using System.Reactive;

namespace SearchAThing
{

    public partial class OpenGlControl : Control
    {

        #region vars ========================================================================================
        bool initialized = false;

        #endregion

        #region props =======================================================================================

        #region Model
        private OpenGlModelBase _Model = null;

        public static readonly DirectProperty<OpenGlControl, OpenGlModelBase> ModelProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, OpenGlModelBase>("Model", o => o.Model, (o, v) => o.Model = v);

        public OpenGlModelBase Model
        {
            get => _Model;
            set
            {
                System.Console.WriteLine($"OpenGlCtl: FocusedCtl");
                var attachModel = false;
                if (_Model != value)
                {
                    if (_Model != null)
                    {
                        Model.Deregister(this);
                        this.PointerPressed -= CtlPointerPressed;
                        this.GotFocus -= CtlGotFocus;
                        OnDetachModel();
                    }
                    attachModel = true;
                }
                SetAndRaise(ModelProperty, ref _Model, value);
                if (attachModel)
                {
                    Model.Register(this);
                    this.PointerPressed += CtlPointerPressed;
                    this.GotFocus += CtlGotFocus;
                    OnModelAttached();
                }
            }
        }
        #endregion       

        #region Yaw
        private float _Yaw = 0;

        public static readonly DirectProperty<OpenGlControl, float> YawProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Yaw", o => o.Yaw, (o, v) => o.Yaw = v);

        public float Yaw
        {
            get => _Yaw;
            set => SetAndRaise(YawProperty, ref _Yaw, value);
        }
        #endregion    

        #region Pitch
        private float _Pitch = 0;

        public static readonly DirectProperty<OpenGlControl, float> PitchProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Pitch", o => o.Pitch, (o, v) => o.Pitch = v);

        public float Pitch
        {
            get => _Pitch;
            set => SetAndRaise(PitchProperty, ref _Pitch, value);
        }
        #endregion    

        #region Roll
        private float _Roll = 0;

        public static readonly DirectProperty<OpenGlControl, float> RollProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Roll", o => o.Roll, (o, v) => o.Roll = v);

        public float Roll
        {
            get => _Roll;
            set => SetAndRaise(RollProperty, ref _Roll, value);
        }
        #endregion

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

        #region Fov
        private float _Fov = 0;

        public static readonly DirectProperty<OpenGlControl, float> FovProperty =
            AvaloniaProperty.RegisterDirect<OpenGlControl, float>("Fov", o => o.Fov, (o, v) => o.Fov = v);

        public float Fov
        {
            get => _Fov;
            set => SetAndRaise(FovProperty, ref _Fov, value);
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

        #endregion        

        public OpenGlControl()
        {
            this.RenderTransform = new ScaleTransform(1, -1);
        }

        void CtlPointerPressed(object sender, PointerPressedEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            ctl.Focus();

            Model.FocusedCtl = ctl;
        }

        void CtlGotFocus(object sender, GotFocusEventArgs e)
        {
            var ctl = sender as OpenGlControl;

            Model.FocusedCtl = ctl;
        }

        protected virtual void OnModelAttached() { }

        protected virtual void OnDetachModel() { }

        public virtual Matrix4x4 GetModelMatrix()
        {
            var yaw = Yaw.ToRad();
            var pitch = Pitch.ToRad();
            var roll = Roll.ToRad();

            return Matrix4x4.CreateFromYawPitchRoll(yaw, pitch, roll);
        }

        public virtual Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(CameraPos, CameraTarget, CameraUp);
        }

        public virtual Matrix4x4 GetProjectionMatrix()
        {
            var aspectRatio = (float)(Bounds.Width / Bounds.Height);

            return Matrix4x4.CreatePerspectiveFieldOfView(Fov.ToRad(), aspectRatio, Near, Far);
        }

        public override void Render(DrawingContext context)
        {
            if (Model == null) return;

            //System.Console.WriteLine($"render ctl:{Name}");

            var scaling = VisualRoot.RenderScaling;
            var ps = new PixelSize(
                Max(1, (int)(Bounds.Width * scaling)),
                Max(1, (int)(Bounds.Height * scaling)));

            Model.RenderToGlControl(this, context, ps);
        }

    }

}