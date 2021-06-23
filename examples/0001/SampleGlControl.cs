using Avalonia;

namespace SearchAThing.SciExamples
{

    public class SampleGlControl : OpenGlControl
    {

        #region ObjColorRed
        private float _ObjColorRed = 0f;

        public static readonly DirectProperty<SampleGlControl, float> ObjColorRedProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("ObjColorRed", o => o.ObjColorRed, (o, v) => o.ObjColorRed = v);

        public float ObjColorRed
        {
            get => _ObjColorRed;
            set
            {
                SetAndRaise(ObjColorRedProperty, ref _ObjColorRed, value);
            }
        }
        #endregion    

        #region ObjColorGreen
        private float _ObjColorGreen = 1f;

        public static readonly DirectProperty<SampleGlControl, float> ObjColorGreenProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("ObjColorGreen", o => o.ObjColorGreen, (o, v) => o.ObjColorGreen = v);

        public float ObjColorGreen
        {
            get => _ObjColorGreen;
            set
            {
                SetAndRaise(ObjColorGreenProperty, ref _ObjColorGreen, value);
            }
        }
        #endregion    

        #region ObjColorBlue
        private float _ObjColorBlue = 0f;

        public static readonly DirectProperty<SampleGlControl, float> ObjColorBlueProperty =
            AvaloniaProperty.RegisterDirect<SampleGlControl, float>("ObjColorBlue", o => o.ObjColorBlue, (o, v) => o.ObjColorBlue = v);

        public float ObjColorBlue
        {
            get => _ObjColorBlue;
            set
            {
                SetAndRaise(ObjColorBlueProperty, ref _ObjColorBlue, value);
            }
        }
        #endregion

        public SampleGlControl()
        {
            AffectsRender<SampleGlControl>(ObjColorRedProperty, ObjColorGreenProperty, ObjColorBlueProperty);
        }

        protected override void OnModelAttached()
        {
            InvalidateVisual();
        }

    }


}